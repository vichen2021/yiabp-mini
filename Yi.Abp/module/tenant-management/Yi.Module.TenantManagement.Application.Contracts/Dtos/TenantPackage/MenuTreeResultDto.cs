using Yi.Module.Rbac.Domain.Shared.Dtos;

namespace Yi.Module.TenantManagement.Application.Contracts.Dtos.TenantPackage;

/// <summary>
/// 套餐菜单树结果DTO
/// </summary>
public class MenuTreeResultDto
{
    /// <summary>
    /// 已选中的菜单ID列表
    /// </summary>
    public List<Guid> CheckedKeys { get; set; } = new();

    /// <summary>
    /// 菜单树列表
    /// </summary>
    public List<MenuTreeDto> Menus { get; set; } = new();
}