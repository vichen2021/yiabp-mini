namespace Yi.Framework.Authorization.Abstractions.Permissions
{
    /// <summary>
    /// 权限定义校验器。
    /// </summary>
    public interface IPermissionDefinitionValidator
    {
        PermissionDefinitionValidationResult Validate(IEnumerable<string> usedPermissionCodes);
    }
}
