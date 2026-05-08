using SqlSugar;
using Volo.Abp.Auditing;
using Volo.Abp.Domain.Entities;
using Yi.Framework.OperationRecord.Abstractions.Enums;

namespace Yi.Module.AuditLogging.Domain.Entities
{
    /// <summary>
    /// 操作记录表
    ///</summary>
    [SugarTable("OperationLog")]
    [SugarIndex($"index_{nameof(CreationTime)}", nameof(CreationTime), OrderByType.Desc)]
    public class OperationLogEntity : Entity<Guid>, ICreationAuditedObject
    {
        [SugarColumn(IsPrimaryKey = true)]
        public override Guid Id { get; protected set; }

        /// <summary>
        /// 操作标题
        ///</summary>
        [SugarColumn(ColumnName = "Title", Length = 200)]
        public string? Title { get; set; }

        /// <summary>
        /// 操作类型
        ///</summary>
        [SugarColumn(ColumnName = "OperType")]
        public OperEnum OperType { get; set; }

        /// <summary>
        /// 请求方法（HTTP Method）
        ///</summary>
        [SugarColumn(ColumnName = "RequestMethod", Length = 16)]
        public string? RequestMethod { get; set; }

        /// <summary>
        /// 操作人员
        ///</summary>
        [SugarColumn(ColumnName = "OperUser", Length = 64)]
        public string? OperUser { get; set; }

        /// <summary>
        /// 操作 IP
        ///</summary>
        [SugarColumn(ColumnName = "OperIp", Length = 64)]
        public string? OperIp { get; set; }

        /// <summary>
        /// 操作地点
        ///</summary>
        [SugarColumn(ColumnName = "OperLocation", Length = 128)]
        public string? OperLocation { get; set; }

        /// <summary>
        /// 操作方法（请求路径）
        ///</summary>
        [SugarColumn(ColumnName = "Method", Length = 512)]
        public string? Method { get; set; }

        /// <summary>
        /// 请求参数
        ///</summary>
        [SugarColumn(ColumnName = "RequestParam", ColumnDataType = StaticConfig.CodeFirst_BigString)]
        public string? RequestParam { get; set; }

        /// <summary>
        /// 响应数据
        ///</summary>
        [SugarColumn(ColumnName = "RequestResult", ColumnDataType = StaticConfig.CodeFirst_BigString)]
        public string? RequestResult { get; set; }

        /// <summary>
        /// 是否成功
        ///</summary>
        [SugarColumn(ColumnName = "IsSuccess")]
        public bool IsSuccess { get; set; }

        /// <summary>
        /// 错误信息
        ///</summary>
        [SugarColumn(ColumnName = "ErrorMessage", ColumnDataType = StaticConfig.CodeFirst_BigString)]
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// 执行耗时（毫秒）
        ///</summary>
        [SugarColumn(ColumnName = "ExecutionDuration")]
        public int ExecutionDuration { get; set; }

        public DateTime CreationTime { get; set; }
        public Guid? CreatorId { get; set; }

        public OperationLogEntity() { }

        public OperationLogEntity(
            Guid id,
            string? title,
            OperEnum operType,
            string? requestMethod,
            string? operUser,
            string? operIp,
            string? method,
            bool isSuccess,
            int executionDuration)
        {
            Id = id;
            Title = title;
            OperType = operType;
            RequestMethod = requestMethod;
            OperUser = operUser;
            OperIp = operIp;
            Method = method;
            IsSuccess = isSuccess;
            ExecutionDuration = executionDuration;
            CreationTime = DateTime.Now;
        }
    }
}
