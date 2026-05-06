using SqlSugar;
using Volo.Abp.Domain.Entities;

namespace Yi.Module.TenantManagement.Domain.Entities;

/// <summary>
/// 租户套餐菜单关系表
/// </summary>
[SugarTable("TenantPackageMenu")]
public class TenantPackageMenuEntity : Entity<Guid>
{
    [SugarColumn(IsPrimaryKey = true)]
    public override Guid Id { get; protected set; }

    /// <summary>
    /// 套餐ID
    /// </summary>
    public Guid PackageId { get; set; }

    /// <summary>
    /// 菜单ID
    /// </summary>
    public Guid MenuId { get; set; }
}