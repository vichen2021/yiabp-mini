using System.Reflection;

namespace Yi.Framework.ActionMetadata.Abstractions.Metadata
{
    /// <summary>
    /// Action 身份信息。只包含服务类型、方法和诊断信息，不包含权限或操作记录决策。
    /// </summary>
    public sealed class ActionIdentity
    {
        /// <summary>
        /// 服务类型（Controller 或 ApplicationService）。
        /// </summary>
        public Type ServiceType { get; set; }

        /// <summary>
        /// 执行方法。
        /// </summary>
        public MethodInfo MethodInfo { get; set; }

        /// <summary>
        /// ABP RemoteServiceName 候选值，仅用于诊断。
        /// </summary>
        public string? RemoteServiceName { get; set; }

        /// <summary>
        /// 生成缓存 Key。
        /// </summary>
        public string GetCacheKey()
        {
            return $"Identity:{ServiceType.FullName}:{MethodInfo.Name}";
        }
    }
}
