using Microsoft.AspNetCore.Mvc.Controllers;

namespace Yi.Framework.Operation.Abstractions.Metadata
{
    /// <summary>
    /// Action元数据解析接口
    /// </summary>
    public interface IActionMetadataResolver
    {
        /// <summary>
        /// 从ControllerActionDescriptor解析元数据
        /// </summary>
        ActionMetadata Resolve(ControllerActionDescriptor descriptor);
    }
}