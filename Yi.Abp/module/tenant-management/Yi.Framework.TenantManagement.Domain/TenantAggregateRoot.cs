using JetBrains.Annotations;
using SqlSugar;
using Volo.Abp;
using Volo.Abp.Auditing;
using Volo.Abp.Data;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.TenantManagement;
using Yi.Framework.Core.Data;
using Yi.Framework.SqlSugarCore.Abstractions;

namespace Yi.Framework.TenantManagement.Domain
{
    [SugarTable("YiTenant")]
    [DefaultTenantTable]
    public class TenantAggregateRoot : FullAuditedAggregateRoot<Guid>, IHasEntityVersion, IState
    {
        public TenantAggregateRoot()
        {

        }
        protected internal TenantAggregateRoot(Guid id, [NotNull] string name)
    : base(id)
        {
            SetName(name);
        }

        [SugarColumn(IsPrimaryKey = true)]
        public override Guid Id { get; protected set; }

        /// <summary>租户名称</summary>
        public virtual string Name { get; protected set; }

        /// <summary>实体版本号，分布式场景下追踪数据版本演进</summary>
        public int EntityVersion { get; protected set; }

        /// <summary>数据库连接字符串</summary>
        public string TenantConnectionString { get; protected set; }

        /// <summary>数据库类型</summary>
        public DbType DbType { get; protected set; }

        /// <summary>启用状态</summary>
        public bool State { get; set; } = true;

        /// <summary>联系人</summary>
        public string? ContactUserName { get; set; }

        /// <summary>联系电话</summary>
        public string? ContactPhone { get; set; }

        /// <summary>到期时间，null 表示无期限</summary>
        public DateTime? ExpireTime { get; set; }

        /// <summary>最大用户数量，-1 表示不限制</summary>
        public int AccountCount { get; set; } = -1;

        /// <summary>绑定域名</summary>
        public string? Domain { get; set; }

        /// <summary>企业地址</summary>
        public string? Address { get; set; }

        /// <summary>统一社会信用代码</summary>
        public string? LicenseNumber { get; set; }

        /// <summary>企业介绍</summary>
        public string? Intro { get; set; }

        /// <summary>备注</summary>
        public string? Remark { get; set; }

        [SugarColumn(IsIgnore = true)]
        public override ExtraPropertyDictionary ExtraProperties { get => base.ExtraProperties; protected set => base.ExtraProperties = value; }
        public virtual void SetConnectionString(DbType dbType, string connectionString)
        {
            DbType = dbType;
            TenantConnectionString = connectionString;
        }

        protected internal virtual void SetName([NotNull] string name)
        {
            Name = Volo.Abp.Check.NotNullOrWhiteSpace(name, nameof(name), TenantConsts.MaxNameLength);
        }

    }
}
