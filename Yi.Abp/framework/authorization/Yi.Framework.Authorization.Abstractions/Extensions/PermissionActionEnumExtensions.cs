namespace Yi.Framework.Authorization.Abstractions.Extensions;

/// <summary>
/// 权限动作枚举扩展方法
/// </summary>
public static class PermissionActionEnumExtensions
{
    /// <summary>
    /// 将权限动作枚举转换为权限码字符串（小写）
    /// </summary>
    public static string ToPermissionCode(this Enums.PermissionActionEnum action)
    {
        return action switch
        {
            Enums.PermissionActionEnum.Query => "query",
            Enums.PermissionActionEnum.Add => "add",
            Enums.PermissionActionEnum.Edit => "edit",
            Enums.PermissionActionEnum.Remove => "remove",
            Enums.PermissionActionEnum.Export => "export",
            Enums.PermissionActionEnum.Import => "import",
            _ => throw new ArgumentOutOfRangeException(nameof(action), action, null)
        };
    }
}