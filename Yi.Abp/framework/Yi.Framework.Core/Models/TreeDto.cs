using System.Collections.Generic;

namespace Yi.Framework.Core.Models
{
    /// <summary>
    /// 通用树形结构数据传输对象
    /// </summary>
    public class TreeDto
    {
        /// <summary>
        /// 节点唯一标识
        /// </summary>
        public object Id { get; set; }

        /// <summary>
        /// 父节点标识
        /// </summary>
        public object ParentId { get; set; }

        /// <summary>
        /// 节点显示文本
        /// </summary>
        public string Label { get; set; }

        /// <summary>
        /// 排序权重（值越小越靠前）
        /// </summary>
        public int Weight { get; set; }

        /// <summary>
        /// 是否禁用该节点
        /// </summary>
        public bool Disabled { get; set; }

        /// <summary>
        /// 子节点集合
        /// </summary>
        public List<TreeDto> Children { get; set; } = new List<TreeDto>();
    }
}

