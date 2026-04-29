using Yi.Framework.Operation.Abstractions.Metadata;
using Yi.Framework.Operation.Abstractions.Permissions;

namespace Yi.Framework.Operation.Core.Permissions
{
    /// <summary>
    /// 默认权限码生成器
    /// 格式: {module}:{entity}:{action}
    /// </summary>
    public class DefaultPermissionCodeGenerator : IPermissionCodeGenerator
    {
        public string Generate(ActionMetadata metadata)
        {
            if (!metadata.IsResolved)
            {
                return string.Empty;
            }

            return $"{metadata.ModuleName}:{metadata.EntityName}:{metadata.ActionName}";
        }
    }
}