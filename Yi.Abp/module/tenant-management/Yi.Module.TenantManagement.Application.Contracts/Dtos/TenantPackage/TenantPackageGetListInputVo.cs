using Yi.Framework.Ddd.Application.Contracts;

namespace Yi.Module.TenantManagement.Application.Contracts.Dtos.TenantPackage
{
    /// <summary>
    /// 租户套餐列表查询输入
    /// </summary>
    public class TenantPackageGetListInputVo : PagedAllResultRequestDto
    {
        /// <summary>
        /// 套餐名称
        /// </summary>
        public string? PackageName { get; set; }
        /// <summary>
        /// 状态
        /// </summary>
        public bool? State { get; set; }
    }
}
