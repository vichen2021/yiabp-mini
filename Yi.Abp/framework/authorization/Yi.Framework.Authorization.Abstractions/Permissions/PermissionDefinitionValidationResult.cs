namespace Yi.Framework.Authorization.Abstractions.Permissions
{
    public sealed class PermissionDefinitionValidationResult
    {
        public List<string> MissingDefinitions { get; } = new();

        public List<string> DuplicatedDefinitions { get; } = new();

        public bool HasErrors => MissingDefinitions.Count > 0 || DuplicatedDefinitions.Count > 0;
    }
}
