using Yi.Framework.Operation.Abstractions.Metadata;

namespace Yi.Framework.Operation.Abstractions.Permissions
{
    /// <summary>
    /// 权限码生成器接口
    /// </summary>
    public interface IPermissionCodeGenerator
    {
        /// <summary>
        /// 根据 Action 元数据生成权限码
        /// </summary>
        string Generate(ActionMetadata metadata);
    }
}