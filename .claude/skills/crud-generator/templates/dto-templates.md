# DTO Templates

## Template Data Structure

从实体解析的数据结构：

```json
{
  "entityName": "ProductCategory",
  "moduleName": "product",
  "moduleNamespace": "Product",
  "tableName": "ProductCategory",
  "isTree": true,
  "hasOrderNum": true,
  "hasState": true,
  "hasAudited": true,
  "hasSoftDelete": true,
  "fields": [
    {"name": "CategoryName", "type": "string", "nullable": false, "comment": "分类名称", "isIndex": true},
    {"name": "CategoryCode", "type": "string", "nullable": true, "comment": "分类编码"},
    {"name": "ParentId", "type": "Guid", "nullable": false, "comment": "父级id", "isTreeField": true},
    {"name": "Remark", "type": "string", "nullable": true, "comment": "描述"}
  ],
  "standardFields": ["Id", "CreationTime", "CreatorId", "LastModifierId", "LastModificationTime", "OrderNum", "State", "IsDeleted"]
}
```

---

## GetOutputDto Template

⚠️ 架构边界：Application.Contracts 层不能引用 Domain.Entities，只能引用 Domain.Shared.Enums

```csharp
using Volo.Abp.Application.Dtos;
{{#if hasEnums}}
using Yi.Framework.{{moduleNamespace}}.Domain.Shared.Enums;
{{/if}}

namespace Yi.Framework.{{moduleNamespace}}.Application.Contracts.Dtos.{{entityName}}
{
    /// <summary>
    /// {{entityComment}}单个输出DTO
    /// </summary>
    public class {{entityName}}GetOutputDto : EntityDto<Guid>
    {
        {{#each fields}}
        /// <summary>
        /// {{comment}}
        /// </summary>
        public {{#if nullable}}{{type}}?{{else}}{{type}}{{/if}} {{name}} { get; set; }
        {{/each}}
        
        {{#if hasOrderNum}}
        /// <summary>
        /// 排序
        /// </summary>
        public int OrderNum { get; set; }
        {{/if}}
        
        {{#if hasState}}
        /// <summary>
        /// 状态
        /// </summary>
        public bool State { get; set; }
        {{/if}}
        
        {{#if hasAudited}}
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreationTime { get; set; }
        
        /// <summary>
        /// 创建者
        /// </summary>
        public Guid? CreatorId { get; set; }
        {{/if}}
    }
}
```

---

## GetListOutputDto Template

```csharp
using Volo.Abp.Application.Dtos;

namespace Yi.Framework.{{moduleNamespace}}.Application.Contracts.Dtos.{{entityName}}
{
    /// <summary>
    /// {{entityComment}}列表输出DTO
    /// </summary>
    public class {{entityName}}GetListOutputDto : EntityDto<Guid>
    {
        {{#each fields}}
        {{#unless isTreeField}}
        /// <summary>
        /// {{comment}}
        /// </summary>
        public {{#if nullable}}{{type}}?{{else}}{{type}}{{/if}} {{name}} { get; set; }
        {{/unless}}
        {{/each}}
        
        {{#if isTree}}
        /// <summary>
        /// 父级id
        /// </summary>
        public Guid ParentId { get; set; }
        {{/if}}
        
        {{#if hasOrderNum}}
        /// <summary>
        /// 排序
        /// </summary>
        public int OrderNum { get; set; }
        {{/if}}
        
        {{#if hasState}}
        /// <summary>
        /// 状态
        /// </summary>
        public bool State { get; set; }
        {{/if}}
        
        {{#if hasAudited}}
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreationTime { get; set; }
        
        /// <summary>
        /// 创建者
        /// </summary>
        public Guid? CreatorId { get; set; }
        {{/if}}
    }
}
```

---

## GetListInputVo Template (普通实体)

```csharp
using Yi.Framework.Ddd.Application.Contracts;

namespace Yi.Framework.{{moduleNamespace}}.Application.Contracts.Dtos.{{entityName}}
{
    /// <summary>
    /// {{entityComment}}列表查询输入
    /// </summary>
    public class {{entityName}}GetListInputVo : PagedAllResultRequestDto
    {
        {{#each fields}}
        {{#unless isTreeField}}
        {{#unless isIndex}}
        /// <summary>
        /// {{comment}}
        /// </summary>
        public {{type}}? {{name}} { get; set; }
        {{/unless}}
        {{/unless}}
        {{/each}}
        
        {{#if hasState}}
        /// <summary>
        /// 状态
        /// </summary>
        public bool? State { get; set; }
        {{/if}}
    }
}
```

---

## GetListInputVo Template (树形实体)

```csharp
namespace Yi.Framework.{{moduleNamespace}}.Application.Contracts.Dtos.{{entityName}}
{
    /// <summary>
    /// {{entityComment}}列表查询输入（树形实体不分页）
    /// </summary>
    public class {{entityName}}GetListInputVo
    {
        {{#each fields}}
        {{#unless isTreeField}}
        /// <summary>
        /// {{comment}}
        /// </summary>
        public {{#if nullable}}{{type}}?{{else}}{{type}}{{/if}} {{name}} { get; set; }
        {{/unless}}
        {{/each}}
        
        {{#if hasState}}
        /// <summary>
        /// 状态
        /// </summary>
        public bool? State { get; set; }
        {{/if}}
    }
}
```

---

## CreateInputVo Template

⚠️ isIndex 字段（如 ItemName）必须包含在 Input 中，它只是表示有唯一索引需要校验

```csharp
using System.ComponentModel.DataAnnotations;
{{#if hasEnums}}
using Yi.Framework.{{moduleNamespace}}.Domain.Shared.Enums;
{{/if}}

namespace Yi.Framework.{{moduleNamespace}}.Application.Contracts.Dtos.{{entityName}}
{
    /// <summary>
    /// {{entityComment}}创建输入
    /// </summary>
    public class {{entityName}}CreateInputVo
    {
        {{#each fields}}
        {{#unless isTreeField}}
        /// <summary>
        /// {{comment}}
        /// </summary>
        {{#unless nullable}}
        [Required]
        {{/unless}}
        public {{#if nullable}}{{type}}?{{else}}{{type}}{{/if}} {{name}} { get; set; }
        {{/unless}}
        {{/each}}
        
        {{#if hasOrderNum}}
        /// <summary>
        /// 排序
        /// </summary>
        public int OrderNum { get; set; }
        {{/if}}
        
        {{#if hasState}}
        /// <summary>
        /// 状态
        /// </summary>
        public bool State { get; set; } = true;
        {{/if}}
    }
}
```

---

## UpdateInputVo Template

```csharp
using System.ComponentModel.DataAnnotations;
{{#if hasEnums}}
using Yi.Framework.{{moduleNamespace}}.Domain.Shared.Enums;
{{/if}}

namespace Yi.Framework.{{moduleNamespace}}.Application.Contracts.Dtos.{{entityName}}
{
    /// <summary>
    /// {{entityComment}}更新输入
    /// </summary>
    public class {{entityName}}UpdateInputVo
    {
        {{#each fields}}
        {{#unless isTreeField}}
        /// <summary>
        /// {{comment}}
        /// </summary>
        {{#unless nullable}}
        [Required]
        {{/unless}}
        public {{#if nullable}}{{type}}?{{else}}{{type}}{{/if}} {{name}} { get; set; }
        {{/unless}}
        {{/each}}
        
        {{#if hasOrderNum}}
        /// <summary>
        /// 排序
        /// </summary>
        public int OrderNum { get; set; }
        {{/if}}
        
        {{#if hasState}}
        /// <summary>
        /// 状态
        /// </summary>
        public bool State { get; set; }
        {{/if}}
    }
}
```