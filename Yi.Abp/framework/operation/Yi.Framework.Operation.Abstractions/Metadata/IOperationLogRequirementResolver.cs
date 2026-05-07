using System.Reflection;
using Microsoft.AspNetCore.Mvc.Controllers;

namespace Yi.Framework.Operation.Abstractions.Metadata
{
    /// <summary>
    /// 操作日志要求解析器接口
    /// 仅解析日志决策所需信息，不涉及权限
    /// </summary>
    public interface IOperationLogRequirementResolver
    {
        /// <summary>
        /// 从 Controller Action 解析日志要求
        /// </summary>
        OperationLogRequirement Resolve(ControllerActionDescriptor descriptor);

        /// <summary>
        /// 从服务类型和方法解析日志要求（用于非 Controller 场景）
        /// </summary>
        OperationLogRequirement Resolve(Type serviceType, MethodInfo methodInfo);
    }
}