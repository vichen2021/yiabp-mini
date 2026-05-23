using Microsoft.AspNetCore.Mvc;
using Yi.Module.FileManagement.Application.Contracts.Dtos;

namespace Yi.Module.FileManagement.Application.Contracts.IServices;

/// <summary>
/// 租户 OSS 存储设置服务接口（租户自治，租户管理员在自己的系统内配置）。
/// </summary>
public interface ITenantOssSettingsService
{
    /// <summary>
    /// 读取当前租户的 OSS 设置。AccessKeySecret 不回显，始终返回空字符串。
    /// </summary>
    Task<TenantOssSettingDto> GetAsync();

    /// <summary>
    /// 更新当前租户的 OSS 设置，写入租户自己数据库的 Global 维度。
    /// AccessKeySecret 为空时跳过写入（保持原值）。
    /// </summary>
    Task UpdateAsync([FromBody] TenantOssSettingDto input);
}
