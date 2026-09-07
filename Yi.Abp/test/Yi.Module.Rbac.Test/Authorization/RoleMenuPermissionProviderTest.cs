using System.Security.Claims;
using Shouldly;
using Volo.Abp.Authorization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Security.Claims;
using Yi.Framework.SqlSugarCore.Abstractions;
using Yi.Module.Rbac.Application.Contracts.Dtos.Account;
using Yi.Module.Rbac.Application.Contracts.IServices;
using Yi.Module.Rbac.Domain.Entities;
using Yi.Module.Rbac.Domain.Managers;
using Yi.Module.Rbac.Domain.Shared.Consts;
using Xunit;

namespace Yi.Module.Rbac.Test.Authorization;

/// <summary>
/// 角色菜单 ABP 权限提供器集成测试
/// </summary>
public class RoleMenuPermissionProviderTest : YiModuleRbacTestBase
{
    private const string PostQueryPermission = "system:post:query";

    /// <summary>
    /// 验证角色菜单授权与撤权通过版本切换对同一身份立即生效
    /// </summary>
    [Fact]
    public async Task GrantAndRevoke_ShouldTakeEffectWithoutRefreshingPrincipal()
    {
        var accountService = GetRequiredService<IAccountService>();
        var userRepository = GetRequiredService<ISqlSugarRepository<UserAggregateRoot>>();
        var roleRepository = GetRequiredService<ISqlSugarRepository<RoleAggregateRoot>>();
        var menuRepository = GetRequiredService<ISqlSugarRepository<MenuAggregateRoot>>();
        var roleManager = GetRequiredService<RoleManager>();
        var permissionChecker = GetRequiredService<IPermissionChecker>();
        var principalAccessor = GetRequiredService<ICurrentPrincipalAccessor>();

        var userName = $"permission_{Guid.NewGuid():N}";
        await accountService.PostRegisterAsync(new RegisterDto
        {
            UserName = userName,
            Password = "123456",
            Phone = Random.Shared.NextInt64(13000000000, 19000000000)
        });

        var user = await userRepository.GetFirstAsync(entity => entity.UserName == userName);
        var role = await roleRepository.GetFirstAsync(entity => entity.RoleCode == UserConst.DefaultRoleCode);
        var menu = await menuRepository.GetFirstAsync(entity => entity.PermissionCode == PostQueryPermission);
        var principal = CreatePrincipal(user.Id);

        using (principalAccessor.Change(principal))
        {
            (await permissionChecker.IsGrantedAsync(PostQueryPermission)).ShouldBeFalse();

            await roleManager.GiveRoleSetMenuAsync([role.Id], [menu.Id]);
            (await permissionChecker.IsGrantedAsync(PostQueryPermission)).ShouldBeTrue();

            await roleManager.GiveRoleSetMenuAsync([role.Id], []);
            (await permissionChecker.IsGrantedAsync(PostQueryPermission)).ShouldBeFalse();
        }
    }

    /// <summary>
    /// 验证未认证的直接应用服务调用进入授权拦截器并被拒绝
    /// </summary>
    [Fact]
    public async Task DirectApplicationServiceCall_ShouldRequireAuthentication()
    {
        var postService = GetRequiredService<IPostService>();

        await Should.ThrowAsync<AbpAuthorizationException>(
            () => postService.GetListAsync(new()));
    }

    /// <summary>
    /// 验证同一用户在宿主与租户上下文中的授权缓存相互隔离
    /// </summary>
    [Fact]
    public async Task PermissionCache_ShouldBeIsolatedByTenant()
    {
        var accountService = GetRequiredService<IAccountService>();
        var userRepository = GetRequiredService<ISqlSugarRepository<UserAggregateRoot>>();
        var roleRepository = GetRequiredService<ISqlSugarRepository<RoleAggregateRoot>>();
        var userRoleRepository = GetRequiredService<ISqlSugarRepository<UserRoleEntity>>();
        var menuRepository = GetRequiredService<ISqlSugarRepository<MenuAggregateRoot>>();
        var roleManager = GetRequiredService<RoleManager>();
        var permissionChecker = GetRequiredService<IPermissionChecker>();
        var principalAccessor = GetRequiredService<ICurrentPrincipalAccessor>();
        var currentTenant = GetRequiredService<ICurrentTenant>();

        var userName = $"tenant_permission_{Guid.NewGuid():N}";
        await accountService.PostRegisterAsync(new RegisterDto
        {
            UserName = userName,
            Password = "123456",
            Phone = Random.Shared.NextInt64(13000000000, 19000000000)
        });

        var user = await userRepository.GetFirstAsync(entity => entity.UserName == userName);
        var menu = await menuRepository.GetFirstAsync(entity => entity.PermissionCode == PostQueryPermission);
        var tenantId = Guid.NewGuid();
        var principal = CreatePrincipal(user.Id);

        using (currentTenant.Change(tenantId))
        {
            var tenantRole = await roleRepository.InsertReturnEntityAsync(new RoleAggregateRoot
            {
                TenantId = tenantId,
                RoleName = "租户权限测试角色",
                RoleCode = $"tenant_permission_{tenantId:N}",
                State = true
            });
            await roleManager.GiveRoleSetMenuAsync([tenantRole.Id], [menu.Id]);
            await userRoleRepository.InsertAsync(new UserRoleEntity
            {
                UserId = user.Id,
                RoleId = tenantRole.Id
            });
        }

        using (principalAccessor.Change(principal))
        {
            (await permissionChecker.IsGrantedAsync(PostQueryPermission)).ShouldBeFalse();

            using (currentTenant.Change(tenantId))
            {
                (await permissionChecker.IsGrantedAsync(PostQueryPermission)).ShouldBeTrue();
            }

            (await permissionChecker.IsGrantedAsync(PostQueryPermission)).ShouldBeFalse();
        }
    }

    /// <summary>
    /// 创建只包含用户标识的已认证测试身份
    /// </summary>
    private static ClaimsPrincipal CreatePrincipal(Guid userId)
    {
        var identity = new ClaimsIdentity(
            [new Claim(AbpClaimTypes.UserId, userId.ToString())],
            authenticationType: "Test");
        return new ClaimsPrincipal(identity);
    }
}
