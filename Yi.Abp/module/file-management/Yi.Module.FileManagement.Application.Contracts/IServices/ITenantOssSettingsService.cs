using Microsoft.AspNetCore.Mvc;
using Yi.Module.FileManagement.Application.Contracts.Dtos;

namespace Yi.Module.FileManagement.Application.Contracts.IServices;

/// <summary>
/// 租户 OSS 设置管理服务接口（宿主管理员使用，按租户 ID 操作）。
/// </summary>
public interface ITenantOssSettingsService
{
    /// <summary>
    /// 读取指定租户的 OSS 设置（fallback 链：Tenant → Global → Configuration → Default）。
    /// AccessKeySecret 不回显，始终返回空字符串。
    /// </summary>
    Task<TenantOssSettingDto> GetAsync([FromRoute] Guid tenantId);

    /// <summary>
    /// 更新指定租户的 OSS 设置，写入租户维度 Setting。
    /// AccessKeySecret 传明文，由框架自动加密落库；为空时跳过写入（保持原值）。
    /// </summary>
    Task UpdateAsync([FromRoute] Guid tenantId, [FromBody] TenantOssSettingDto input);
}
