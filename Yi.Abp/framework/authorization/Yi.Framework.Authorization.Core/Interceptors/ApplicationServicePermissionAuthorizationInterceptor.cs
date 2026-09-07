using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Authorization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.DependencyInjection;
using Volo.Abp.DynamicProxy;
using Volo.Abp.Users;
using Yi.Framework.Authorization.Abstractions.Metadata;
using Yi.Framework.Authorization.Core.Metadata;

namespace Yi.Framework.Authorization.Core.Interceptors;

/// <summary>
/// 使用 ABP 权限检查器执行 Yi 应用服务权限要求
/// </summary>
public class ApplicationServicePermissionAuthorizationInterceptor : AbpInterceptor, ITransientDependency
{
    private readonly IPermissionRequirementResolver _requirementResolver;
    private readonly IPermissionChecker _permissionChecker;
    private readonly ICurrentUser _currentUser;

    /// <summary>
    /// 创建非 HTTP 应用服务权限授权拦截器
    /// </summary>
    public ApplicationServicePermissionAuthorizationInterceptor(
        IPermissionRequirementResolver requirementResolver,
        IPermissionChecker permissionChecker,
        ICurrentUser currentUser)
    {
        _requirementResolver = requirementResolver;
        _permissionChecker = permissionChecker;
        _currentUser = currentUser;
    }

    /// <summary>
    /// 在应用服务方法执行前完成默认拒绝式权限检查
    /// </summary>
    public override async Task InterceptAsync(IAbpMethodInvocation invocation)
    {
        var serviceType = invocation.TargetObject?.GetType() ?? invocation.Method.DeclaringType;
        if (serviceType == null || IsAllowAnonymous(serviceType, invocation.Method) || IsRemoteServiceDisabled(serviceType, invocation.Method))
        {
            await invocation.ProceedAsync();
            return;
        }

        var requirement = _requirementResolver.Resolve(serviceType, invocation.Method);
        if (requirement.Ignore)
        {
            if (!_currentUser.IsAuthenticated)
            {
                throw new AbpAuthorizationException("当前应用服务方法要求登录后访问");
            }

            await invocation.ProceedAsync();
            return;
        }

        if (!requirement.IsResolved || string.IsNullOrWhiteSpace(requirement.Code))
        {
            throw new AbpAuthorizationException($"远程应用服务方法未声明权限: {serviceType.FullName}.{invocation.Method.Name}");
        }

        if (!await _permissionChecker.IsGrantedAsync(requirement.Code))
        {
            throw new AbpAuthorizationException($"没有权限执行: {requirement.Code}");
        }

        await invocation.ProceedAsync();
    }

    /// <summary>
    /// 判断服务或方法是否显式允许匿名访问
    /// </summary>
    private static bool IsAllowAnonymous(Type serviceType, MethodInfo method)
    {
        return PermissionReflectionHelper.GetMethodAttribute<AllowAnonymousAttribute>(method, serviceType) != null ||
               serviceType.IsDefined(typeof(AllowAnonymousAttribute), inherit: true);
    }

    /// <summary>
    /// 判断服务、实现方法或接口方法是否关闭 ABP 远程服务暴露
    /// </summary>
    private static bool IsRemoteServiceDisabled(Type serviceType, MethodInfo method)
    {
        var methodAttribute = PermissionReflectionHelper.GetMethodAttribute<RemoteServiceAttribute>(method, serviceType);
        if (methodAttribute?.IsEnabled == false)
        {
            return true;
        }

        var serviceAttribute = serviceType.GetCustomAttribute<RemoteServiceAttribute>(inherit: true) ??
                               serviceType.GetInterfaces()
                                   .Select(interfaceType => interfaceType.GetCustomAttribute<RemoteServiceAttribute>(inherit: true))
                                   .FirstOrDefault(attribute => attribute != null);
        return serviceAttribute?.IsEnabled == false;
    }
}
