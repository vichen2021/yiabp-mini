using Volo.Abp.DependencyInjection;
using Yi.Framework.Authorization.Abstractions.Permissions;

namespace Yi.Framework.Authorization.Core.Permissions
{
    public class DefaultPermissionDefinitionValidator : IPermissionDefinitionValidator, ITransientDependency
    {
        private readonly IPermissionDefinitionProvider _definitionProvider;

        public DefaultPermissionDefinitionValidator(IPermissionDefinitionProvider definitionProvider)
        {
            _definitionProvider = definitionProvider;
        }

        public PermissionDefinitionValidationResult Validate(IEnumerable<string> usedPermissionCodes)
        {
            var result = new PermissionDefinitionValidationResult();
            var definedCodes = _definitionProvider.GetPermissionCodes();

            foreach (var code in usedPermissionCodes.Where(code => !string.IsNullOrWhiteSpace(code)).Distinct(StringComparer.OrdinalIgnoreCase))
            {
                if (!definedCodes.Contains(code))
                {
                    result.MissingDefinitions.Add(code);
                }
            }

            var duplicatedCodes = _definitionProvider.GetResources()
                .SelectMany(resource => resource.Permissions)
                .GroupBy(permission => permission.Code, StringComparer.OrdinalIgnoreCase)
                .Where(group => group.Count() > 1)
                .Select(group => group.Key);

            result.DuplicatedDefinitions.AddRange(duplicatedCodes);
            return result;
        }
    }
}
