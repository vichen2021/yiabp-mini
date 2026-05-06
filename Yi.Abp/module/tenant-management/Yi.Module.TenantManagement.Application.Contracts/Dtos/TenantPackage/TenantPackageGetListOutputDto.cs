using Volo.Abp.Application.Dtos;

namespace Yi.Module.TenantManagement.Application.Contracts.Dtos.TenantPackage
{
    /// <summary>
    /// 租户套餐列表输出DTO
    /// </summary>
    public class TenantPackageGetListOutputDto : EntityDto<Guid>
    {
        /// <summary>
        /// 套餐名称
        /// </summary>
        public string PackageName { get; set; }
        /// <summary>
        /// 菜单树选择是否父子关联
        /// </summary>
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
        /// 创建时间
        /// </summary>
        public DateTime CreationTime { get; set; }
        /// <summary>
        /// 创建者
        /// </summary>
        public Guid? CreatorId { get; set; }
    }
}
