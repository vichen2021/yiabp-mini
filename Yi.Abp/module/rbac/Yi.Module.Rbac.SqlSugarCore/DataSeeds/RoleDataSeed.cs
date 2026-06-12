using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Yi.Module.Rbac.Domain.Entities;
using Yi.Module.Rbac.Domain.Shared.Consts;
using Yi.Module.Rbac.Domain.Shared.Enums;
using Yi.Framework.SqlSugarCore.Abstractions;

namespace Yi.Module.Rbac.SqlSugarCore.DataSeeds
{
    public class RoleDataSeed : IDataSeedContributor, ITransientDependency
    {
        private ISqlSugarRepository<RoleAggregateRoot> _repository;
        public RoleDataSeed(ISqlSugarRepository<RoleAggregateRoot> repository)
        {
            _repository = repository;
        }

        public List<RoleAggregateRoot> GetSeedData(bool isHost = true)
        {
            var entities = new List<RoleAggregateRoot>();
            RoleAggregateRoot role1 = new RoleAggregateRoot()
            {

                RoleName = isHost ? "超级管理员" : "管理员",
                RoleCode = isHost ? UserConst.SuperAdminRoleCode : UserConst.AdminRoleCode,
                DataScope = DataScopeEnum.ALL,
                OrderNum = 999,
                Remark = isHost ? "平台超级管理员" : "租户管理员",
                IsDeleted = false
            };
            entities.Add(role1);

            RoleAggregateRoot role2 = new RoleAggregateRoot()
            {

                RoleName = "测试角色",
                RoleCode = "test",
                DataScope = DataScopeEnum.ALL,
                OrderNum = 1,
                Remark = "测试用的角色",
                IsDeleted = false
            };
            entities.Add(role2);

            RoleAggregateRoot role3 = new RoleAggregateRoot()
            {

                RoleName = "普通角色",
                RoleCode = "common",
                DataScope = DataScopeEnum.ALL,
                OrderNum = 1,
                Remark = "正常用户",
                IsDeleted = false
            };
            entities.Add(role3);

            RoleAggregateRoot role4 = new RoleAggregateRoot()
            {

                RoleName = "默认角色",
                RoleCode = "default",
                DataScope = DataScopeEnum.ALL,
                OrderNum = 1,
                Remark = "可简单浏览",
                IsDeleted = false
            };
            entities.Add(role4);


            return entities;
        }

        public async Task SeedAsync(DataSeedContext context)
        {
            var isHost = context.TenantId is null;
            if (isHost)
            {
                await SeedHostSuperAdminRoleAsync();
            }
            else
            {
                await SeedTenantAdminRoleAsync();
            }

            if (!await _repository.IsAnyAsync(x => true))
            {
                await _repository.InsertManyAsync(GetSeedData(isHost));
            }
        }

        private async Task SeedHostSuperAdminRoleAsync()
        {
            var superAdminRole = await _repository.GetFirstAsync(x =>
                x.RoleCode == UserConst.SuperAdminRoleCode);
            var legacySuperAdminRole = await _repository.GetFirstAsync(x =>
                x.RoleCode == UserConst.LegacySuperAdminRoleCode);
            if (superAdminRole is null && legacySuperAdminRole is not null)
            {
                legacySuperAdminRole.RoleName = "超级管理员";
                legacySuperAdminRole.RoleCode = UserConst.SuperAdminRoleCode;
                legacySuperAdminRole.Remark = "平台超级管理员";
                await _repository.UpdateAsync(legacySuperAdminRole);
                superAdminRole = legacySuperAdminRole;
            }

            if (superAdminRole is null && await _repository.IsAnyAsync(x => true))
            {
                await _repository.InsertAsync(GetSeedData(true)[0]);
            }
        }

        private async Task SeedTenantAdminRoleAsync()
        {
            var tenantAdminRole = await _repository.GetFirstAsync(x =>
                x.RoleCode == UserConst.AdminRoleCode);
            var invalidSuperAdminRole = await _repository.GetFirstAsync(x =>
                x.RoleCode == UserConst.SuperAdminRoleCode);
            if (tenantAdminRole is null && invalidSuperAdminRole is not null)
            {
                invalidSuperAdminRole.RoleName = "管理员";
                invalidSuperAdminRole.RoleCode = UserConst.AdminRoleCode;
                invalidSuperAdminRole.Remark = "租户管理员";
                await _repository.UpdateAsync(invalidSuperAdminRole);
                tenantAdminRole = invalidSuperAdminRole;
            }
            else if (tenantAdminRole is not null && invalidSuperAdminRole is not null)
            {
                await _repository._Db.Deleteable<UserRoleEntity>()
                    .Where(x => x.RoleId == invalidSuperAdminRole.Id)
                    .ExecuteCommandAsync();
                await _repository._Db.Deleteable<RoleMenuEntity>()
                    .Where(x => x.RoleId == invalidSuperAdminRole.Id)
                    .ExecuteCommandAsync();
                await _repository._Db.Deleteable<RoleDeptEntity>()
                    .Where(x => x.RoleId == invalidSuperAdminRole.Id)
                    .ExecuteCommandAsync();
                await _repository.DeleteAsync(invalidSuperAdminRole);
            }

            if (tenantAdminRole is null && await _repository.IsAnyAsync(x => true))
            {
                await _repository.InsertAsync(GetSeedData(false)[0]);
            }
        }
    }
}
