using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Yi.Framework.Authorization.Abstractions.Attributes;
using Yi.Framework.Authorization.Abstractions.Enums;
using Yi.Framework.OperationRecord.Abstractions.Attributes;
using Yi.Module.FileManagement.Application.Contracts.Dtos;
using Yi.Module.FileManagement.Application.Contracts.IServices;
using Yi.Module.FileManagement.Domain.Shared.Settings;
using Yi.Module.SettingManagement.Domain.Shared;

namespace Yi.Module.FileManagement.Application.Services;

/// <summary>
/// 租户 OSS 设置管理服务（宿主管理员使用）。
/// <para>
/// 读取：<see cref="ISettingManager.GetOrNullAsync"/> 按指定租户读取 OSS Setting，不触发当前请求租户上下文。
/// 写入：<see cref="TenantSettingManagerExtensions.SetForTenantAsync"/> 向指定租户维度写入 Setting。
/// </para>
/// </summary>
[RemoteService]
[PermissionResource("system", "tenantOSSSettings")]
[OperLogEntity("租户OSS设置")]
public class TenantOssSettingsService : ApplicationService, ITenantOssSettingsService
{
    private readonly ISettingManager _settingManager;

    public TenantOssSettingsService(ISettingManager settingManager)
    {
        _settingManager = settingManager;
    }

    /// <summary>
    /// 读取指定租户的 OSS 设置。AccessKeySecret 不回显。
    /// </summary>
    [HttpGet("file-management/tenants/{tenantId}/oss-settings")]
    [PermissionAction(PermissionActionEnum.Query)]
    public async Task<TenantOssSettingDto> GetAsync([FromRoute] Guid tenantId)
    {
        return new TenantOssSettingDto
        {
            Provider = await _settingManager.GetOrNullForTenantAsync(FileManagementSettingNames.Provider, tenantId),
            PathPrefix = await _settingManager.GetOrNullForTenantAsync(FileManagementSettingNames.PathPrefix, tenantId),
            AccessKeyId = await _settingManager.GetOrNullForTenantAsync(FileManagementSettingNames.Aliyun.AccessKeyId, tenantId),
            AccessKeySecret = string.Empty,
            Endpoint = await _settingManager.GetOrNullForTenantAsync(FileManagementSettingNames.Aliyun.Endpoint, tenantId),
            ContainerName = await _settingManager.GetOrNullForTenantAsync(FileManagementSettingNames.Aliyun.ContainerName, tenantId),
            CreateContainerIfNotExists = bool.Parse(
                await _settingManager.GetOrNullForTenantAsync(
                    FileManagementSettingNames.Aliyun.CreateContainerIfNotExists, tenantId) ?? "false")
        };
    }

    /// <summary>
    /// 更新指定租户的 OSS 设置。
    /// </summary>
    [HttpPut("file-management/tenants/{tenantId}/oss-settings")]
    [PermissionAction(PermissionActionEnum.Edit)]
    public async Task UpdateAsync([FromRoute] Guid tenantId, [FromBody] TenantOssSettingDto input)
    {
        await _settingManager.SetForTenantAsync(tenantId, FileManagementSettingNames.Provider, input.Provider);
        await _settingManager.SetForTenantAsync(tenantId, FileManagementSettingNames.PathPrefix, input.PathPrefix);
        await _settingManager.SetForTenantAsync(tenantId, FileManagementSettingNames.Aliyun.AccessKeyId, input.AccessKeyId);
        await _settingManager.SetForTenantAsync(tenantId, FileManagementSettingNames.Aliyun.Endpoint, input.Endpoint);
        await _settingManager.SetForTenantAsync(tenantId, FileManagementSettingNames.Aliyun.ContainerName, input.ContainerName);
        await _settingManager.SetForTenantAsync(
            tenantId,
            FileManagementSettingNames.Aliyun.CreateContainerIfNotExists,
            input.CreateContainerIfNotExists.ToString().ToLower());

        if (!string.IsNullOrWhiteSpace(input.AccessKeySecret))
        {
            await _settingManager.SetForTenantAsync(tenantId, FileManagementSettingNames.Aliyun.AccessKeySecret, input.AccessKeySecret);
        }
    }
}
