using System.ComponentModel.DataAnnotations;

namespace Yi.Module.TenantManagement.Application.Contracts.Dtos.TenantPackage
{
    /// <summary>
    /// 租户套餐更新输入
    /// </summary>
    public class TenantPackageUpdateInputVo
    {
        /// <summary>
        /// 套餐名称
        /// </summary>
        [Required]
        public string PackageName { get; set; }
        /// <summary>
        /// 菜单树选择是否父子关联
        /// </summary>
        [Required]
        public bool MenuCheckStrictly { get; set; }
        /// <summary>
        /// 排序
        /// </summary>
        public int OrderNum { get; set; }
        /// <summary>
        /// 状态
        /// </summary>
        public bool State { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string? Remark { get; set; }
        /// <summary>
        /// 关联菜单ID列表
        /// </summary>
        public List<Guid>? MenuIds { get; set; }
    }
}
