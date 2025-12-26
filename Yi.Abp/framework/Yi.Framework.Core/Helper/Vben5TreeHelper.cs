using System;
using System.Collections.Generic;
using System.Linq;
using Yi.Framework.Core.Models;

namespace Yi.Framework.Core.Helper
{
    /// <summary>
    /// Vben5 树形结构辅助类
    /// 用于构建标准 TreeDto 树形结构
    /// </summary>
    public static class Vben5TreeHelper
    {
        /// <summary>
        /// 构建树形结构
        /// </summary>
        /// <param name="list">树节点列表</param>
        /// <param name="rootParentId">根节点的 ParentId，默认为 null</param>
        /// <returns>树形结构列表</returns>
        public static List<TreeDto> BuildTree(List<TreeDto> list, object rootParentId = null)
        {
            if (list == null || list.Count == 0)
            {
                return new List<TreeDto>();
            }

            // 如果没有指定根节点ParentId，尝试查找最小的ParentId或使用常见的根节点标识
            if (rootParentId == null)
            {
                // 查找所有节点的ParentId
                var parentIds = list.Select(m => m.ParentId).Distinct().ToList();
                var allIds = list.Select(m => m.Id).ToList();
                
                // 找到那些ParentId不在所有Id中的节点，这些是根节点
                var rootNodes = list.Where(m => 
                    m.ParentId == null || 
                    !allIds.Any(id => IsEqual(id, m.ParentId)) ||
                    IsEmptyGuid(m.ParentId)
                ).ToList();

                if (rootNodes.Any())
                {
                    // 如果找到了根节点，使用第一个根节点的ParentId作为根标识
                    rootParentId = rootNodes.First().ParentId;
                }
                else
                {
                    // 否则尝试使用Guid.Empty
                    rootParentId = Guid.Empty;
                }
            }

            // 获取根节点
            var rootItems = list.Where(m => IsEqual(m.ParentId, rootParentId) || IsEmptyGuid(m.ParentId))
                .OrderBy(m => m.Weight)
                .ToList();

            // 递归构建子节点
            foreach (var item in rootItems)
            {
                BuildChildren(list, item);
            }

            return rootItems;
        }

        /// <summary>
        /// 递归构建子节点
        /// </summary>
        /// <param name="allItems">所有节点列表</param>
        /// <param name="parentItem">父节点</param>
        private static void BuildChildren(List<TreeDto> allItems, TreeDto parentItem)
        {
            // 查找当前节点的所有子节点
            var children = allItems.Where(m => IsEqual(m.ParentId, parentItem.Id))
                .OrderBy(m => m.Weight)
                .ToList();

            if (children.Any())
            {
                parentItem.Children = children;

                // 递归构建每个子节点的子节点
                foreach (var child in children)
                {
                    BuildChildren(allItems, child);
                }
            }
            else
            {
                parentItem.Children = new List<TreeDto>();
            }
        }

        /// <summary>
        /// 判断两个对象是否相等
        /// </summary>
        private static bool IsEqual(object obj1, object obj2)
        {
            if (obj1 == null && obj2 == null) return true;
            if (obj1 == null || obj2 == null) return false;

            // 尝试转换为 Guid 进行比较
            if (obj1 is Guid guid1 && obj2 is Guid guid2)
            {
                return guid1 == guid2;
            }

            // 尝试转换为字符串进行比较
            if (obj1 is string str1 && obj2 is string str2)
            {
                return str1 == str2;
            }

            // 通用比较
            return obj1.Equals(obj2);
        }

        /// <summary>
        /// 判断是否为空的 Guid
        /// </summary>
        private static bool IsEmptyGuid(object obj)
        {
            if (obj == null) return false;
            
            if (obj is Guid guid)
            {
                return guid == Guid.Empty;
            }

            if (obj is string str && Guid.TryParse(str, out var parsedGuid))
            {
                return parsedGuid == Guid.Empty;
            }

            return false;
        }
    }
}

