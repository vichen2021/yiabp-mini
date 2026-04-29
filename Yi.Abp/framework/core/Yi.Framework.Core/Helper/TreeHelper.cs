using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yi.Framework.Core.Helper
{
    public static class TreeHelper
    {
        public static List<T> SetTree<T>(List<T> list, Action<T> action = null!)
        {
            if (list is not null && list.Count > 0)
            {
                IList<T> result = new List<T>();
                
                // 安全地获取最小ParentId，如果列表为空则返回Guid.Empty
                var parentIds = list.Select(m => (m as ITreeModel<T>)?.ParentId).Where(id => id.HasValue).Select(id => id.Value);
                if (!parentIds.Any())
                {
                    return new List<T>();
                }
                
                Guid pid = parentIds.Min();
                IList<T> t = list.Where(m => (m as ITreeModel<T>)?.ParentId == pid).ToList();
                
                foreach (T model in t)
                {
                    if (action is not null)
                    {
                        action(model);
                    }
                    result.Add(model);
                    var item = model as ITreeModel<T>;
                    if (item != null)
                    {
                        IList<T> children = list.Where(m => (m as ITreeModel<T>)?.ParentId == item.Id).ToList();
                        if (children.Count > 0)
                        {
                            SetTreeChildren(list, children, model, action!);
                        }
                    }
                }
                return result.OrderByDescending(m => (m as ITreeModel<T>)?.OrderNum ?? 0).ToList();
            }
            return new List<T>();
        }
        private static void SetTreeChildren<T>(IList<T> list, IList<T> children, T model, Action<T> action = null!)
        {
            var mm = model as ITreeModel<T>;
            if (mm == null) return;
            
            mm.Children = new List<T>();
            foreach (T item in children)
            {
                if (action is not null)
                {
                    action(item);
                }
                mm.Children.Add(item);
                var _item = item as ITreeModel<T>;
                if (_item != null)
                {
                    IList<T> _children = list.Where(m => (m as ITreeModel<T>)?.ParentId == _item.Id).ToList();
                    if (_children.Count > 0)
                    {
                        SetTreeChildren(list, _children, item, action!);
                    }
                }
            }
            mm.Children = mm.Children.OrderByDescending(m => (m as ITreeModel<T>)?.OrderNum ?? 0).ToList();
        }


        public interface ITreeModel<T>
        {
            public Guid Id { get; set; }
            public Guid ParentId { get; set; }
            public int OrderNum { get; set; }

            public List<T>? Children { get; set; }
        }
    }

}
