using System.Reflection;
using Microsoft.AspNetCore.Mvc.Controllers;

namespace Yi.Framework.Authorization.Abstractions.Metadata
{
    /// <summary>
    /// 权限要求解析器接口。仅解析权限决策所需信息，不涉及操作记录。
    /// </summary>
    public interface IPermissionRequirementResolver
    {
        /// <summary>
        /// 从 Controller Action 解析权限要求。
        /// </summary>
        PermissionRequirement Resolve(ControllerActionDescriptor descriptor);

        /// <summary>
        /// 从服务类型和方法解析权限要求。
        /// </summary>
        PermissionRequirement Resolve(Type serviceType, MethodInfo methodInfo);
    }
}
