namespace Yi.Framework.Authorization.Abstractions.Permissions
{
    public sealed class PermissionDefinition
    {
        public PermissionDefinition(string code, string module, string resource, string action, string? displayName = null)
        {
            Code = code;
            Module = module;
            Resource = resource;
            Action = action;
            DisplayName = displayName;
        }

        public string Code { get; }

        public string Module { get; }

        public string Resource { get; }

        public string Action { get; }

        public string? DisplayName { get; }
    }
}
