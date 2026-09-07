using Volo.Abp.Domain.Services;
using Volo.Abp.MultiTenancy;
using Yi.Module.Rbac.Domain.Entities;
using Yi.Module.Rbac.Domain.Shared.Authorization;
using Yi.Framework.SqlSugarCore.Abstractions;

namespace Yi.Module.Rbac.Domain.Managers
{
    public class RoleManager : DomainService
    {
        private ISqlSugarRepository<RoleAggregateRoot> _repository;
        private ISqlSugarRepository<RoleMenuEntity> _roleMenuRepository;
        private readonly IPermissionCacheVersionManager _permissionCacheVersionManager;
        private readonly ICurrentTenant _currentTenant;

        /// <summary>
        /// 创建负责角色菜单授权及缓存失效的领域服务
        /// </summary>
        public RoleManager(
            ISqlSugarRepository<RoleAggregateRoot> repository,
            ISqlSugarRepository<RoleMenuEntity> roleMenuRepository,
            IPermissionCacheVersionManager permissionCacheVersionManager,
            ICurrentTenant currentTenant)
        {
            _repository = repository;
            _roleMenuRepository = roleMenuRepository;
            _permissionCacheVersionManager = permissionCacheVersionManager;
            _currentTenant = currentTenant;
        }

        /// <summary>
        /// 给角色设置菜单
        /// </summary>
        /// <param name="roleIds"></param>
        /// <param name="menuIds"></param>
        /// <returns></returns>
        public async Task GiveRoleSetMenuAsync(List<Guid> roleIds, List<Guid> menuIds)
        {
            //这个是需要事务的，在service中进行工作单元
            await _roleMenuRepository.DeleteAsync(u => roleIds.Contains(u.RoleId));
            //遍历用户
            foreach (var roleId in roleIds)
            {
                //添加新的关系
                List<RoleMenuEntity> roleMenuEntity = new();
                foreach (var menu in menuIds)
                {
                    roleMenuEntity.Add(new RoleMenuEntity() { RoleId = roleId, MenuId = menu });
                }
                //一次性批量添加
                await _roleMenuRepository.InsertRangeAsync(roleMenuEntity);
            }

            await _permissionCacheVersionManager.InvalidateAsync(_currentTenant.Id);
        }
    }
}
