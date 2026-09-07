using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Shouldly;
using Volo.Abp.Auditing;
using Volo.Abp.Data;
using Volo.Abp.Security.Claims;
using Xunit;
using Yi.Framework.Auditing.Abstractions;
using Yi.Framework.Auditing.Abstractions.Enums;
using Yi.Framework.SqlSugarCore.Abstractions;
using Yi.Module.AuditLogging.Application.Contracts.Dtos.OperationLog;
using Yi.Module.AuditLogging.Application.Contracts.IServices;
using Yi.Module.AuditLogging.Domain.Entities;
using Yi.Module.AuditLogging.Domain.Repositories;
using Yi.Module.Rbac.Domain.Entities;
using Yi.Module.Rbac.Domain.Repositories;

namespace Yi.Module.Rbac.Test.Auditing;

/// <summary>
/// 统一审计模型操作记录服务集成测试
/// </summary>
public class OperationLogServiceTest : YiModuleRbacTestBase
{
    /// <summary>
    /// 验证详情、导出与按 Action 删除完整审计聚合
    /// </summary>
    [Fact]
    public async Task DetailExportAndDelete_ShouldUseUnifiedAuditAggregate()
    {
        var repository = GetRequiredService<IAuditLogRepository>();
        var service = GetRequiredService<IOperationLogService>();
        var principalAccessor = GetRequiredService<ICurrentPrincipalAccessor>();
        var (_, actionIds) = await CreateAuditLogAsync("添加岗位", OperEnum.Insert, true, 2);

        using (principalAccessor.Change(await CreateHostSuperAdminPrincipalAsync()))
        {
            var detail = await service.GetAsync(actionIds[0]);
            detail.Title.ShouldBe("添加岗位");
            detail.OperType.ShouldBe(OperEnum.Insert);
            detail.RequestMethod.ShouldBe("POST");
            detail.IsSuccess.ShouldBeTrue();

            var exportResult = await service.PostExportAsync(new OperationLogGetListInputVo
            {
                Title = "添加岗位"
            });
            var physicalFile = exportResult.ShouldBeOfType<PhysicalFileResult>();
            try
            {
                File.Exists(physicalFile.FileName).ShouldBeTrue();
                new FileInfo(physicalFile.FileName).Length.ShouldBeGreaterThan(0);
            }
            finally
            {
                File.Delete(physicalFile.FileName);
            }

            await service.DeleteAsync([actionIds[0]]);
        }

        (await repository.GetOperationLogAsync(actionIds[0])).ShouldBeNull();
        (await repository.GetOperationLogAsync(actionIds[1])).ShouldBeNull();
    }

    /// <summary>
    /// 验证清空操作只删除包含业务 Action 的审计聚合
    /// </summary>
    [Fact]
    public async Task Clean_ShouldPreserveNonBusinessAuditAggregate()
    {
        var repository = GetRequiredService<IAuditLogRepository>();
        var service = GetRequiredService<IOperationLogService>();
        var principalAccessor = GetRequiredService<ICurrentPrincipalAccessor>();
        var (_, firstBusinessActions) = await CreateAuditLogAsync("修改岗位", OperEnum.Update, true);
        var (_, secondBusinessActions) = await CreateAuditLogAsync("删除岗位", OperEnum.Delete, true);
        var (nonBusinessAuditLogId, _) = await CreateAuditLogAsync(string.Empty, OperEnum.Other, false);

        using (principalAccessor.Change(await CreateHostSuperAdminPrincipalAsync()))
        {
            await service.DeleteCleanAsync();
        }

        (await repository.GetOperationLogAsync(firstBusinessActions[0])).ShouldBeNull();
        (await repository.GetOperationLogAsync(secondBusinessActions[0])).ShouldBeNull();
        (await repository.GetFirstAsync(entity => entity.Id == nonBusinessAuditLogId)).ShouldNotBeNull();
    }

    /// <summary>
    /// 验证操作记录删除接口不能通过普通审计 Action 标识删除非业务审计聚合
    /// </summary>
    [Fact]
    public async Task Delete_ShouldIgnoreNonBusinessAuditAction()
    {
        var repository = GetRequiredService<IAuditLogRepository>();
        var service = GetRequiredService<IOperationLogService>();
        var principalAccessor = GetRequiredService<ICurrentPrincipalAccessor>();
        var (auditLogId, actionIds) = await CreateAuditLogAsync(string.Empty, OperEnum.Other, false);

        using (principalAccessor.Change(await CreateHostSuperAdminPrincipalAsync()))
        {
            await service.DeleteAsync(actionIds);
        }

        (await repository.GetFirstAsync(entity => entity.Id == auditLogId)).ShouldNotBeNull();
    }

    /// <summary>
    /// 创建包含指定数量 Action 的测试审计聚合
    /// </summary>
    /// <param name="title">业务标题</param>
    /// <param name="operationType">业务操作类型</param>
    /// <param name="isBusinessOperation">是否作为业务操作展示</param>
    /// <param name="actionCount">Action 数量</param>
    private async Task<(Guid AuditLogId, List<Guid> ActionIds)> CreateAuditLogAsync(
        string title,
        OperEnum operationType,
        bool isBusinessOperation,
        int actionCount = 1)
    {
        var repository = GetRequiredService<IAuditLogRepository>();
        var auditLogId = Guid.NewGuid();
        var executionTime = DateTime.Now;
        var actions = Enumerable.Range(0, actionCount)
            .Select(index => CreateAction(
                Guid.NewGuid(),
                auditLogId,
                executionTime.AddMilliseconds(index),
                title,
                operationType,
                isBusinessOperation))
            .ToList();
        var auditLog = new AuditLogAggregateRoot(
            auditLogId,
            "Yi.Module.Rbac.Test",
            null,
            string.Empty,
            null,
            "audit-test-user",
            executionTime,
            25,
            "127.0.0.1",
            "test-client",
            string.Empty,
            Guid.NewGuid().ToString("N"),
            "test-browser",
            "POST",
            "/api/post",
            200,
            null,
            string.Empty,
            null,
            string.Empty,
            new ExtraPropertyDictionary(),
            [],
            actions,
            string.Empty,
            string.Empty);

        await repository.InsertAsync(auditLog);
        return (auditLogId, actions.Select(action => action.Id).ToList());
    }

    /// <summary>
    /// 创建携带 Yi 业务元数据的 ABP 审计 Action
    /// </summary>
    private static AuditLogActionEntity CreateAction(
        Guid actionId,
        Guid auditLogId,
        DateTime executionTime,
        string title,
        OperEnum operationType,
        bool isBusinessOperation)
    {
        var actionInfo = new AuditLogActionInfo
        {
            ServiceName = "Yi.Module.Rbac.Application.Services.System.PostService",
            MethodName = "CreateAsync",
            Parameters = "{\"postName\":\"测试岗位\"}",
            ExecutionTime = executionTime,
            ExecutionDuration = 20
        };
        actionInfo.SetProperty(OperationMetadataPropertyNames.Title, title);
        actionInfo.SetProperty(OperationMetadataPropertyNames.Type, (int)operationType);
        actionInfo.SetProperty(OperationMetadataPropertyNames.IsBusinessOperation, isBusinessOperation);

        return new AuditLogActionEntity(actionId, auditLogId, actionInfo);
    }

    /// <summary>
    /// 创建宿主超级管理员测试身份
    /// </summary>
    private async Task<ClaimsPrincipal> CreateHostSuperAdminPrincipalAsync()
    {
        var userRepository = GetRequiredService<IUserRepository>();
        var user = await userRepository.GetFirstAsync(entity => entity.UserName == "cc");
        var identity = new ClaimsIdentity(
            [new Claim(AbpClaimTypes.UserId, user.Id.ToString())],
            authenticationType: "Test");
        return new ClaimsPrincipal(identity);
    }
}
