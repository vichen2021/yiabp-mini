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
/// 租户 OSS 存储设置服务。
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
    /// 读取当前租户的 OSS 设置（仅读取显式设置的值，不 fallback 默认值）。AccessKeySecret 不回显。
    /// </summary>
    [HttpGet("file-management/oss-settings")]
    [PermissionAction(PermissionActionEnum.Query)]
    public async Task<TenantOssSettingDto> GetAsync()
    {
        return new TenantOssSettingDto
        {
            Provider = await _settingManager.GetOrNullGlobalAsync(FileManagementSettingNames.Provider, fallback: false),
            PathPrefix = await _settingManager.GetOrNullGlobalAsync(FileManagementSettingNames.PathPrefix, fallback: false),
            AccessKeyId = await _settingManager.GetOrNullGlobalAsync(FileManagementSettingNames.Aliyun.AccessKeyId, fallback: false),
            AccessKeySecret = string.Empty,
            Endpoint = await _settingManager.GetOrNullGlobalAsync(FileManagementSettingNames.Aliyun.Endpoint, fallback: false),
            ContainerName = await _settingManager.GetOrNullGlobalAsync(FileManagementSettingNames.Aliyun.ContainerName, fallback: false),
            CreateContainerIfNotExists = bool.Parse(
                await _settingManager.GetOrNullGlobalAsync(
                    FileManagementSettingNames.Aliyun.CreateContainerIfNotExists, fallback: false) ?? "false")
        };
    }

    /// <summary>
    /// 更新当前租户的 OSS 设置，写入租户自己数据库的 Global 维度。
    /// </summary>
    [HttpPut("file-management/oss-settings")]
    [PermissionAction(PermissionActionEnum.Edit)]
    public async Task UpdateAsync([FromBody] TenantOssSettingDto input)
    {
        await _settingManager.SetGlobalAsync(FileManagementSettingNames.Provider, input.Provider);
        await _settingManager.SetGlobalAsync(FileManagementSettingNames.PathPrefix, input.PathPrefix);
        await _settingManager.SetGlobalAsync(FileManagementSettingNames.Aliyun.AccessKeyId, input.AccessKeyId);
        await _settingManager.SetGlobalAsync(FileManagementSettingNames.Aliyun.Endpoint, input.Endpoint);
        await _settingManager.SetGlobalAsync(FileManagementSettingNames.Aliyun.ContainerName, input.ContainerName);
        await _settingManager.SetGlobalAsync(
            FileManagementSettingNames.Aliyun.CreateContainerIfNotExists,
            input.CreateContainerIfNotExists.ToString().ToLower());

        if (!string.IsNullOrWhiteSpace(input.AccessKeySecret))
        {
            await _settingManager.SetGlobalAsync(FileManagementSettingNames.Aliyun.AccessKeySecret, input.AccessKeySecret);
        }
    }
}
