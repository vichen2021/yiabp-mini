using System;
using System.Text.RegularExpressions;
using System.Web;
using NUglify.Helpers;
using SqlSugar;
using Volo.Abp;
using Volo.Abp.Auditing;
using Volo.Abp.Domain.Entities;
using Yi.Framework.Core.Data;
using Yi.Framework.Core.Helper;
using Yi.Framework.Rbac.Domain.Shared.Dtos;
using Yi.Framework.Rbac.Domain.Shared.Enums;

namespace Yi.Framework.Rbac.Domain.Entities
{
    /// <summary>
    /// 菜单表
    ///</summary>
    [SugarTable("Menu")]
    public partial class MenuAggregateRoot : AggregateRoot<Guid>, ISoftDelete, IAuditedObject, IOrderNum, IState
    {
        public MenuAggregateRoot()
        {
        }

        public MenuAggregateRoot(Guid id)
        {
            Id = id;
            ParentId = Guid.Empty;
        }

        public MenuAggregateRoot(Guid id, Guid parentId)
        {
            Id = id;
            ParentId = parentId;
        }

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
        public bool State { get; set; }

        /// <summary>
        /// 菜单名
        /// </summary>
        public string MenuName { get; set; } 

        /// <summary>
        /// 路由名称
        /// </summary>
        public string? RouterName { get; set; }

        /// <summary>
        ///  
        ///</summary>
        [SugarColumn(ColumnName = "MenuType")]
        public MenuTypeEnum MenuType { get; set; } = MenuTypeEnum.Menu;

        /// <summary>
        ///  
        ///</summary>
        [SugarColumn(ColumnName = "PermissionCode")]
        public string? PermissionCode { get; set; }

        /// <summary>
        ///  
        ///</summary>
        [SugarColumn(ColumnName = "ParentId")]
        public Guid ParentId { get; set; }

        /// <summary>
        /// 菜单图标 
        ///</summary>
        [SugarColumn(ColumnName = "MenuIcon")]
        public string? MenuIcon { get; set; }

        /// <summary>
        /// 菜单组件路由 
        ///</summary>
        [SugarColumn(ColumnName = "Router")]
        public string? Router { get; set; }

        /// <summary>
        /// 是否为外部链接 
        ///</summary>
        [SugarColumn(ColumnName = "IsLink")]
        public bool IsLink { get; set; }

        /// <summary>
        /// 是否缓存 
        ///</summary>
        [SugarColumn(ColumnName = "IsCache")]
        public bool IsCache { get; set; }

        /// <summary>
        /// 是否显示 
        ///</summary>
        [SugarColumn(ColumnName = "IsShow")]
        public bool IsShow { get; set; } = true;

        /// <summary>
        /// 描述 
        ///</summary>
        [SugarColumn(ColumnName = "Remark")]
        public string? Remark { get; set; }

        /// <summary>
        /// 组件路径 
        ///</summary>
        [SugarColumn(ColumnName = "Component")]
        public string? Component { get; set; }

        /// <summary>
        /// 菜单来源
        /// </summary>
        public MenuSourceEnum MenuSource { get; set; } = MenuSourceEnum.Ruoyi;

        /// <summary>
        /// 路由参数 
        ///</summary>
        [SugarColumn(ColumnName = "Query")]
        public string? Query { get; set; }

        [SugarColumn(IsIgnore = true)] public List<MenuAggregateRoot>? Children { get; set; }
    }

    /// <summary>
    /// 实体扩展
    /// </summary>
    public static class MenuEntityExtensions
    {
        /// <summary>
        /// 构建vue3路由
        /// </summary>
        /// <param name="menus"></param>
        /// <returns></returns>
        public static List<RouterDto> RouterBuild(this List<MenuAggregateRoot> menus)
        {
            menus = menus
                .Where(m => m.State == true)
                .Where(m => m.MenuType != MenuTypeEnum.Component)
                .Where(m => m.MenuSource == MenuSourceEnum.Ruoyi)
                .ToList();
            List<RouterDto> routers = new();
            foreach (var m in menus)
            {
                var r = new RouterDto();
                r.OrderNum = m.OrderNum;
                r.Id = m.Id;
                r.ParentId = m.ParentId;
                r.Hidden = !m.IsShow;

                // 检测是否为 URL 链接（http:// 或 https:// 开头）
                bool isUrl = !string.IsNullOrEmpty(m.Router) && 
                    (m.Router.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || 
                     m.Router.StartsWith("https://", StringComparison.OrdinalIgnoreCase));

                // 判断是否为内嵌 iframe：
                // 1. Component 明确设置为 "InnerLink"（优先级最高）
                // 2. 或者检测到是 URL 且 isLink = false（自动识别为内嵌）
                bool isInnerLink = (!string.IsNullOrEmpty(m.Component) && 
                    m.Component.Equals("InnerLink", StringComparison.OrdinalIgnoreCase)) ||
                    (isUrl && !m.IsLink);

                // 判断是否为外链（新标签页打开）：
                // 检测到是 URL 且 isLink = true，且不是内嵌 iframe
                bool isExternalLink = isUrl && m.IsLink && !isInnerLink;

                // 生成路由名称
                string routerName;
                if (isInnerLink)
                {
                    // 内嵌 iframe：从 path 或 router 中提取名称
                    routerName = m.Router?.Split("/").LastOrDefault() ?? "InnerLink";
                }
                else if (isExternalLink)
                {
                    // 外链：从 URL 中提取名称
                    try
                    {
                        var uri = new Uri(m.Router!);
                        routerName = uri.Host.Replace(".", "").Replace("-", "");
                    }
                    catch
                    {
                        // 如果 URL 格式不正确，使用默认名称
                        routerName = "ExternalLink";
                    }
                }
                else
                {
                    // 普通路由：从 router 中提取名称
                    routerName = m.Router?.Split("/").LastOrDefault() ?? string.Empty;
                }

                // 开头大写处理
                if (string.IsNullOrEmpty(routerName))
                {
                    r.Name = routerName;
                }
                else if (routerName.Length == 1)
                {
                    r.Name = routerName.ToUpper();
                }
                else
                {
                    r.Name = routerName.First().ToString().ToUpper() + routerName.Substring(1);
                }

                // 设置路径
                r.Path = m.Router ?? string.Empty;

                // 处理内嵌 iframe 场景（优先级最高）
                // 触发条件：Component = "InnerLink" 或 (检测到 URL 且 isLink = false)
                if (isInnerLink)
                {
                    // 内嵌 iframe：component 为 InnerLink，meta.link 包含完整 iframe 地址
                    r.Redirect = "noRedirect";
                    r.AlwaysShow = false;
                    r.Component = "InnerLink";
                    
                    // meta.link 应该包含完整的 iframe 地址，优先使用 Router
                    string iframeUrl = !string.IsNullOrEmpty(m.Router) ? m.Router : m.Component ?? string.Empty;
                    
                    // 清理 path：去除协议和特殊字符，避免前端路由拼接时出现问题
                    string cleanedPath = m.Router ?? m.Component ?? string.Empty;
                    if (!string.IsNullOrEmpty(cleanedPath))
                    {
                        // 去除 http:// 或 https://
                        cleanedPath = Regex.Replace(cleanedPath, @"^https?://", "", RegexOptions.IgnoreCase);
                        // 去除 /#/
                        cleanedPath = cleanedPath.Replace("/#/", "");
                        // 去除 #
                        cleanedPath = cleanedPath.Replace("#", "");
                        // 去除 ? 和 &
                        cleanedPath = cleanedPath.Replace("?", "").Replace("&", "");
                    }
                    
                    // 使用清理后的 path，用于前端路由匹配
                    r.Path = cleanedPath;
                    
                    r.Meta = new Meta
                    {
                        Title = m.MenuName!,
                        Icon = m.MenuIcon ?? string.Empty,
                        NoCache = !m.IsCache,
                        link = iframeUrl // meta.link 保持完整的 URL，用于 iframe 加载
                    };
                }
                // 处理外链场景（新标签页打开）
                // 触发条件：检测到 URL 且 isLink = true
                else if (isExternalLink)
                {
                    // 外链：path 保持原样，component 为 Layout 或 ParentView，meta.link 包含完整外链地址
                    r.Redirect = "noRedirect";
                    r.AlwaysShow = false;
                    
                    // 判断是否为最顶层的路由
                    if (Guid.Empty == m.ParentId)
                    {
                        r.Component = "Layout";
                    }
                    else
                    {
                        r.Component = "ParentView";
                    }
                    
                    r.Meta = new Meta
                    {
                        Title = m.MenuName!,
                        Icon = m.MenuIcon ?? string.Empty,
                        NoCache = !m.IsCache,
                        link = m.Router! // 完整的外链地址
                    };
                }
                // 处理普通路由菜单
                else
                {
                    if (m.MenuType == MenuTypeEnum.Catalogue)
                    {
                        r.Redirect = "noRedirect";
                        r.AlwaysShow = true;

                        // 判断是否为最顶层的路由
                        if (Guid.Empty == m.ParentId)
                        {
                            r.Component = "Layout";
                        }
                        else
                        {
                            r.Component = "ParentView";
                        }
                    }
                    else if (m.MenuType == MenuTypeEnum.Menu)
                    {
                        r.Redirect = "noRedirect";
                        r.AlwaysShow = false;
                        r.Component = m.Component ?? string.Empty;
                    }

                    r.Meta = new Meta
                    {
                        Title = m.MenuName!,
                        Icon = m.MenuIcon ?? string.Empty,
                        NoCache = !m.IsCache
                    };
                    
                    // 如果 IsLink 为 true 但不是外链，则可能是其他类型的链接
                    if (m.IsLink && !string.IsNullOrEmpty(m.Router))
                    {
                        r.Meta.link = m.Router;
                    }
                }

                routers.Add(r);
            }

            return TreeHelper.SetTree(routers);
        }

        /// <summary>
        /// 构建菜单树表
        /// </summary>
        /// <param name="menus"></param>
        /// <returns></returns>
        public static List<MenuTreeDto> TreeDtoBuild(this List<MenuAggregateRoot> menus)
        {
            List<MenuTreeDto> treeDtos = new();
            foreach (var m in menus)
            {
                var treeDto = new MenuTreeDto
                {
                    Id = m.Id,
                    ParentId = m.ParentId,
                    OrderNum = m.OrderNum,
                    MenuName = m.MenuName,
                    MenuType = m.MenuType,
                    MenuIcon = m.MenuIcon
                };
                treeDtos.Add(treeDto);
            }

            return TreeHelper.SetTree(treeDtos);
        }
    }
}