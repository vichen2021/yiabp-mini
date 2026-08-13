using Shouldly;
using SqlSugar;
using Xunit;
using Yi.Module.Rbac.Domain.Shared.Authorization;

namespace Yi.Module.Rbac.Test.Authorization;

public class DataPermissionQueryFilterTest
{
    /// <summary>
    /// 验证 SqlSugar 接口级过滤器只作用于显式接入数据权限契约的实体，并按部门或本人范围取并集
    /// </summary>
    [Fact]
    public void Should_Apply_Interface_Filter_To_Opted_In_Entity()
    {
        var allowedDeptId = Guid.NewGuid();
        var currentUserId = Guid.NewGuid();
        var db = new SqlSugarClient(new ConnectionConfig
        {
            DbType = DbType.Sqlite,
            ConnectionString = "DataSource=:memory:",
            IsAutoCloseConnection = false
        });

        try
        {
            db.Ado.Open();
            db.CodeFirst.InitTables<DataPermissionTestEntity>();
            db.Insertable(new[]
            {
                Entity(allowedDeptId, Guid.NewGuid()),
                Entity(Guid.NewGuid(), currentUserId),
                Entity(Guid.NewGuid(), Guid.NewGuid())
            }).ExecuteCommand();

            var deptIds = new List<Guid> { allowedDeptId };
            // 接口级 AddTableFilter 映射到具体实体属性依赖当前 SqlSugar 行为，本测试同时承担 ORM 升级回归验证
            db.QueryFilter.AddTableFilter<IHasDataPermission>(entity =>
                deptIds.Contains(entity.DeptId ?? Guid.Empty) || entity.CreatorId == currentUserId);

            var visibleEntities = db.Queryable<DataPermissionTestEntity>().ToList();

            visibleEntities.Count.ShouldBe(2);
            visibleEntities.ShouldContain(entity => entity.DeptId == allowedDeptId);
            visibleEntities.ShouldContain(entity => entity.CreatorId == currentUserId);
        }
        finally
        {
            db.Dispose();
        }
    }

    /// <summary>
    /// 构造用于验证接口过滤器的数据样本
    /// </summary>
    /// <param name="deptId">数据归属部门标识</param>
    /// <param name="creatorId">数据创建用户标识</param>
    /// <returns>数据权限测试实体</returns>
    private static DataPermissionTestEntity Entity(Guid deptId, Guid creatorId)
    {
        return new DataPermissionTestEntity
        {
            Id = Guid.NewGuid(),
            DeptId = deptId,
            CreatorId = creatorId
        };
    }

    /// <summary>
    /// 模拟显式接入 <see cref="IHasDataPermission"/> 的最小业务实体
    /// </summary>
    [SugarTable("DataPermissionTest")]
    private sealed class DataPermissionTestEntity : IHasDataPermission
    {
        /// <summary>
        /// 测试数据标识
        /// </summary>
        [SugarColumn(IsPrimaryKey = true)]
        public Guid Id { get; set; }

        /// <summary>
        /// 数据归属部门
        /// </summary>
        public Guid? DeptId { get; set; }

        /// <summary>
        /// 数据创建用户
        /// </summary>
        public Guid? CreatorId { get; set; }
    }
}
