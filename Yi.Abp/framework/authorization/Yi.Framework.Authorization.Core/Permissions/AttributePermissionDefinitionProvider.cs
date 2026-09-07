using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Volo.Abp;
using Volo.Abp.Authorization.Permissions;
using Yi.Framework.Authorization.Abstractions.Attributes;
using Yi.Framework.Authorization.Abstractions.Metadata;
using Yi.Framework.Authorization.Abstractions.Options;
using Yi.Framework.Authorization.Core.Metadata;

namespace Yi.Framework.Authorization.Core.Permissions;

/// <summary>
/// 将 Yi 权限声明注册为 ABP 权限定义
/// </summary>
public class AttributePermissionDefinitionProvider : PermissionDefinitionProvider
{
    private const string ApplicationServiceInterfaceName = "Volo.Abp.Application.Services.IApplicationService";

    private readonly PermissionDefinitionOptions _options;
    private readonly IPermissionRequirementResolver _requirementResolver;

    /// <summary>
    /// 创建基于特性发现权限的 ABP 定义提供器
    /// </summary>
    public AttributePermissionDefinitionProvider(
        IOptions<PermissionDefinitionOptions> options,
        IPermissionRequirementResolver requirementResolver)
    {
        _options = options.Value;
        _requirementResolver = requirementResolver;
    }

    /// <summary>
    /// 按模块和资源构建 ABP 权限定义树
    /// </summary>
    public override void Define(IPermissionDefinitionContext context)
    {
        var permissions = DiscoverPermissions();
        foreach (var moduleGroup in permissions.GroupBy(permission => permission.Module, StringComparer.OrdinalIgnoreCase))
        {
            var group = context.GetGroupOrNull(moduleGroup.Key) ?? context.AddGroup(moduleGroup.Key);
            foreach (var resourceGroup in moduleGroup.GroupBy(permission => permission.Resource, StringComparer.OrdinalIgnoreCase))
            {
                var resourceCode = $"{moduleGroup.Key}:{resourceGroup.Key}";
                var resource = group.GetPermissionOrNull(resourceCode) ?? group.AddPermission(resourceCode);
                foreach (var permission in resourceGroup)
                {
                    if (resource.Children.All(existing => !string.Equals(existing.Name, permission.Code, StringComparison.OrdinalIgnoreCase)))
                    {
                        resource.AddChild(permission.Code);
                    }
                }
            }
        }
    }

    /// <summary>
    /// 从显式登记的程序集发现所有三段式权限码
    /// </summary>
    private IReadOnlyList<DiscoveredPermission> DiscoverPermissions()
    {
        var result = new Dictionary<string, DiscoveredPermission>(StringComparer.OrdinalIgnoreCase);
        foreach (var serviceType in _options.ApplicationAssemblies.SelectMany(GetTypesSafely).Where(IsConcreteService))
        {
            foreach (var method in serviceType.GetMethods(BindingFlags.Instance | BindingFlags.Public))
            {
                if (!IsRemoteApplicationMethod(serviceType, method))
                {
                    continue;
                }

                var requirement = _requirementResolver.Resolve(serviceType, method);
                if (requirement.Ignore)
                {
                    continue;
                }

                if (!requirement.IsResolved || string.IsNullOrWhiteSpace(requirement.Code))
                {
                    throw new AbpException(
                        $"远程应用服务方法未声明权限: {serviceType.FullName}.{method.Name}; {requirement.UnresolvedReason}");
                }

                var parts = requirement.Code.Split(':', StringSplitOptions.TrimEntries);
                if (parts.Length != 3 || parts.Any(string.IsNullOrWhiteSpace))
                {
                    throw new AbpException($"权限码必须使用三段式格式: {requirement.Code}");
                }

                result.TryAdd(requirement.Code, new DiscoveredPermission(requirement.Code, parts[0], parts[1]));
            }
        }

        return result.Values.ToList();
    }

    /// <summary>
    /// 判断类型是否为具体应用服务
    /// </summary>
    private static bool IsConcreteService(Type type)
    {
        return type is { IsClass: true, IsAbstract: false } &&
               type.GetInterfaces().Any(interfaceType => interfaceType.FullName == ApplicationServiceInterfaceName);
    }

    /// <summary>
    /// 判断公共方法是否属于需要权限声明的远程应用服务边界
    /// </summary>
    private static bool IsRemoteApplicationMethod(Type serviceType, MethodInfo method)
    {
        if (method.IsSpecialName || method.DeclaringType == typeof(object))
        {
            return false;
        }

        if (PermissionReflectionHelper.GetMethodAttribute<AllowAnonymousAttribute>(method, serviceType) != null ||
            serviceType.IsDefined(typeof(AllowAnonymousAttribute), inherit: true))
        {
            return false;
        }

        var methodRemoteService = PermissionReflectionHelper.GetMethodAttribute<RemoteServiceAttribute>(method, serviceType);
        if (methodRemoteService?.IsEnabled == false)
        {
            return false;
        }

        var serviceRemoteService = serviceType.GetCustomAttribute<RemoteServiceAttribute>(inherit: true) ??
                                   serviceType.GetInterfaces()
                                       .Select(interfaceType => interfaceType.GetCustomAttribute<RemoteServiceAttribute>(inherit: true))
                                       .FirstOrDefault(attribute => attribute != null);
        return serviceRemoteService?.IsEnabled != false;
    }

    /// <summary>
    /// 解析服务类或服务接口上的权限资源
    /// </summary>
    private static PermissionResourceAttribute? ResolvePermissionResource(Type serviceType)
    {
        return serviceType.GetCustomAttribute<PermissionResourceAttribute>(inherit: true) ??
               serviceType.GetInterfaces()
                   .Select(interfaceType => interfaceType.GetCustomAttribute<PermissionResourceAttribute>(inherit: true))
                   .FirstOrDefault(attribute => attribute != null);
    }

    /// <summary>
    /// 安全读取程序集类型并忽略无法加载的可选类型
    /// </summary>
    private static IEnumerable<Type> GetTypesSafely(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException exception)
        {
            return exception.Types.Where(type => type != null)!;
        }
    }

    private sealed record DiscoveredPermission(string Code, string Module, string Resource);
}
