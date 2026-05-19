using Volo.Abp.Data;
using Yi.Module.Rbac.Domain.Shared.Authorization;

namespace Yi.Module.Rbac.Domain.Extensions
{
    public static class DataPermissionExtensions
    {
        /// <summary>
        /// 关闭数据权限
        /// </summary>
        /// <param name="dataFilter"></param>
        /// <returns></returns>
        public static IDisposable DisablePermissionHandler(this IDataFilter dataFilter)
        {
            return dataFilter.Disable<IDataPermission>();
        }
    }
}
