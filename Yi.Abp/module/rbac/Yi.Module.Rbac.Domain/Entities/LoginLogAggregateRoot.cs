using SqlSugar;
using Volo.Abp.Auditing;
using Volo.Abp.Domain.Entities;

namespace Yi.Module.Rbac.Domain.Entities
{
    [SugarTable("LoginLog")]
    [SugarIndex($"index_{nameof(LoginUser)}", nameof(LoginUser), OrderByType.Asc)]
    public class LoginLogAggregateRoot : AggregateRoot<Guid>, ICreationAuditedObject
    {
        [SugarColumn(ColumnName = "Id", IsPrimaryKey = true)]
        public override Guid Id { get; protected set; }
        public DateTime CreationTime { get; set; }

        /// <summary>
        /// 登录用户 
        ///</summary>
        [SugarColumn(ColumnName = "LoginUser")]
        public string? LoginUser { get; set; }
        /// <summary>
        /// 登录地点 
        ///</summary>
        [SugarColumn(ColumnName = "LoginLocation")]
        public string? LoginLocation { get; set; }
        /// <summary>
        /// 登录Ip 
        ///</summary>
        [SugarColumn(ColumnName = "LoginIp")]
        public string? LoginIp { get; set; }
        /// <summary>
        /// 浏览器 
        ///</summary>
        [SugarColumn(ColumnName = "Browser")]
        public string? Browser { get; set; }
        /// <summary>
        /// 操作系统 
        ///</summary>
        [SugarColumn(ColumnName = "Os")]
        public string? Os { get; set; }
        /// <summary>
        /// 登录信息 
        ///</summary>
        [SugarColumn(ColumnName = "LogMsg")]
        public string? LogMsg { get; set; }

        public Guid? CreatorId { get; set; }
    }

}
