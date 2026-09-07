using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Net;
using SqlSugar;
using Volo.Abp.Auditing;
using Volo.Abp.Domain.Entities;
using Volo.Abp.MultiTenancy;
using Yi.Framework.Auditing.Abstractions.Enums;
using Yi.Module.AuditLogging.Domain;
using Yi.Module.AuditLogging.Domain.Entities;
using Yi.Module.AuditLogging.Domain.Repositories;
using Yi.Framework.SqlSugarCore.Abstractions;
using Yi.Framework.SqlSugarCore.Repositories;

namespace Yi.Module.AuditLogging.SqlSugarCore.Repositories;

public class SqlSugarCoreAuditLogRepository : SqlSugarRepository<AuditLogAggregateRoot, Guid>, IAuditLogRepository
{
    private static readonly Expression<Func<AuditLogActionEntity, AuditLogAggregateRoot, OperationLogRecord>> OperationLogProjection =
        (action, auditLog) => new OperationLogRecord
        {
            Id = action.Id,
            AuditLogId = auditLog.Id,
            Title = action.OperationTitle,
            OperType = action.OperationType ?? OperEnum.Other,
            RequestMethod = auditLog.HttpMethod,
            OperUser = auditLog.UserName,
            OperIp = auditLog.ClientIpAddress,
            Method = auditLog.HttpMethod + " " + auditLog.Url,
            RequestParam = action.Parameters,
            IsSuccess = (auditLog.Exceptions == null || auditLog.Exceptions == "") &&
                        (auditLog.HttpStatusCode == null || auditLog.HttpStatusCode < 400),
            ErrorMessage = auditLog.Exceptions,
            ExecutionDuration = action.ExecutionDuration ?? 0,
            CreationTime = action.ExecutionTime ?? auditLog.ExecutionTime ?? DateTime.MinValue
        };

    private readonly ICurrentTenant _currentTenant;

    /// <summary>
    /// 创建支持租户隔离操作记录查询的统一审计仓储
    /// </summary>
    public SqlSugarCoreAuditLogRepository(
        ISugarDbContextProvider<ISqlSugarDbContext> sugarDbContextProvider,
        ICurrentTenant currentTenant) : base(sugarDbContextProvider)
    {
        _currentTenant = currentTenant;
    }

    /// <summary>
    /// 重写插入，支持导航属性
    /// </summary>
    /// <param name="insertObj"></param>
    /// <returns></returns>
    public override async Task<bool> InsertAsync(AuditLogAggregateRoot insertObj)
    {

        return await _Db.InsertNav<AuditLogAggregateRoot>(insertObj)
                 .Include(z1 => z1.Actions)
                 //.Include(z1 => z1.EntityChanges).ThenInclude(z2 => z2.PropertyChanges)
                 .ExecuteCommandAsync();
    }

    public virtual async Task<List<AuditLogAggregateRoot>> GetListAsync(
        string? sorting = null,
        int maxResultCount = 50,
        int skipCount = 0,
        DateTime? startTime = null,
        DateTime? endTime = null,
        string? httpMethod = null,
        string? url = null,
        Guid? userId = null,
        string? userName = null,
        string? applicationName = null,
        string? clientIpAddress = null,
        string? correlationId = null,
        int? maxExecutionDuration = null,
        int? minExecutionDuration = null,
        bool? hasException = null,
        HttpStatusCode? httpStatusCode = null,
        bool includeDetails = false)
    {
        var query = await GetListQueryAsync(
            startTime,
            endTime,
            httpMethod,
            url,
            userId,
            userName,
            applicationName,
            clientIpAddress,
            correlationId,
            maxExecutionDuration,
            minExecutionDuration,
            hasException,
            httpStatusCode,
            includeDetails
        );

        var auditLogs = await query
            .OrderBy(sorting.IsNullOrWhiteSpace() ? (nameof(AuditLogAggregateRoot.ExecutionTime) + " DESC") : sorting)
            .ToPageListAsync(skipCount, maxResultCount);

        return auditLogs;
    }

    public virtual async Task<long> GetCountAsync(
        DateTime? startTime = null,
        DateTime? endTime = null,
        string? httpMethod = null,
        string? url = null,
        Guid? userId = null,
        string? userName = null,
        string? applicationName = null,
        string? clientIpAddress = null,
        string? correlationId = null,
        int? maxExecutionDuration = null,
        int? minExecutionDuration = null,
        bool? hasException = null,
        HttpStatusCode? httpStatusCode = null,
        CancellationToken cancellationToken = default)
    {
        var query = await GetListQueryAsync(
            startTime,
            endTime,
            httpMethod,
            url,
            userId,
            userName,
            applicationName,
            clientIpAddress,
            correlationId,
            maxExecutionDuration,
            minExecutionDuration,
            hasException,
            httpStatusCode
        );

        var totalCount = await query.CountAsync();

        return totalCount;
    }

    protected virtual async Task<ISugarQueryable<AuditLogAggregateRoot>> GetListQueryAsync(
        DateTime? startTime = null,
        DateTime? endTime = null,
        string? httpMethod = null,
        string? url = null,
        Guid? userId = null,
        string? userName = null,
        string? applicationName = null,
        string? clientIpAddress = null,
        string? correlationId = null,
        int? maxExecutionDuration = null,
        int? minExecutionDuration = null,
        bool? hasException = null,
        HttpStatusCode? httpStatusCode = null,
        bool includeDetails = false)
    {
        var nHttpStatusCode = (int?)httpStatusCode;
        return _DbQueryable
            .WhereIF(startTime.HasValue, auditLog => auditLog.ExecutionTime >= startTime)
            .WhereIF(endTime.HasValue, auditLog => auditLog.ExecutionTime <= endTime)
            .WhereIF(hasException.HasValue && hasException.Value, auditLog => auditLog.Exceptions != null && auditLog.Exceptions != "")
            .WhereIF(hasException.HasValue && !hasException.Value, auditLog => auditLog.Exceptions == null || auditLog.Exceptions == "")
            .WhereIF(httpMethod != null, auditLog => auditLog.HttpMethod == httpMethod)
            .WhereIF(url != null, auditLog => auditLog.Url != null && auditLog.Url.Contains(url))
            .WhereIF(userId != null, auditLog => auditLog.UserId == userId)
            .WhereIF(userName != null, auditLog => auditLog.UserName == userName)
            .WhereIF(applicationName != null, auditLog => auditLog.ApplicationName == applicationName)
            .WhereIF(clientIpAddress != null, auditLog => auditLog.ClientIpAddress != null && auditLog.ClientIpAddress == clientIpAddress)
            .WhereIF(correlationId != null, auditLog => auditLog.CorrelationId == correlationId)
            .WhereIF(httpStatusCode != null && httpStatusCode > 0, auditLog => auditLog.HttpStatusCode == nHttpStatusCode)
            .WhereIF(maxExecutionDuration != null && maxExecutionDuration.Value > 0, auditLog => auditLog.ExecutionDuration <= maxExecutionDuration)
            .WhereIF(minExecutionDuration != null && minExecutionDuration.Value > 0, auditLog => auditLog.ExecutionDuration >= minExecutionDuration);
    }

    public virtual async Task<Dictionary<DateTime, double>> GetAverageExecutionDurationPerDayAsync(
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        var result = await _DbQueryable
            .Where(a => a.ExecutionTime < endDate.AddDays(1) && a.ExecutionTime > startDate)
            .OrderBy(t => t.ExecutionTime)
            .GroupBy(t => new { t.ExecutionTime!.Value.Date })
            .Select(g => new { Day = SqlFunc.AggregateMin(g.ExecutionTime), avgExecutionTime = SqlFunc.AggregateAvg(g.ExecutionDuration) })
            .ToListAsync();

        return result.ToDictionary(element => element.Day!.Value.ClearTime(), element => (double)element.avgExecutionTime!);
    }


    public virtual async Task<EntityChangeEntity> GetEntityChange(
        Guid entityChangeId,
        CancellationToken cancellationToken = default)
    {
        var entityChange = await (await GetDbContextAsync()).Queryable<EntityChangeEntity>()
                                .Where(x => x.Id == entityChangeId)
                                .OrderBy(x => x.Id)
                                .FirstAsync();

        if (entityChange == null)
        {
            throw new EntityNotFoundException(typeof(EntityChangeEntity));
        }

        return entityChange;
    }

    public virtual async Task<List<EntityChangeEntity>> GetEntityChangeListAsync(
        string? sorting = null,
        int maxResultCount = 50,
        int skipCount = 0,
        Guid? auditLogId = null,
        DateTime? startTime = null,
        DateTime? endTime = null,
        EntityChangeType? changeType = null,
        string? entityId = null,
        string? entityTypeFullName = null,
        bool includeDetails = false,
        CancellationToken cancellationToken = default)
    {
        var query = await GetEntityChangeListQueryAsync(auditLogId, startTime, endTime, changeType, entityId, entityTypeFullName, includeDetails);

        return await query.OrderBy(sorting.IsNullOrWhiteSpace() ? (nameof(EntityChangeEntity.ChangeTime) + " DESC") : sorting)
            .ToPageListAsync(skipCount, maxResultCount);
    }

    public virtual async Task<long> GetEntityChangeCountAsync(
        Guid? auditLogId = null,
        DateTime? startTime = null,
        DateTime? endTime = null,
        EntityChangeType? changeType = null,
        string? entityId = null,
        string? entityTypeFullName = null,
        CancellationToken cancellationToken = default)
    {
        var query = await GetEntityChangeListQueryAsync(auditLogId, startTime, endTime, changeType, entityId, entityTypeFullName);

        var totalCount = await query.CountAsync();

        return totalCount;
    }

    public virtual async Task<EntityChangeWithUsername> GetEntityChangeWithUsernameAsync(
        Guid entityChangeId)
    {
        var auditLog = await _DbQueryable
            .Where(x => x.EntityChanges.Any(y => y.Id == entityChangeId)).FirstAsync();

        return new EntityChangeWithUsername()
        {
            EntityChange = auditLog.EntityChanges.First(x => x.Id == entityChangeId),
            UserName = auditLog.UserName!
        };
    }

    public virtual async Task<List<EntityChangeWithUsername>> GetEntityChangesWithUsernameAsync(
        string entityId,
        string entityTypeFullName,
        CancellationToken cancellationToken = default)
    {
        var dbContext = await GetDbContextAsync();

        var query = dbContext.Queryable<EntityChangeEntity>()
                            .Where(x => x.EntityId == entityId && x.EntityTypeFullName == entityTypeFullName);
        return await query.LeftJoin<AuditLogAggregateRoot>((x, audit) => x.AuditLogId == audit.Id)
             .Select((x, audit) => new EntityChangeWithUsername { EntityChange = x, UserName = audit.UserName! })
                .OrderByDescending(x => x.EntityChange.ChangeTime).ToListAsync();
    }

    protected virtual async Task<ISugarQueryable<EntityChangeEntity>> GetEntityChangeListQueryAsync(
        Guid? auditLogId = null,
        DateTime? startTime = null,
        DateTime? endTime = null,
        EntityChangeType? changeType = null,
        string? entityId = null,
        string? entityTypeFullName = null,
        bool includeDetails = false)
    {
        return (await GetDbContextAsync())
            .Queryable<EntityChangeEntity>()
            .WhereIF(auditLogId.HasValue, e => e.AuditLogId == auditLogId)
            .WhereIF(startTime.HasValue, e => e.ChangeTime >= startTime)
            .WhereIF(endTime.HasValue, e => e.ChangeTime <= endTime)
            .WhereIF(changeType.HasValue, e => e.ChangeType == changeType)
            .WhereIF(!string.IsNullOrWhiteSpace(entityId), e => e.EntityId == entityId)
            .WhereIF(!string.IsNullOrWhiteSpace(entityTypeFullName), e => e.EntityTypeFullName.Contains(entityTypeFullName));
    }

    /// <summary>
    /// 分页查询统一审计模型中的业务操作记录
    /// </summary>
    public async Task<(List<OperationLogRecord> Items, int Total)> GetOperationLogPageAsync(
        OperEnum? operationType,
        string? userName,
        string? title,
        bool? isSuccess,
        DateTime? startTime,
        DateTime? endTime,
        int skipCount,
        int maxResultCount)
    {
        RefAsync<int> total = 0;
        var records = await BuildOperationLogQuery(operationType, userName, title, isSuccess, startTime, endTime)
            .OrderBy((action, auditLog) => action.ExecutionTime, OrderByType.Desc)
            .Select(OperationLogProjection)
            .ToPageListAsync(skipCount, maxResultCount, total);

        return (records, total);
    }

    /// <summary>
    /// 查询符合条件的全部业务操作记录
    /// </summary>
    public async Task<List<OperationLogRecord>> GetOperationLogListAsync(
        OperEnum? operationType,
        string? userName,
        string? title,
        bool? isSuccess,
        DateTime? startTime,
        DateTime? endTime)
    {
        return await BuildOperationLogQuery(operationType, userName, title, isSuccess, startTime, endTime)
            .OrderBy((action, auditLog) => action.ExecutionTime, OrderByType.Desc)
            .Select(OperationLogProjection)
            .ToListAsync();
    }

    /// <summary>
    /// 按 Action 标识查询业务操作详情
    /// </summary>
    public async Task<OperationLogRecord?> GetOperationLogAsync(Guid actionId)
    {
        return await BuildOperationLogQuery(null, null, null, null, null, null)
            .Where((action, auditLog) => action.Id == actionId)
            .Select(OperationLogProjection)
            .FirstAsync();
    }

    /// <summary>
    /// 按业务 Action 标识删除完整审计聚合
    /// </summary>
    public async Task DeleteOperationLogsAsync(IEnumerable<Guid> actionIds)
    {
        var ids = actionIds.Distinct().ToArray();
        if (ids.Length == 0)
        {
            return;
        }

        var auditLogIds = await _Db.Queryable<AuditLogActionEntity>()
            .Where(action => action.TenantId == _currentTenant.Id && action.IsBusinessOperation && ids.Contains(action.Id))
            .Select(action => action.AuditLogId)
            .Distinct()
            .ToListAsync();

        await DeleteAuditLogAggregatesAsync(auditLogIds);
    }

    /// <summary>
    /// 清空当前租户包含业务操作的完整审计聚合
    /// </summary>
    public async Task DeleteAllOperationLogsAsync()
    {
        var auditLogIds = await _Db.Queryable<AuditLogActionEntity>()
            .Where(action => action.TenantId == _currentTenant.Id && action.IsBusinessOperation)
            .Select(action => action.AuditLogId)
            .Distinct()
            .ToListAsync();

        await DeleteAuditLogAggregatesAsync(auditLogIds);
    }

    /// <summary>
    /// 构建租户隔离的业务操作联表查询
    /// </summary>
    private ISugarQueryable<AuditLogActionEntity, AuditLogAggregateRoot> BuildOperationLogQuery(
        OperEnum? operationType,
        string? userName,
        string? title,
        bool? isSuccess,
        DateTime? startTime,
        DateTime? endTime)
    {
        return _Db.Queryable<AuditLogActionEntity, AuditLogAggregateRoot>(
                (action, auditLog) => new JoinQueryInfos(JoinType.Inner, action.AuditLogId == auditLog.Id))
            .Where((action, auditLog) => action.IsBusinessOperation &&
                                         action.TenantId == _currentTenant.Id &&
                                         auditLog.TenantId == _currentTenant.Id)
            .WhereIF(operationType.HasValue, (action, auditLog) => action.OperationType == operationType)
            .WhereIF(!string.IsNullOrWhiteSpace(userName), (action, auditLog) => auditLog.UserName != null && auditLog.UserName.Contains(userName!))
            .WhereIF(!string.IsNullOrWhiteSpace(title), (action, auditLog) => action.OperationTitle != null && action.OperationTitle.Contains(title!))
            .WhereIF(startTime.HasValue, (action, auditLog) => action.ExecutionTime >= startTime)
            .WhereIF(endTime.HasValue, (action, auditLog) => action.ExecutionTime <= endTime)
            .WhereIF(isSuccess == true, (action, auditLog) =>
                (auditLog.Exceptions == null || auditLog.Exceptions == "") &&
                (auditLog.HttpStatusCode == null || auditLog.HttpStatusCode < 400))
            .WhereIF(isSuccess == false, (action, auditLog) =>
                (auditLog.Exceptions != null && auditLog.Exceptions != "") ||
                (auditLog.HttpStatusCode != null && auditLog.HttpStatusCode >= 400));
    }

    /// <summary>
    /// 删除审计根及其全部 Action 和实体变更以保持聚合完整性
    /// </summary>
    private async Task DeleteAuditLogAggregatesAsync(IReadOnlyCollection<Guid> auditLogIds)
    {
        if (auditLogIds.Count == 0)
        {
            return;
        }

        var entityChangeIds = await _Db.Queryable<EntityChangeEntity>()
            .Where(change => auditLogIds.Contains(change.AuditLogId))
            .Select(change => change.Id)
            .ToListAsync();

        if (entityChangeIds.Count > 0)
        {
            await _Db.Deleteable<EntityPropertyChangeEntity>()
                .Where(change => change.EntityChangeId.HasValue && entityChangeIds.Contains(change.EntityChangeId.Value))
                .ExecuteCommandAsync();
        }

        await _Db.Deleteable<EntityChangeEntity>()
            .Where(change => auditLogIds.Contains(change.AuditLogId))
            .ExecuteCommandAsync();
        await _Db.Deleteable<AuditLogActionEntity>()
            .Where(action => auditLogIds.Contains(action.AuditLogId))
            .ExecuteCommandAsync();
        await _Db.Deleteable<AuditLogAggregateRoot>()
            .Where(auditLog => auditLogIds.Contains(auditLog.Id) && auditLog.TenantId == _currentTenant.Id)
            .ExecuteCommandAsync();
    }
}
