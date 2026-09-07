using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Http;
using Shouldly;
using Volo.Abp.Security.Claims;
using Volo.Abp.Domain.Repositories;
using Xunit;
using Yi.Module.Rbac.Application.Contracts.Dtos.Account;
using Yi.Module.Rbac.Application.Contracts.Dtos.User;
using Yi.Module.Rbac.Application.Contracts.IServices;
using Yi.Module.Rbac.Application.Services;
using Yi.Module.Rbac.Domain.Entities;
using Yi.Module.Rbac.Domain.Managers;
using Yi.Module.Rbac.Domain.Shared.Consts;
using Yi.Module.Rbac.Test;
using Yi.Framework.SqlSugarCore.Abstractions;

namespace Yi.Module.Rbac.Test.System
{
    public class AccountFrameworkRbacTest : YiModuleRbacTestWebBase
    {

        private IAccountService _accountService;
        private ISqlSugarRepository<UserAggregateRoot> _userRepository;
        public AccountFrameworkRbacTest()
        {
            _accountService = GetRequiredService<IAccountService>();
            _userRepository = GetRequiredService<ISqlSugarRepository<UserAggregateRoot>>();
        }

        /// <summary>
        /// 注册
        /// </summary>
        [Fact]
        public async Task Register_Test()
        {
            await _accountService.PostRegisterAsync(new RegisterDto() { UserName = "RegisterTest", Password = "123456", Phone = 15945645645 });
            var user = await _userRepository.GetFirstAsync(user => user.UserName == "RegisterTest");
            user.ShouldNotBeNull();
            user.JudgePassword("123456").ShouldBeTrue();
        }

        /// <summary>
        /// 用户名重复注册
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Register_UserNameRepeat_Error_Test()
        {
            try
            {
                await _accountService.PostRegisterAsync(new RegisterDto() { UserName = "RegisterUserNameRepeatErrorTest", Password = "123456", Phone = 15945645641 });
                await _accountService.PostRegisterAsync(new RegisterDto() { UserName = "RegisterUserNameRepeatErrorTest", Password = "123456", Phone = 15945645642 });
            }
            catch (UserFriendlyException ex)
            {
                ex.Message.ShouldBe(UserConst.Exist);
            }
        }

        /// <summary>
        /// 电话号码重复注册
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Register_PhoneRepeat_Error_Test()
        {
            try
            {
                await _accountService.PostRegisterAsync(new RegisterDto() { UserName = "RegisterPhoneRepeatErrorTest1", Password = "123456", Phone = 15945645633 });
                await _accountService.PostRegisterAsync(new RegisterDto() { UserName = "RegisterPhoneRepeatErrorTest2", Password = "123456", Phone = 15945645633 });
            }
            catch (UserFriendlyException ex)
            {
                ex.Message.ShouldBe(UserConst.Phone_Repeat);
            }
        }


        /// <summary>
        /// 登录测试
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Login_Test()
        {
            await GrantDefaultRolePermissionAsync();
            await _accountService.PostRegisterAsync(new RegisterDto() { UserName = "LoginTest", Password = "123456", Phone = 13845645645 });
            var result = await _accountService.PostLoginAsync(new LoginInputVo { UserName = "LoginTest", Password = "123456" });

            var accessToken = result.GetType().GetProperty("Token").GetValue(result, null).ToString();
            accessToken.ShouldNotBeNull();
            result.GetType().GetProperty("RefreshToken").GetValue(result, null).ToString().ShouldNotBeNull();

            var claims = new JwtSecurityTokenHandler().ReadJwtToken(accessToken).Claims;
            claims.ShouldNotContain(claim => claim.Type == "Permission");
        }

        /// <summary>
        /// 重置密码
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Reset_Passworld_Test()
        {
            await GrantDefaultRolePermissionAsync("system:user:resetPwd");
            await _accountService.PostRegisterAsync(new RegisterDto() { UserName = "ResetPassworldTest", Password = "123456", Phone = 15945645555 });
            var user = await _userRepository.GetFirstAsync(user => user.UserName == "ResetPassworldTest");
            var principalAccessor = GetRequiredService<ICurrentPrincipalAccessor>();
            using (principalAccessor.Change(CreatePrincipal(user.Id)))
            {
                await _accountService.ResetPasswordAsync(user.Id, new RestPasswordDto { Password = "654321abc" });
            }
            var result = await _accountService.PostLoginAsync(new LoginInputVo { UserName = "ResetPassworldTest", Password = "654321abc" });

        }

        /// <summary>
        /// 为登录测试准备具备最小菜单权限的默认角色
        /// </summary>
        /// <param name="permissionCode">指定权限码，为空时使用岗位查询权限</param>
        private async Task GrantDefaultRolePermissionAsync(string? permissionCode = null)
        {
            var roleManager = GetRequiredService<RoleManager>();
            var roleRepository = GetRequiredService<ISqlSugarRepository<RoleAggregateRoot>>();
            var menuRepository = GetRequiredService<ISqlSugarRepository<MenuAggregateRoot>>();
            var role = await roleRepository.GetFirstAsync(entity => entity.RoleCode == UserConst.DefaultRoleCode);
            permissionCode ??= "system:post:query";
            var menu = await menuRepository.GetFirstAsync(entity => entity.PermissionCode == permissionCode);

            await roleManager.GiveRoleSetMenuAsync([role.Id], [menu.Id]);
        }

        /// <summary>
        /// 创建只包含用户标识的已认证测试身份
        /// </summary>
        /// <param name="userId">用户标识</param>
        private static ClaimsPrincipal CreatePrincipal(Guid userId)
        {
            var identity = new ClaimsIdentity(
                [new Claim(AbpClaimTypes.UserId, userId.ToString())],
                authenticationType: "Test");
            return new ClaimsPrincipal(identity);
        }
    }
}
