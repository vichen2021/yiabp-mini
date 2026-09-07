using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Authorization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Users;
using Yi.Framework.Authorization.Abstractions.Metadata;
using Yi.Framework.Authorization.Core.Metadata;

namespace Yi.Framework.Authorization.Core.Filters;

/// <summary>
/// 对 ABP 动态 API 执行 Yi 权限检查
/// </summary>
public class ApplicationServicePermissionAuthorizationFilter : IAsyncAuthorizationFilter, ITransientDependency
{
    private readonly IPermissionRequirementResolver _requirementResolver;
    private readonly IPermissionChecker _permissionChecker;
    private readonly ICurrentUser _currentUser;

    /// <summary>
    /// 创建动态 API 权限授权过滤器
    /// </summary>
    public ApplicationServicePermissionAuthorizationFilter(
        IPermissionRequirementResolver requirementResolver,
        IPermissionChecker permissionChecker,
        ICurrentUser currentUser)
    {
        _requirementResolver = requirementResolver;
        _permissionChecker = permissionChecker;
        _currentUser = currentUser;
    }

    /// <summary>
    /// 在动态 API Action 执行前完成默认拒绝式权限检查
    /// </summary>
    /// <param name="context">授权过滤器上下文</param>
    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        if (context.ActionDescriptor is not ControllerActionDescriptor descriptor)
        {
            return;
        }

        var serviceType = descriptor.ControllerTypeInfo.AsType();
        if (!typeof(IApplicationService).IsAssignableFrom(serviceType))
        {
            return;
        }

        if (IsAllowAnonymous(descriptor, serviceType) || IsRemoteServiceDisabled(serviceType, descriptor.MethodInfo))
        {
            return;
        }

        var requirement = _requirementResolver.Resolve(serviceType, descriptor.MethodInfo);
        if (!_currentUser.IsAuthenticated)
        {
            context.Result = new ChallengeResult();
            return;
        }

        if (requirement.Ignore)
        {
            return;
        }

        if (!requirement.IsResolved || string.IsNullOrWhiteSpace(requirement.Code))
        {
            throw new AbpAuthorizationException($"远程应用服务方法未声明权限: {serviceType.FullName}.{descriptor.MethodInfo.Name}");
        }

        if (!await _permissionChecker.IsGrantedAsync(requirement.Code))
        {
            context.Result = new ForbidResult();
        }
    }

    /// <summary>
    /// 判断动态 API 服务或方法是否显式允许匿名访问
    /// </summary>
    private static bool IsAllowAnonymous(ControllerActionDescriptor descriptor, Type serviceType)
    {
        return descriptor.EndpointMetadata.OfType<AllowAnonymousAttribute>().Any() ||
               PermissionReflectionHelper.GetMethodAttribute<AllowAnonymousAttribute>(descriptor.MethodInfo, serviceType) != null ||
               serviceType.IsDefined(typeof(AllowAnonymousAttribute), inherit: true);
    }

    /// <summary>
    /// 判断动态 API 服务、实现方法或接口方法是否关闭远程访问
    /// </summary>
    private static bool IsRemoteServiceDisabled(Type serviceType, System.Reflection.MethodInfo method)
    {
        var methodAttribute = PermissionReflectionHelper.GetMethodAttribute<RemoteServiceAttribute>(method, serviceType);
        if (methodAttribute?.IsEnabled == false)
        {
            return true;
        }

        var serviceAttribute = serviceType.GetCustomAttributes(typeof(RemoteServiceAttribute), inherit: true)
            .OfType<RemoteServiceAttribute>()
            .FirstOrDefault() ?? serviceType.GetInterfaces()
            .Select(interfaceType => interfaceType.GetCustomAttributes(typeof(RemoteServiceAttribute), inherit: true)
                .OfType<RemoteServiceAttribute>()
                .FirstOrDefault())
            .FirstOrDefault(attribute => attribute != null);
        return serviceAttribute?.IsEnabled == false;
    }
}
