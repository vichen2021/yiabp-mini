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
    protected IUnitOfWorkManager UnitOfWorkManager { get; }
    protected AbpAuditingOptions Options { get; }
    protected YiAuditLoggingOptions YiOptions { get; }
    protected IAuditLogInfoToAuditLogConverter Converter { get; }

    public AuditingStore(
        IAuditLogRepository auditLogRepository,
        IUnitOfWorkManager unitOfWorkManager,
        IOptions<AbpAuditingOptions> options,
        IOptions<YiAuditLoggingOptions> yiOptions,
        IAuditLogInfoToAuditLogConverter converter)
    {
        AuditLogRepository = auditLogRepository;
        UnitOfWorkManager = unitOfWorkManager;
        Converter = converter;
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
            if (ShouldSave(auditInfo))
            {
                await AuditLogRepository.InsertAsync(await Converter.ConvertAsync(auditInfo));
            }

            await uow.CompleteAsync();
        }
    }

    /// <summary>
    /// 根据统一持久化模式判断当前 ABP 审计聚合是否需要保存
    /// </summary>
    private bool ShouldSave(AuditLogInfo auditInfo)
    {
        return YiOptions.SaveMode switch
        {
            AuditLogSaveMode.Disabled => false,
            AuditLogSaveMode.All => true,
            AuditLogSaveMode.BusinessOperations => auditInfo.Actions.Any(action =>
                action.ExtraProperties.TryGetValue(
                    Yi.Framework.Auditing.Abstractions.OperationMetadataPropertyNames.IsBusinessOperation,
                    out var value) && value is true),
            _ => false
        };
    }
}
