using System.Reflection;
using Shouldly;
using SqlSugar;
using Xunit;
using Yi.Module.Rbac.Domain.Entities;
using Yi.Module.Rbac.Domain.Shared.Authorization;

namespace Yi.Module.Rbac.Test.Authorization;

public class DataPermissionQueryFilterTest
{
    /// <summary>
    /// 使用真实部门树和岗位实体验证当前部门、下级部门及本人创建数据可见，越权岗位不可见，
    /// 同时确认作为权限配置来源的部门实体不会被业务数据过滤器裁剪
    /// </summary>
    [Fact]
    public void Should_Filter_Posts_By_Department_Tree_And_Creator()
    {
        var currentDeptId = Guid.NewGuid();
        var childDeptId = Guid.NewGuid();
        var outsideDeptId = Guid.NewGuid();
        var currentUserId = Guid.NewGuid();
        var db = new SqlSugarClient(new ConnectionConfig
        {
            DbType = DbType.Sqlite,
            ConnectionString = "DataSource=:memory:",
            IsAutoCloseConnection = false,
            ConfigureExternalServices = new ConfigureExternalServices
            {
                // 复用生产 DefaultSqlSugarDbContext 的可空列识别规则；直接使用 CodeFirst 时，
                // 当前 SqlSugar 版本不会仅凭 Guid? 正确生成所有可空列，ORM 升级后需复核这一兼容配置。
                EntityService = (property, column) =>
                {
                    if (new NullabilityInfoContext().Create(property).WriteState is NullabilityState.Nullable)
                    {
                        column.IsNullable = true;
                    }
                }
            }
        });

        try
        {
            db.Ado.Open();
            db.CodeFirst.InitTables<DeptAggregateRoot, PostAggregateRoot>();
            db.Insertable(new[]
            {
                Department(currentDeptId, Guid.Empty, "Current"),
                Department(childDeptId, currentDeptId, "Child"),
                Department(outsideDeptId, Guid.Empty, "Outside")
            }).ExecuteCommand();
            db.Insertable(new[]
            {
                Post(currentDeptId, Guid.NewGuid(), "CurrentDept"),
                Post(childDeptId, Guid.NewGuid(), "ChildDept"),
                Post(outsideDeptId, currentUserId, "CreatedByCurrentUser"),
                Post(outsideDeptId, Guid.NewGuid(), "Invisible")
            }).ExecuteCommand();

            var deptIds = db.Queryable<DeptAggregateRoot>()
                .ToChildList(dept => dept.ParentId, currentDeptId)
                .Select(dept => dept.Id)
                .Append(currentDeptId)
                .Distinct()
                .ToList();
            // 接口级 AddTableFilter 映射到具体岗位属性依赖当前 SqlSugar 行为，本测试同时承担 ORM 升级回归验证。
            db.QueryFilter.AddTableFilter<IHasDataPermission>(entity =>
                deptIds.Contains(entity.DeptId ?? Guid.Empty) || entity.CreatorId == currentUserId);

            var visiblePosts = db.Queryable<PostAggregateRoot>().ToList();
            var visibleDepartments = db.Queryable<DeptAggregateRoot>().ToList();

            visiblePosts.Count.ShouldBe(3);
            visiblePosts.ShouldContain(post => post.PostCode == "CurrentDept");
            visiblePosts.ShouldContain(post => post.PostCode == "ChildDept");
            visiblePosts.ShouldContain(post => post.PostCode == "CreatedByCurrentUser");
            visiblePosts.ShouldNotContain(post => post.PostCode == "Invisible");
            visibleDepartments.Count.ShouldBe(3);
        }
        finally
        {
            db.Dispose();
        }
    }

    /// <summary>
    /// 构造用于验证部门树展开的数据样本
    /// </summary>
    /// <param name="id">部门标识</param>
    /// <param name="parentId">上级部门标识</param>
    /// <param name="code">部门编码及名称</param>
    /// <returns>部门聚合根</returns>
    private static DeptAggregateRoot Department(Guid id, Guid parentId, string code)
    {
        return new DeptAggregateRoot(id, parentId)
        {
            DeptName = code,
            DeptCode = code
        };
    }

    /// <summary>
    /// 构造用于验证数据权限过滤的岗位样本
    /// </summary>
    /// <remarks>
    /// 反射设置受保护的聚合根标识仅用于准备持久化测试数据，绕过了领域对象正常创建路径；
    /// 生产代码仍应由框架 GUID 生成器或领域构造过程分配标识，框架实体实现升级时需复核此测试辅助逻辑
    /// </remarks>
    /// <param name="deptId">岗位归属部门标识</param>
    /// <param name="creatorId">岗位创建用户标识</param>
    /// <param name="code">岗位编码及名称</param>
    /// <returns>岗位聚合根</returns>
    private static PostAggregateRoot Post(Guid deptId, Guid creatorId, string code)
    {
        var post = new PostAggregateRoot
        {
            DeptId = deptId,
            CreatorId = creatorId,
            PostCode = code,
            PostName = code
        };
        typeof(PostAggregateRoot).GetProperty(nameof(PostAggregateRoot.Id))!
            .SetValue(post, Guid.NewGuid());
        return post;
    }
}
