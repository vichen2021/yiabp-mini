namespace Yi.Framework.Operation.Abstractions.Permissions
{
    /// <summary>
    /// 权限配置选项
    /// </summary>
    public class PermissionOptions
    {
        /// <summary>
        /// 自动检查已解析的权限码
        /// 默认 true：当能解析出权限码时自动校验
        /// </summary>
        public bool AutoCheckResolvedActions { get; set; } = true;

        /// <summary>
        /// 允许未解析权限码的方法通过
        /// 默认 true：兼容模式，降低开发期接入成本
        /// 生产环境建议设置为 false
        /// </summary>
        public bool AllowUnresolvedActions { get; set; } = true;

        /// <summary>
        /// 白名单方法（无需权限检查）
        /// 格式：{ControllerName}:{MethodName}
        /// 示例：AccountService:LoginAsync
        /// </summary>
        public List<string> WhitelistActions { get; set; } = new();

        /// <summary>
        /// 权限码配置映射表
        /// Key: {FullTypeName}.{MethodName}
        /// Value: 权限码，如 system:user:update
        /// 优先级：低于显式 [Permission]，高于自动推断
        /// </summary>
        public Dictionary<string, string> Mappings { get; set; } = new();
    }
}