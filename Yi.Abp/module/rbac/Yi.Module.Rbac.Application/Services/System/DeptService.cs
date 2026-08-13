using Microsoft.AspNetCore.Mvc;
using SqlSugar;
using Volo.Abp.Application.Dtos;
using Yi.Framework.Core.Helper;
using Yi.Framework.Ddd.Application;
using Yi.Module.Rbac.Application.Contracts.Dtos.Dept;
using Yi.Module.Rbac.Application.Contracts.IServices;
using Yi.Module.Rbac.Domain.Entities;
using Yi.Module.Rbac.Domain.Repositories;
using Yi.Framework.Authorization.Abstractions.Attributes;
using Yi.Framework.Authorization.Abstractions.Enums;
using Yi.Framework.OperationRecord.Abstractions.Attributes;
using Yi.Module.Rbac.Domain.Shared.Consts;
using Yi.Module.Rbac.Domain.Shared.Dtos;
using Yi.Module.Rbac.Domain.Shared.Authorization;
using Yi.Framework.SqlSugarCore.Abstractions;

namespace Yi.Module.Rbac.Application.Services
{
    /// <summary>
    /// Dept服务实现
    /// </summary>
    [PermissionResource("system", "dept")]
    [OperLogEntity("部门")]
    public class DeptService : YiCrudAppService<DeptAggregateRoot, DeptGetOutputDto, DeptGetListOutputDto, Guid,
        DeptGetListInputVo, DeptCreateInputVo, DeptUpdateInputVo>, IDeptService
    {
        private IDeptRepository _repository;
        private readonly IDataPermissionScopeProvider _dataPermissionScopeProvider;
        private readonly ISqlSugarRepository<RoleDeptEntity> _roleDeptRepository;

        /// <summary>
        /// 初始化部门服务并注入当前用户数据权限范围提供器
        /// </summary>
        /// <param name="repository">部门仓储</param>
        /// <param name="dataPermissionScopeProvider">当前用户数据权限范围提供器</param>
        /// <param name="roleDeptRepository">角色部门关系仓储</param>
        public DeptService(
            IDeptRepository repository,
            IDataPermissionScopeProvider dataPermissionScopeProvider,
            ISqlSugarRepository<RoleDeptEntity> roleDeptRepository) : base(repository)
        {
            _repository = repository;
            _dataPermissionScopeProvider = dataPermissionScopeProvider;
            _roleDeptRepository = roleDeptRepository;
        }

        /// <summary>
        /// 获取供业务页面选择使用的部门基础数据
        /// </summary>
        /// <param name="keywords">部门查询关键字</param>
        /// <returns>部门选择数据列表</returns>
        /// <remarks>该接口是岗位、用户等页面的公共读取依赖，不要求调用方同时拥有部门管理权限；返回值仍按当前数据权限裁剪，并补齐从顶级部门到目标部门的完整路径；仅跳过资源权限校验，ABP 默认认证仍然生效，框架授权行为升级时需要复核</remarks>
        [IgnorePermission]
        public override async Task<List<DeptGetListOutputDto>> GetSelectDataListAsync(string? keywords = null)
        {
            var allDepts = await _repository._DbQueryable
                .Where(dept => dept.State == true)
                .OrderBy(dept => dept.OrderNum, OrderByType.Asc)
                .ToListAsync();
            var scope = _dataPermissionScopeProvider.GetCurrent();

            if (scope.IsAll)
            {
                return MapDeptSelectData(allDepts, keywords);
            }

            var visibleDeptIds = new HashSet<Guid>();
            if (scope.IncludesCurrentDept && scope.CurrentDeptId.HasValue)
            {
                visibleDeptIds.Add(scope.CurrentDeptId.Value);
            }

            if (scope.IncludesDescendantDepts && scope.CurrentDeptId.HasValue)
            {
                AddChildDeptIds(scope.CurrentDeptId.Value, allDepts, visibleDeptIds);
            }

            if (scope.CustomRoleIds.Count > 0)
            {
                var customRoleIds = scope.CustomRoleIds.ToList();
                var customDeptIds = await _roleDeptRepository._DbQueryable
                    .Where(roleDept => customRoleIds.Contains(roleDept.RoleId))
                    .Select(roleDept => roleDept.DeptId)
                    .ToListAsync();
                visibleDeptIds.UnionWith(customDeptIds);
            }

            // 上级部门节点只用于维持树的层级结构，不代表调用方获得上级部门的业务数据权限
            AddParentDeptIds(allDepts, visibleDeptIds);
            var visibleDepts = allDepts.Where(dept => visibleDeptIds.Contains(dept.Id)).ToList();
            return MapDeptSelectData(visibleDepts, keywords);
        }

        /// <summary>
        /// 递归收集当前部门的全部启用子部门
        /// </summary>
        /// <param name="parentId">父部门标识</param>
        /// <param name="allDepts">全部启用部门</param>
        /// <param name="visibleDeptIds">可见部门标识集合</param>
        private static void AddChildDeptIds(
            Guid parentId,
            IReadOnlyCollection<DeptAggregateRoot> allDepts,
            ISet<Guid> visibleDeptIds)
        {
            foreach (var child in allDepts.Where(dept => dept.ParentId == parentId))
            {
                if (visibleDeptIds.Add(child.Id))
                {
                    AddChildDeptIds(child.Id, allDepts, visibleDeptIds);
                }
            }
        }

        /// <summary>
        /// 补齐可见部门的上级部门节点以维持前端树结构
        /// </summary>
        /// <param name="allDepts">全部启用部门</param>
        /// <param name="visibleDeptIds">可见部门标识集合</param>
        private static void AddParentDeptIds(
            IReadOnlyCollection<DeptAggregateRoot> allDepts,
            ISet<Guid> visibleDeptIds)
        {
            var deptMap = allDepts.ToDictionary(dept => dept.Id);
            foreach (var deptId in visibleDeptIds.ToArray())
            {
                var currentId = deptId;
                while (deptMap.TryGetValue(currentId, out var current) && current.ParentId != Guid.Empty)
                {
                    if (!visibleDeptIds.Add(current.ParentId))
                    {
                        break;
                    }

                    currentId = current.ParentId;
                }
            }
        }

        /// <summary>
        /// 将授权后的部门实体映射为选择数据并应用关键字过滤
        /// </summary>
        /// <param name="depts">授权后的部门实体</param>
        /// <param name="keywords">部门查询关键字</param>
        /// <returns>部门选择数据列表</returns>
        private static List<DeptGetListOutputDto> MapDeptSelectData(
            IReadOnlyCollection<DeptAggregateRoot> depts,
            string? keywords)
        {
            var query = depts.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(keywords))
            {
                query = query.Where(dept =>
                    dept.DeptName.Contains(keywords, StringComparison.OrdinalIgnoreCase) ||
                    dept.DeptCode.Contains(keywords, StringComparison.OrdinalIgnoreCase));
            }

            return query.Select(dept => new DeptGetListOutputDto
            {
                Id = dept.Id,
                CreationTime = dept.CreationTime,
                CreatorId = dept.CreatorId,
                State = dept.State,
                DeptName = dept.DeptName,
                DeptCode = dept.DeptCode,
                Leader = dept.Leader,
                ParentId = dept.ParentId,
                Remark = dept.Remark,
                OrderNum = dept.OrderNum
            }).ToList();
        }

        [RemoteService(false)]
        public async Task<List<Guid>> GetChildListAsync(Guid deptId)
        {
            return await _repository.GetChildListAsync(deptId);
        }

        /// <summary>
        /// 通过角色id查询该角色全部部门
        /// </summary>
        /// <param name="roleId"></param>
        /// <returns></returns>
        //[Route("{roleId}")]
        [PermissionAction(PermissionActionEnum.Query)]
        public async Task<List<DeptGetListOutputDto>> GetRoleIdAsync(Guid roleId)
        {
            var result = await _repository._DbQueryable
                .InnerJoin<RoleDeptEntity>((dept, roleDept) => dept.Id == roleDept.DeptId && roleDept.RoleId == roleId)
                .LeftJoin<UserAggregateRoot>((dept, roleDept, user) => dept.Leader == user.Id)
                .OrderBy((dept, roleDept, user) => dept.OrderNum, OrderByType.Asc)
                .Select((dept, roleDept, user) => new DeptGetListOutputDto
                {
                    Id = dept.Id,
                    CreationTime = dept.CreationTime,
                    CreatorId = dept.CreatorId,
                    State = dept.State,
                    DeptName = dept.DeptName,
                    DeptCode = dept.DeptCode,
                    Leader = dept.Leader,
                    LeaderName = user.Name,
                    ParentId = dept.ParentId,
                    Remark = dept.Remark,
                    OrderNum = dept.OrderNum
                })
                .ToListAsync();
            
            return result;
        }

        /// <summary>
        /// 多查
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [Route("dept/list")]
        [PermissionAction(PermissionActionEnum.Query)]
        public new async Task<List<DeptGetListOutputDto>> GetListAsync(DeptGetListInputVo input)
        {
            var result = await _repository._DbQueryable
                .WhereIF(!string.IsNullOrEmpty(input.DeptName), u => u.DeptName.Contains(input.DeptName!))
                .WhereIF(input.State is not null, u => u.State == input.State)
                .LeftJoin<UserAggregateRoot>((dept, user) => dept.Leader == user.Id)
                .OrderBy((dept, user) => dept.OrderNum, OrderByType.Asc)
                .Select((dept, user) => new DeptGetListOutputDto
                {
                    Id = dept.Id,
                    CreationTime = dept.CreationTime,
                    CreatorId = dept.CreatorId,
                    State = dept.State,
                    DeptName = dept.DeptName,
                    DeptCode = dept.DeptCode,
                    Leader = dept.Leader,
                    LeaderName = user.Name,
                    ParentId = dept.ParentId,
                    Remark = dept.Remark,
                    OrderNum = dept.OrderNum
                })
                .ToListAsync();
            
            return result;
        }

        protected override async Task CheckCreateInputDtoAsync(DeptCreateInputVo input)
        {
            var isExist =
                await _repository.IsAnyAsync(x => x.DeptCode == input.DeptCode);
            if (isExist)
            {
                throw new UserFriendlyException(DeptConst.Exist);
            }
        }

        protected override async Task CheckUpdateInputDtoAsync(DeptAggregateRoot entity, DeptUpdateInputVo input)
        {
            // 校验部门编码唯一性
            var isExist = await _repository._DbQueryable.Where(x => x.Id != entity.Id)
                .AnyAsync(x => x.DeptCode == input.DeptCode);
            if (isExist)
            {
                throw new UserFriendlyException(DeptConst.Exist);
            }

            // 校验上级部门不能是自己或自己的子孙部门
            if (input.ParentId.HasValue && input.ParentId.Value != Guid.Empty)
            {
                // 不能将自己设置为上级部门
                if (input.ParentId.Value == entity.Id)
                {
                    throw new UserFriendlyException("上级部门不能是自己");
                }

                // 获取当前部门的所有子孙部门ID
                var childrenIds = await GetAllChildrenIdsAsync(entity.Id);
                
                // 上级部门不能是当前部门的子孙部门
                if (childrenIds.Contains(input.ParentId.Value))
                {
                    throw new UserFriendlyException("上级部门不能是当前部门的子孙部门，这会造成循环引用");
                }
            }
        }

        // /// <summary>
        // /// 获取树形结构的部门列表
        // /// </summary>
        // /// <returns>树形结构的部门列表</returns>
        // public async Task<List<TreeDto>> GetTreeAsync()
        // {
        //     // 获取所有启用的部门
        //     var entities = await _repository._DbQueryable
        //         .Where(x => x.State == true)
        //         .OrderBy(x => x.OrderNum, OrderByType.Asc)
        //         .ToListAsync();
        //
        //     // 将部门实体转换为标准 TreeDto
        //     var treeDtos = entities.Select(dept => new TreeDto
        //     {
        //         Id = dept.Id,
        //         ParentId = dept.ParentId,
        //         Label = dept.DeptName,
        //         Weight = dept.OrderNum,
        //         Disabled = !dept.State,
        //         Children = new List<TreeDto>()
        //     }).ToList();
        //
        //     // 使用 Vben5TreeHelper 构建树形结构
        //     return  Vben5TreeHelper.BuildTree(treeDtos);
        // }
        
        /// <summary>
        /// 获取树形结构的部门列表
        /// </summary>
        /// <returns>树形结构的部门列表</returns>
        [PermissionAction(PermissionActionEnum.Query)]
        public async Task<List<DeptTreeDto>> GetTreeAsync()
        {
            // 获取所有启用的部门
            var entities = await _repository._DbQueryable
                .Where(x => x.State == true)
                .OrderBy(x => x.OrderNum, OrderByType.Asc)
                .ToListAsync();
            return entities.DeptTreeBuild();
        }

        /// <summary>
        /// 获取排除指定部门及其子孙部门的部门列表
        /// </summary>
        /// <param name="id">要排除的部门ID</param>
        /// <returns>排除后的部门列表</returns>
        [HttpGet]
        [Route("dept/list/exclude/{id}")]
        [PermissionAction(PermissionActionEnum.Query)]
        public async Task<List<DeptGetListOutputDto>> GetListExcludeAsync(Guid id)
        {
            // 获取要排除的部门及其所有子孙部门的ID
            var excludeIds = await GetAllChildrenIdsAsync(id);
            excludeIds.Add(id); // 同时排除自己

            // 查询并过滤掉排除的部门，同时 Join 用户表获取负责人姓名
            var result = await _repository._DbQueryable
                .Where(x => !excludeIds.Contains(x.Id))
                .LeftJoin<UserAggregateRoot>((dept, user) => dept.Leader == user.Id)
                .OrderBy((dept, user) => dept.OrderNum, OrderByType.Asc)
                .Select((dept, user) => new DeptGetListOutputDto
                {
                    Id = dept.Id,
                    CreationTime = dept.CreationTime,
                    CreatorId = dept.CreatorId,
                    State = dept.State,
                    DeptName = dept.DeptName,
                    DeptCode = dept.DeptCode,
                    Leader = dept.Leader,
                    LeaderName = user.Name,
                    ParentId = dept.ParentId,
                    Remark = dept.Remark,
                    OrderNum = dept.OrderNum
                })
                .ToListAsync();

            return result;
        }

        /// <summary>
        /// 递归获取指定部门的所有子孙部门ID
        /// </summary>
        /// <param name="deptId">部门ID</param>
        /// <returns>所有子孙部门ID列表</returns>
        private async Task<List<Guid>> GetAllChildrenIdsAsync(Guid deptId)
        {
            var result = new List<Guid>();
            
            // 获取所有部门
            var allDepts = await _repository._DbQueryable.ToListAsync();
            
            // 递归获取子孙部门ID
            GetChildrenIdsRecursive(deptId, allDepts, result);
            
            return result;
        }

        /// <summary>
        /// 递归辅助方法：获取子孙部门ID
        /// </summary>
        /// <param name="parentId">父部门ID</param>
        /// <param name="allDepts">所有部门列表</param>
        /// <param name="result">结果列表</param>
        private void GetChildrenIdsRecursive(Guid parentId, List<DeptAggregateRoot> allDepts, List<Guid> result)
        {
            // 查找直接子部门
            var children = allDepts.Where(x => x.ParentId == parentId).ToList();
            
            foreach (var child in children)
            {
                // 添加子部门ID
                result.Add(child.Id);
                
                // 递归获取子部门的子部门
                GetChildrenIdsRecursive(child.Id, allDepts, result);
            }
        }

    }
}
