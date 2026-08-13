using Microsoft.Extensions.DependencyInjection;
using SqlSugar;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Yi.Module.Rbac.Domain.Shared.Authorization;
using Yi.Module.Rbac.Domain.Entities;
using Yi.Framework.SqlSugarCore;

namespace Yi.Module.Rbac.SqlSugarCore
{
    public class YiRbacDbContext : SqlSugarDbContext
    {
        protected IDataFilter DataFilter => LazyServiceProvider.LazyGetRequiredService<IDataFilter>();
        protected IDataPermissionScopeProvider DataPermissionScopeProvider =>
            LazyServiceProvider.LazyGetRequiredService<IDataPermissionScopeProvider>();

        /// <summary>
        /// 在 SqlSugar 查询过滤器初始化阶段按 ABP 数据过滤开关注册数据权限规则
        /// </summary>
        /// <param name="sqlSugarClient">当前 DbContext 使用的 SqlSugar 客户端</param>
        protected override void CustomDataFilter(ISqlSugarClient sqlSugarClient)
        {
            // IDataPermission 只负责控制过滤器启停，便于受控场景通过 ABP IDataFilter 临时禁用数据权限。
            if (DataFilter.IsEnabled<IDataPermission>())
            {
                DataPermissionFilter(sqlSugarClient);
            }
        }

        /// <summary>
        /// 初始化 RBAC 模块 SqlSugar 数据库上下文
        /// </summary>
        /// <param name="lazyServiceProvider">ABP 延迟服务提供器，用于避免在构造阶段提前解析过滤器依赖</param>
        public YiRbacDbContext(IAbpLazyServiceProvider lazyServiceProvider) : base(lazyServiceProvider)
        {
        }

        /// <summary>
        /// 将当前用户的数据权限范围转换为业务实体和用户实体的查询过滤器
        /// </summary>
        /// <param name="sqlSugarClient">当前 DbContext 使用的 SqlSugar 客户端</param>
        protected void DataPermissionFilter(ISqlSugarClient sqlSugarClient)
        {
            var scope = DataPermissionScopeProvider.GetCurrent();
            // 全量权限时不注册空表达式，避免 SqlSugar 将占位条件误解释为有效过滤规则。
            if (scope.IsAll)
            {
                return;
            }

            var deptIds = ResolveDeptIds(sqlSugarClient, scope);
            var includesSelf = scope.IncludesSelf && scope.CurrentUserId.HasValue;

            AddBusinessDataPermissionFilter(sqlSugarClient, scope, deptIds, includesSelf);
            AddRbacUserDataPermissionFilter(sqlSugarClient, scope, deptIds, includesSelf);
        }

        /// <summary>
        /// 为显式实现 <see cref="IHasDataPermission"/> 的业务实体注册部门或本人过滤器
        /// </summary>
        /// <param name="sqlSugarClient">当前 DbContext 使用的 SqlSugar 客户端</param>
        /// <param name="scope">当前用户的数据权限范围</param>
        /// <param name="deptIds">已经展开并去重的可见部门标识</param>
        /// <param name="includesSelf">是否包含当前用户本人创建的数据</param>
        private static void AddBusinessDataPermissionFilter(
            ISqlSugarClient sqlSugarClient,
            DataPermissionScopeResult scope,
            List<Guid> deptIds,
            bool includesSelf)
        {
            // 多角色范围按“部门或本人”取并集，符合一个角色授予的范围不能被另一个角色收窄的语义。
            if (deptIds.Count > 0 && includesSelf)
            {
                var currentUserId = scope.CurrentUserId!.Value;
                sqlSugarClient.QueryFilter.AddTableFilter<IHasDataPermission>(entity =>
                    deptIds.Contains(entity.DeptId ?? Guid.Empty) || entity.CreatorId == currentUserId);
            }
            else if (deptIds.Count > 0)
            {
                sqlSugarClient.QueryFilter.AddTableFilter<IHasDataPermission>(entity =>
                    deptIds.Contains(entity.DeptId ?? Guid.Empty));
            }
            else if (includesSelf)
            {
                var currentUserId = scope.CurrentUserId!.Value;
                sqlSugarClient.QueryFilter.AddTableFilter<IHasDataPermission>(entity =>
                    entity.CreatorId == currentUserId);
            }
            else
            {
                // 没有任何有效范围时采用 fail-closed，防止空条件退化为全表可见。
                sqlSugarClient.QueryFilter.AddTableFilter<IHasDataPermission>(_ => false);
            }
        }

        /// <summary>
        /// 为 RBAC 用户实体注册专用的数据权限过滤器
        /// </summary>
        /// <remarks>
        /// 用户实体的 <c>CreatorId</c> 表示账号创建人，而“仅本人”应匹配账号自身 <c>Id</c>，因此不能复用通用实体表达式
        /// 角色等授权配置实体不接入该过滤器，以保证权限配置查询不会被业务数据范围裁剪
        /// </remarks>
        /// <param name="sqlSugarClient">当前 DbContext 使用的 SqlSugar 客户端</param>
        /// <param name="scope">当前用户的数据权限范围</param>
        /// <param name="deptIds">已经展开并去重的可见部门标识</param>
        /// <param name="includesSelf">是否包含当前用户账号本身</param>
        private static void AddRbacUserDataPermissionFilter(
            ISqlSugarClient sqlSugarClient,
            DataPermissionScopeResult scope,
            List<Guid> deptIds,
            bool includesSelf)
        {
            if (deptIds.Count > 0 && includesSelf)
            {
                var currentUserId = scope.CurrentUserId!.Value;
                sqlSugarClient.QueryFilter.AddTableFilter<UserAggregateRoot>(user =>
                    deptIds.Contains(user.DeptId ?? Guid.Empty) || user.Id == currentUserId);
            }
            else if (deptIds.Count > 0)
            {
                sqlSugarClient.QueryFilter.AddTableFilter<UserAggregateRoot>(user =>
                    deptIds.Contains(user.DeptId ?? Guid.Empty));
            }
            else if (includesSelf)
            {
                var currentUserId = scope.CurrentUserId!.Value;
                sqlSugarClient.QueryFilter.AddTableFilter<UserAggregateRoot>(user =>
                    user.Id == currentUserId);
            }
            else
            {
                // 用户表同样采用 fail-closed，避免无部门且无本人权限时暴露全部账号。
                sqlSugarClient.QueryFilter.AddTableFilter<UserAggregateRoot>(_ => false);
            }
        }

        /// <summary>
        /// 使用当前 SqlSugar 客户端展开部门树和自定义角色部门，返回去重后的可见部门标识
        /// </summary>
        /// <remarks>
        /// 此处不能通过范围提供器注入仓储查询，否则 DbContext 创建过滤器时可能再次解析 DbContext，形成递归依赖
        /// <c>ToChildList</c> 属于 SqlSugar 树查询 API，
        /// 升级 SqlSugar 时需复核其根节点包含语义；本实现显式加入当前部门，不依赖该版本行为
        /// </remarks>
        /// <param name="sqlSugarClient">当前 DbContext 使用的 SqlSugar 客户端</param>
        /// <param name="scope">尚未展开数据库关系的数据权限范围</param>
        /// <returns>已展开并去重的可见部门标识</returns>
        private static List<Guid> ResolveDeptIds(
            ISqlSugarClient sqlSugarClient,
            DataPermissionScopeResult scope)
        {
            var deptIds = new HashSet<Guid>();

            if (scope.IncludesCurrentDept && scope.CurrentDeptId.HasValue)
            {
                deptIds.Add(scope.CurrentDeptId.Value);
            }

            if (scope.IncludesDescendantDepts && scope.CurrentDeptId.HasValue)
            {
                var childDepts = sqlSugarClient.Queryable<DeptAggregateRoot>()
                    .ToChildList(dept => dept.ParentId, scope.CurrentDeptId.Value);
                deptIds.UnionWith(childDepts.Select(dept => dept.Id));
            }

            if (scope.CustomRoleIds.Count > 0)
            {
                // 自定义范围持久化在角色部门关系表中，只在基础设施层展开，保持领域范围计算无数据库依赖。
                var customRoleIds = scope.CustomRoleIds.ToList();
                var customDeptIds = sqlSugarClient.Queryable<RoleDeptEntity>()
                    .Where(roleDept => customRoleIds.Contains(roleDept.RoleId))
                    .Select(roleDept => roleDept.DeptId)
                    .ToList();
                deptIds.UnionWith(customDeptIds);
            }

            return deptIds.ToList();
        }

    }
}
