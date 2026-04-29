using System.Reflection;
using Microsoft.AspNetCore.Mvc.Controllers;

namespace Yi.Framework.Operation.Abstractions.Metadata
{
    /// <summary>
    /// Action元数据解析接口
    /// 支持从 MVC Descriptor 和 Service 反射两种方式解析
    /// </summary>
    public interface IActionMetadataResolver
    {
        /// <summary>
        /// 从ControllerActionDescriptor解析元数据（MVC Filter 使用）
        /// </summary>
        ActionMetadata Resolve(ControllerActionDescriptor descriptor);

        /// <summary>
        /// 从 Service 类型和 MethodInfo 解析元数据（AuditLog Mapper 使用）
        /// </summary>
        ActionMetadata Resolve(Type serviceType, MethodInfo methodInfo);
    }
}