using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Volo.Abp.Auditing;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Uow;
using Yi.Module.AuditLogging.Domain.Entities;
using Yi.Module.AuditLogging.Domain.Repositories;
using Yi.Framework.Core.Helper;

namespace Yi.Module.AuditLogging.Domain;

public class AuditingStore : IAuditingStore, ITransientDependency
{
    public ILogger<AuditingStore> Logger { get; set; }
    protected IAuditLogRepository AuditLogRepository { get; }
    protected IOperationLogRepository OperationLogRepository { get; }
    protected IUnitOfWorkManager UnitOfWorkManager { get; }
    protected AbpAuditingOptions Options { get; }
    protected YiAuditLoggingOptions YiOptions { get; }
    protected IAuditLogInfoToAuditLogConverter Converter { get; }
    protected IOperationLogMapper OperationLogMapper { get; }

    public AuditingStore(
        IAuditLogRepository auditLogRepository,
        IOperationLogRepository operationLogRepository,
        IUnitOfWorkManager unitOfWorkManager,
        IOptions<AbpAuditingOptions> options,
        IOptions<YiAuditLoggingOptions> yiOptions,
        IAuditLogInfoToAuditLogConverter converter,
        IOperationLogMapper operationLogMapper)
    {
        AuditLogRepository = auditLogRepository;
        OperationLogRepository = operationLogRepository;
        UnitOfWorkManager = unitOfWorkManager;
        Converter = converter;
        OperationLogMapper = operationLogMapper;
        Options = options.Value;
        YiOptions = yiOptions.Value;

        Logger = NullLogger<AuditingStore>.Instance;
    }

    public virtual async Task SaveAsync(AuditLogInfo auditInfo)
    {
        if (!Options.HideErrors)
        {
            await SaveLogAsync(auditInfo);
            return;
        }

        try
        {
            await SaveLogAsync(auditInfo);
        }
        catch (Exception ex)
        {
            Logger.LogWarning("Could not save the audit log object: " + Environment.NewLine + auditInfo.ToString());
            Logger.LogException(ex, LogLevel.Error);
        }
    }

    protected virtual async Task SaveLogAsync(AuditLogInfo auditInfo)
    {
        Logger.LogDebug("Yi-请求追踪:" + JsonHelper.ObjToStr(auditInfo, "yyyy-MM-dd HH:mm:ss"));

        using (var uow = UnitOfWorkManager.Begin())
        {
            // 保存详细审计日志（如果配置开启）
            if (YiOptions.SaveAuditLog)
            {
                await AuditLogRepository.InsertAsync(await Converter.ConvertAsync(auditInfo));
            }

            // 映射并保存操作日志
            var operationLog = OperationLogMapper.TryMap(auditInfo);
            if (operationLog != null)
            {
                await OperationLogRepository.InsertAsync(operationLog);
            }

            await uow.CompleteAsync();
        }
    }
}