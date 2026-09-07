using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using System.Security.Claims;
using TencentCloud.Ame.V20190916.Models;
using TencentCloud.Tiw.V20190919.Models;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Security.Claims;
using Xunit;
using Yi.Module.Rbac.Application.Contracts.Dtos.User;
using Yi.Module.Rbac.Application.Contracts.IServices;
using Yi.Module.Rbac.Domain.Entities;
using Yi.Module.Rbac.Domain.Shared.Consts;
using Yi.Module.Rbac.Domain.Shared.Enums;
using Yi.Framework.SqlSugarCore.Abstractions;

namespace Yi.Module.Rbac.Test.System
{
    public class UserFrameworkRbacTest : YiModuleRbacTestBase
    {
        private IUserService _userService;
        private ISqlSugarRepository<UserAggregateRoot> _repository;
        public UserFrameworkRbacTest()
        {
            _userService = ServiceProvider.GetRequiredService<IUserService>();
            _repository = ServiceProvider.GetRequiredService<ISqlSugarRepository<UserAggregateRoot>>();
        }

        /// <summary>
        /// 查询用户
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Get_User_Test()
        {
            await RunAsHostSuperAdminAsync(async () =>
            {
                var user = await _userService.GetListAsync(new UserGetListInputVo { UserName = UserConst.SuperAdminUserName });
                user.ShouldNotBeNull();
            });
        }


        /// <summary>
        /// 创建用户
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Create_User_Test()
        {
            await RunAsHostSuperAdminAsync(async () =>
            {
                await _userService.CreateAsync(new UserCreateInputVo { UserName = "CreateUserTest", Password = "654321" });
                var user = await _userService.GetListAsync(new UserGetListInputVo { UserName = "CreateUserTest" });
                user.ShouldNotBeNull();
            });
        }

        /// <summary>
        /// 更新用户
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Update_User_Test()
        {
            await RunAsHostSuperAdminAsync(async () =>
            {
                var createdUser = await _userService.CreateAsync(new UserCreateInputVo { Nick = "nickTest", Sex = SexEnum.Woman, UserName = "UpdateUserTest", Password = "654321" });
                await _userService.UpdateAsync(createdUser.Id, new UserUpdateInputVo { Nick = "nickTest2", Sex = SexEnum.Woman, UserName = "UpdateUserTest", Password = "123456888abc" });
                var user = await _repository.GetFirstAsync(user => user.UserName == "UpdateUserTest");
                user.ShouldNotBeNull();
                user.Nick.ShouldBe("nickTest2");
                user.Sex.ShouldBe(SexEnum.Woman);
                user.JudgePassword("123456888abc");
            });
        }


        /// <summary>
        /// 删除用户
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Delete_User_Test()
        {
            await RunAsHostSuperAdminAsync(async () =>
            {
                var createdUser = await _userService.CreateAsync(new UserCreateInputVo { UserName = "DeleteUserTest", Password = "123456" });

                var user1 = await _repository.GetFirstAsync(user => user.UserName == "DeleteUserTest");
                user1.ShouldNotBeNull();

                await _userService.DeleteAsync(new List<Guid> { createdUser.Id });
                var user2 = await _repository.GetFirstAsync(user => user.UserName == "DeleteUserTest");
                user2.ShouldBeNull();
            });
        }

        /// <summary>
        /// 在宿主超级管理员身份下执行受权限保护的用户管理用例
        /// </summary>
        /// <param name="action">测试操作</param>
        private async Task RunAsHostSuperAdminAsync(Func<Task> action)
        {
            var principalAccessor = GetRequiredService<ICurrentPrincipalAccessor>();
            var superAdmin = await _repository.GetFirstAsync(user => user.UserName == UserConst.SuperAdminUserName);
            var identity = new ClaimsIdentity(
                [
                    new Claim(AbpClaimTypes.UserId, superAdmin.Id.ToString()),
                    new Claim(AbpClaimTypes.UserName, UserConst.SuperAdminUserName),
                    new Claim(TokenTypeConst.Roles, UserConst.SuperAdminRoleCode)
                ],
                authenticationType: "Test");

            using (principalAccessor.Change(new ClaimsPrincipal(identity)))
            {
                await action();
            }
        }
    }
}
