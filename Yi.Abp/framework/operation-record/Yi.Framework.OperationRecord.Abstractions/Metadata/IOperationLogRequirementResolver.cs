using System.Reflection;
using Microsoft.AspNetCore.Mvc.Controllers;

namespace Yi.Framework.OperationRecord.Abstractions.Metadata
{
    /// <summary>
    /// 操作记录要求解析器接口。仅解析操作记录决策所需信息，不涉及权限。
    /// </summary>
    public interface IOperationLogRequirementResolver
    {
        /// <summary>
        /// 从 Controller Action 解析操作记录要求。
        /// </summary>
        OperationLogRequirement Resolve(ControllerActionDescriptor descriptor);

        /// <summary>
        /// 从服务类型和方法解析操作记录要求。
        /// </summary>
        OperationLogRequirement Resolve(Type serviceType, MethodInfo methodInfo);
    }
}
