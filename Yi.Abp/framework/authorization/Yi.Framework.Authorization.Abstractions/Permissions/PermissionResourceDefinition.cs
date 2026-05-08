namespace Yi.Framework.Authorization.Abstractions.Permissions
{
    public sealed class PermissionResourceDefinition
    {
        public PermissionResourceDefinition(string module, string resource, string? displayName = null)
        {
            Module = module;
            Resource = resource;
            DisplayName = displayName;
        }

        public string Module { get; }

        public string Resource { get; }

        public string? DisplayName { get; }

        public List<PermissionDefinition> Permissions { get; } = new();
    }
}
