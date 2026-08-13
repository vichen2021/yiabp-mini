using SqlSugar;
using Volo.Abp;
using Volo.Abp.Auditing;
using Yi.Framework.Core.Data;
using Yi.Framework.Ddd.Domain;
using Yi.Module.Rbac.Domain.Shared.Authorization;

namespace Yi.Module.Rbac.Domain.Entities
{
    /// <summary>
    /// 岗位表
    ///</summary>
    [SugarTable("Post")]
    public  class PostAggregateRoot : BaseAggregateRoot<Guid>, ISoftDelete, IAuditedObject, IOrderNum, IState, IHasDataPermission
    {

        /// <summary>
        /// 主键
        /// </summary>
        [SugarColumn(IsPrimaryKey = true)]
        public override Guid Id { get; protected set; }

        /// <summary>
        /// 逻辑删除
        /// </summary>
        public bool IsDeleted { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreationTime { get; set; } = DateTime.Now;

        /// <summary>
        /// 创建者
        /// </summary>
        public Guid? CreatorId { get; set; }

        /// <summary>
        /// 最后修改者
        /// </summary>
        public Guid? LastModifierId { get; set; }

        /// <summary>
        /// 最后修改时间
        /// </summary>
        public DateTime? LastModificationTime { get; set; }

        /// <summary>
        /// 排序
        /// </summary>
        public int OrderNum { get; set; } = 0;

        /// <summary>
        /// 状态
        /// </summary>
        public bool State { get; set; } = true;

        /// <summary>
        /// 岗位编码 
        ///</summary>
        [SugarColumn(ColumnName = "PostCode")]
        public string PostCode { get; set; } = string.Empty;
        /// <summary>
        /// 岗位名称 
        ///</summary>
        [SugarColumn(ColumnName = "PostName")]
        public string PostName { get; set; } = string.Empty;

        /// <summary>
        /// 所属部门ID
        ///</summary>
        [SugarColumn(ColumnName = "DeptId")]
        public Guid DeptId { get; set; }

        // 数据库存储仍使用岗位既有的非空 DeptId；显式接口仅适配通用可空契约，不产生新的 ORM 列。
        // SqlSugar 将接口过滤表达式映射到该属性的行为已由真实 SQLite 测试覆盖，升级 ORM 时需保留回归验证。
        Guid? IHasDataPermission.DeptId => DeptId;

        /// <summary>
        /// 描述 
        ///</summary>
        [SugarColumn(ColumnName = "Remark")]
        public string? Remark { get; set; }
    }
}
