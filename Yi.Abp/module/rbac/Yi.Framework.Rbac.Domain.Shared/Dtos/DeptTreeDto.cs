using System.Collections.Generic;
using static Yi.Framework.Core.Helper.TreeHelper;

namespace Yi.Framework.Rbac.Domain.Shared.Dtos
{
    /// <summary>
    /// 部门树形DTO
    /// </summary>
    public class DeptTreeDto : ITreeModel<DeptTreeDto>
    {
        /// <summary>
        /// 部门ID
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// 父部门ID
        /// </summary>
        public Guid ParentId { get; set; }

        /// <summary>
        /// 排序号
        /// </summary>
        public int OrderNum { get; set; }

        /// <summary>
        /// 部门名称
        /// </summary>
        public string DeptName { get; set; } = string.Empty;

        /// <summary>
        /// 部门编码
        /// </summary>
        public string DeptCode { get; set; } = string.Empty;

        /// <summary>
        /// 负责人（用户ID）
        /// </summary>
        public Guid? Leader { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        public string? Remark { get; set; }

        /// <summary>
        /// 状态
        /// </summary>
        public bool State { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreationTime { get; set; }

        /// <summary>
        /// 子部门列表
        /// </summary>
        public List<DeptTreeDto>? Children { get; set; }

        /// <summary>
        /// 部门标签（用于前端显示）
        /// </summary>
        public string Label { get; set; } = string.Empty;

        /// <summary>
        /// 部门值（用于前端选择）
        /// </summary>
        public string Value { get; set; } = string.Empty;

        /// <summary>
        /// 是否禁用
        /// </summary>
        public bool Disabled { get; set; }
    }
}

