using System.Reflection;
using Microsoft.AspNetCore.Mvc.Controllers;

namespace Yi.Framework.Operation.Abstractions.Metadata
{
    /// <summary>
    /// Action 身份解析器接口
    /// 只解析稳定事实（服务类型、方法、模块、资源），不做权限或日志决策
    /// </summary>
    public interface IActionIdentityResolver
    {
        /// <summary>
        /// 从 Controller Action 解析身份信息
        /// </summary>
        ActionIdentity Resolve(ControllerActionDescriptor descriptor);

        /// <summary>
        /// 从服务类型和方法解析身份信息（用于非 Controller 场景）
        /// </summary>
        ActionIdentity Resolve(Type serviceType, MethodInfo methodInfo);
    }
}