# TreeDto Template

```csharp
using System.Collections.Generic;
using static Yi.Framework.Core.Helper.TreeHelper;

namespace Yi.Framework.{{moduleNamespace}}.Domain.Shared.Dtos
{
    /// <summary>
    /// {{entityComment}}树形DTO
    /// </summary>
    public class {{entityName}}TreeDto : ITreeModel<{{entityName}}TreeDto>
    {
        /// <summary>
        /// ID
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// 父级ID
        /// </summary>
        public Guid ParentId { get; set; }

        /// <summary>
        /// 排序号
        /// </summary>
        public int OrderNum { get; set; }

        {{#each fields}}
        {{#unless isTreeField}}
        /// <summary>
        /// {{comment}}
        /// </summary>
        public {{#if nullable}}{{type}}?{{else}}{{type}}{{/if}} {{name}} { get; set; }
        {{/unless}}
        {{/each}}

        /// <summary>
        /// 状态
        /// </summary>
        public bool State { get; set; }

        /// <summary>
        /// 子节点列表
        /// </summary>
        public List<{{entityName}}TreeDto>? Children { get; set; }
    }
}
```

---

# Entity Extension Template (树形实体)

在实体文件末尾添加扩展方法：

```csharp
/// <summary>
/// {{entityComment}}实体扩展
/// </summary>
public static class {{entityName}}EntityExtensions
{
    /// <summary>
    /// 构建{{entityComment}}树形列表
    /// </summary>
    /// <param name="entities">实体列表</param>
    /// <returns>树形结构的列表</returns>
    public static List<{{entityName}}TreeDto> {{entityName}}TreeBuild(this List<{{entityName}}AggregateRoot> entities)
    {
        var filteredEntities = entities
            .Where(e => e.State == true)
            .ToList();

        List<{{entityName}}TreeDto> trees = new();
        foreach (var entity in filteredEntities)
        {
            var tree = new {{entityName}}TreeDto
            {
                Id = entity.Id,
                OrderNum = entity.OrderNum,
                {{#each fields}}
                {{#unless isTreeField}}
                {{name}} = entity.{{name}},
                {{/unless}}
                {{/each}}
                State = entity.State,
                ParentId = entity.ParentId,
            };
            trees.Add(tree);
        }

        return TreeHelper.SetTree(trees);
    }
}
```

---

# Repository Template (树形实体)

## IRepository Interface

```csharp
using Yi.Framework.{{moduleNamespace}}.Domain.Entities;
using Yi.Framework.SqlSugarCore.Abstractions;

namespace Yi.Framework.{{moduleNamespace}}.Domain.Repositories
{
    /// <summary>
    /// {{entityComment}}仓储接口
    /// </summary>
    public interface I{{entityName}}Repository : ISqlSugarRepository<{{entityName}}AggregateRoot, Guid>
    {
        /// <summary>
        /// 获取子节点ID列表
        /// </summary>
        Task<List<Guid>> GetChildListAsync(Guid parentId);
    }
}
```

## Repository Implementation

```csharp
using Yi.Framework.{{moduleNamespace}}.Domain.Entities;
using Yi.Framework.{{moduleNamespace}}.Domain.Repositories;
using Yi.Framework.SqlSugarCore;
using Yi.Framework.SqlSugarCore.Abstractions;

namespace Yi.Framework.{{moduleNamespace}}.SqlSugarCore.Repositories
{
    /// <summary>
    /// {{entityComment}}仓储实现
    /// </summary>
    public class {{entityName}}Repository : SqlSugarRepository<{{entityName}}AggregateRoot, Guid>, I{{entityName}}Repository
    {
        public {{entityName}}Repository(ISqlSugarDbContext sqlSugarDbContext) : base(sqlSugarDbContext)
        {
        }

        public async Task<List<Guid>> GetChildListAsync(Guid parentId)
        {
            var entities = await _DbQueryable.ToChildListAsync(x => x.ParentId, parentId);
            return entities.Select(x => x.Id).ToList();
        }
    }
}
```