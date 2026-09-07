using System.Reflection;
using Volo.Abp.DependencyInjection;
using Volo.Abp.DynamicProxy;
using Yi.Framework.Authorization.Abstractions.Attributes;

namespace Yi.Framework.Authorization.Core.Interceptors;

/// <summary>
/// Yi 权限拦截器注册器
/// </summary>
public static class ApplicationServicePermissionAuthorizationInterceptorRegistrar
{
    private const string ApplicationServiceInterfaceName = "Volo.Abp.Application.Services.IApplicationService";

    /// <summary>
    /// 为包含 Yi 权限声明的服务注册授权拦截器
    /// </summary>
    public static void RegisterIfNeeded(IOnServiceRegistredContext context)
    {
        if (!DynamicProxyIgnoreTypes.Contains(context.ImplementationType) && ShouldIntercept(context.ImplementationType))
        {
            context.Interceptors.TryAdd<ApplicationServicePermissionAuthorizationInterceptor>();
        }

    }

    /// <summary>
    /// 判断服务类型是否包含资源或方法权限声明
    /// </summary>
    private static bool ShouldIntercept(Type type)
    {
        if (type.GetInterfaces().Any(interfaceType => interfaceType.FullName == ApplicationServiceInterfaceName))
        {
            return true;
        }

        if (type.IsDefined(typeof(PermissionResourceAttribute), inherit: true) ||
            type.GetInterfaces().Any(interfaceType => interfaceType.IsDefined(typeof(PermissionResourceAttribute), inherit: true)))
        {
            return true;
        }

        return type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .Any(method => method.IsDefined(typeof(PermissionAttribute), inherit: true) ||
                           method.IsDefined(typeof(PermissionActionAttribute), inherit: true));
    }
}
