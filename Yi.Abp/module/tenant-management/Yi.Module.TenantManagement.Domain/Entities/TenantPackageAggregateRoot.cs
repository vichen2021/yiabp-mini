using SqlSugar;
using Volo.Abp.Auditing;
using Volo.Abp.Domain.Entities;
using Yi.Framework.Core.Data;

namespace Yi.Module.TenantManagement.Domain.Entities;

/// <summary>
/// 租户套餐
/// </summary>
[SugarTable("TenantPackage")]
[SugarIndex($"index_{nameof(PackageName)}", nameof(PackageName), OrderByType.Asc)]
public class TenantPackageAggregateRoot : AggregateRoot<Guid>, ISoftDelete, IAuditedObject, IOrderNum, IState
{
    [SugarColumn(IsPrimaryKey = true)]
    public override Guid Id { get; protected set; }

    /// <summary>
    /// 套餐名称
    /// </summary>
    [SugarColumn(Length = 50)]
    public string PackageName { get; set; } = string.Empty;

    /// <summary>
    /// 菜单树选择是否父子关联
    /// </summary>
    public bool MenuCheckStrictly { get; set; } = true;

    /// <summary>
    /// 状态
    /// </summary>
    public bool State { get; set; } = true;

    /// <summary>
    /// 排序号
    /// </summary>
    public int OrderNum { get; set; } = 0;

    /// <summary>
    /// 备注
    /// </summary>
    [SugarColumn(Length = 500, IsNullable = true)]
    public string? Remark { get; set; }

    /// <summary>
    /// 软删除
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreationTime { get; set; } = DateTime.Now;

    /// <summary>
    /// 创建者ID
    /// </summary>
    public Guid? CreatorId { get; set; }

    /// <summary>
    /// 最后修改者ID
    /// </summary>
    public Guid? LastModifierId { get; set; }

    /// <summary>
    /// 最后修改时间
    /// </summary>
    public DateTime? LastModificationTime { get; set; }
}