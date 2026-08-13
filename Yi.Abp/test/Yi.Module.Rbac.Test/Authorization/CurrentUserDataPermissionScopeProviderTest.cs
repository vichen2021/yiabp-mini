using System.Security.Claims;
using NSubstitute;
using Shouldly;
using Volo.Abp.Users;
using Xunit;
using Yi.Module.Rbac.Domain.Authorization;
using Yi.Module.Rbac.Domain.Shared.Consts;

namespace Yi.Module.Rbac.Test.Authorization;

public class CurrentUserDataPermissionScopeProviderTest
{
    /// <summary>
    /// 验证刷新令牌调用链不注册业务数据过滤，避免令牌刷新所需查询被权限范围截断
    /// </summary>
    [Fact]
    public void Should_Bypass_Data_Permission_For_Refresh_Token()
    {
        var currentUser = CreateCurrentUser();
        currentUser.FindClaims(TokenTypeConst.Refresh)
            .Returns([new Claim(TokenTypeConst.Refresh, bool.TrueString)]);

        var result = new CurrentUserDataPermissionScopeProvider(currentUser).GetCurrent();

        result.IsAll.ShouldBeTrue();
    }

    /// <summary>
    /// 验证超级管理员沿用系统约定绕过数据权限过滤
    /// </summary>
    [Fact]
    public void Should_Bypass_Data_Permission_For_Super_Admin()
    {
        var currentUser = CreateCurrentUser();
        currentUser.UserName.Returns(UserConst.SuperAdminUserName);

        var result = new CurrentUserDataPermissionScopeProvider(currentUser).GetCurrent();

        result.IsAll.ShouldBeTrue();
    }

    /// <summary>
    /// 创建具备默认登录身份、且不携带角色和刷新令牌声明的当前用户替身
    /// </summary>
    /// <returns>用于范围提供器测试的当前用户替身</returns>
    private static ICurrentUser CreateCurrentUser()
    {
        var currentUser = Substitute.For<ICurrentUser>();
        currentUser.Id.Returns(Guid.NewGuid());
        currentUser.UserName.Returns("data-permission-user");
        currentUser.Roles.Returns([]);
        currentUser.FindClaims(Arg.Any<string>()).Returns([]);
        return currentUser;
    }
}
