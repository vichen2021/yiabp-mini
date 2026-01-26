# Backend Code Patterns

This document provides detailed code patterns for backend implementation.

## Entity Patterns

### Basic Entity Structure

```csharp
using SqlSugar;
using Volo.Abp;
using Volo.Abp.Auditing;
using Volo.Abp.Domain.Entities;
using Yi.Framework.Core.Data;

namespace Yi.Framework.{ModuleName}.Domain.Entities
{
    [SugarTable("{TableName}")]
    public class {EntityName}AggregateRoot : AggregateRoot<Guid>, ISoftDelete, IAuditedObject, IOrderNum, IState
    {
        [SugarColumn(IsPrimaryKey = true)]
        public override Guid Id { get; protected set; }
        
        public bool IsDeleted { get; set; }
        public DateTime CreationTime { get; set; } = DateTime.Now;
        public Guid? CreatorId { get; set; }
        public Guid? LastModifierId { get; set; }
        public DateTime? LastModificationTime { get; set; }
        public int OrderNum { get; set; } = 0;
        public bool State { get; set; } = true;
        
        // Entity-specific properties
    }
}
```

### Tree Structure Entity

For entities with parent-child relationships:

```csharp
/// <summary>
/// 父级id 
///</summary>
[SugarColumn(ColumnName = "ParentId")]
public Guid ParentId { get; set; }

/// <summary>
/// 子列表（用于树形结构）
/// </summary>
[SugarColumn(IsIgnore = true)]
public List<{EntityName}AggregateRoot>? Children { get; set; }
```

## DTO Patterns

### GetOutputDto (Single Entity)

```csharp
using Volo.Abp.Application.Dtos;

namespace Yi.Framework.{ModuleName}.Application.Contracts.Dtos.{EntityName}
{
    public class {EntityName}GetOutputDto : EntityDto<Guid>
    {
        public bool State { get; set; }
        // Core properties for single entity view
    }
}
```

### GetListOutputDto (List with Joins)

```csharp
using Volo.Abp.Application.Dtos;

namespace Yi.Framework.{ModuleName}.Application.Contracts.Dtos.{EntityName}
{
    public class {EntityName}GetListOutputDto : EntityDto<Guid>
    {
        public DateTime CreationTime { get; set; } = DateTime.Now;
        public Guid? CreatorId { get; set; }
        public bool State { get; set; }
        
        /// <summary>
        /// Related entity name (from join)
        /// </summary>
        public string? RelatedEntityName { get; set; }
        
        // Other list properties
    }
}
```

### GetListInputVo (Query Parameters)

```csharp
using Yi.Framework.Ddd;
using Yi.Framework.Ddd.Application.Contracts;

namespace Yi.Framework.{ModuleName}.Application.Contracts.Dtos.{EntityName}
{
    public class {EntityName}GetListInputVo : PagedAllResultRequestDto
    {
        public Guid Id { get; set; }
        public bool? State { get; set; }
        public string? Name { get; set; }
        // Filter properties
    }
}
```

## Service Patterns

### Basic Service Implementation

```csharp
using Yi.Framework.Ddd.Application;
using Yi.Framework.{ModuleName}.Application.Contracts.Dtos.{EntityName};
using Yi.Framework.{ModuleName}.Application.Contracts.IServices;
using Yi.Framework.{ModuleName}.Domain.Entities;
using Yi.Framework.{ModuleName}.Domain.Repositories;

namespace Yi.Framework.{ModuleName}.Application.Services
{
    public class {EntityName}Service : YiCrudAppService<{EntityName}AggregateRoot, {EntityName}GetOutputDto, {EntityName}GetListOutputDto, Guid,
        {EntityName}GetListInputVo, {EntityName}CreateInputVo, {EntityName}UpdateInputVo>, I{EntityName}Service
    {
        private I{EntityName}Repository _repository;

        public {EntityName}Service(I{EntityName}Repository repository) : base(repository)
        {
            _repository = repository;
        }
    }
}
```

### Custom List Query with Joins

```csharp
[Route("{entity-name}/list")]
public async Task<List<{EntityName}GetListOutputDto>> GetListAsync({EntityName}GetListInputVo input)
{
    var result = await _repository._DbQueryable
        .WhereIF(!string.IsNullOrEmpty(input.Name), u => u.Name.Contains(input.Name!))
        .WhereIF(input.State is not null, u => u.State == input.State)
        .LeftJoin<RelatedEntity>((main, related) => main.RelatedId == related.Id)
        .OrderBy((main, related) => main.OrderNum, OrderByType.Asc)
        .Select((main, related) => new {EntityName}GetListOutputDto
        {
            Id = main.Id,
            CreationTime = main.CreationTime,
            CreatorId = main.CreatorId,
            State = main.State,
            RelatedEntityName = related.Name,
            // Map other properties
        })
        .ToListAsync();
    
    return result;
}
```

### Input Validation

```csharp
protected override async Task CheckCreateInputDtoAsync({EntityName}CreateInputVo input)
{
    var isExist = await _repository.IsAnyAsync(x => x.Code == input.Code);
    if (isExist)
    {
        throw new UserFriendlyException("Code already exists");
    }
}

protected override async Task CheckUpdateInputDtoAsync({EntityName}AggregateRoot entity, {EntityName}UpdateInputVo input)
{
    if (input.Code != entity.Code)
    {
        var isExist = await _repository.IsAnyAsync(x => x.Code == input.Code && x.Id != entity.Id);
        if (isExist)
        {
            throw new UserFriendlyException("Code already exists");
        }
    }
}
```

### Tree Structure Methods

```csharp
/// <summary>
/// 获取树形结构的列表
/// </summary>
public async Task<List<{EntityName}TreeDto>> GetTreeAsync()
{
    var entities = await _repository._DbQueryable
        .Where(x => x.State == true)
        .OrderBy(x => x.OrderNum, OrderByType.Asc)
        .ToListAsync();
    return entities.{EntityName}TreeBuild();
}

/// <summary>
/// 获取排除指定节点及其子节点的列表
/// </summary>
public async Task<List<{EntityName}GetListOutputDto>> GetListExcludeAsync(Guid id)
{
    // Get all children IDs
    var excludeIds = await GetAllChildrenIdsAsync(id);
    excludeIds.Add(id);
    
    var result = await _repository._DbQueryable
        .Where(x => !excludeIds.Contains(x.Id))
        .OrderBy(x => x.OrderNum, OrderByType.Asc)
        .ToListAsync();
    
    return ObjectMapper.Map<List<{EntityName}AggregateRoot>, List<{EntityName}GetListOutputDto>>(result);
}
```

## Repository Patterns

### Basic Repository

```csharp
using Volo.Abp.DependencyInjection;
using Yi.Framework.{ModuleName}.Domain.Entities;
using Yi.Framework.{ModuleName}.Domain.Repositories;
using Yi.Framework.SqlSugarCore.Abstractions;
using Yi.Framework.SqlSugarCore.Repositories;

namespace Yi.Framework.{ModuleName}.SqlSugarCore.Repositories
{
    public class {EntityName}Repository : SqlSugarRepository<{EntityName}AggregateRoot, Guid>, I{EntityName}Repository, ITransientDependency
    {
        public {EntityName}Repository(ISugarDbContextProvider<ISqlSugarDbContext> sugarDbContextProvider) : base(sugarDbContextProvider)
        {
        }
    }
}
```

### Custom Repository Methods

```csharp
public async Task<List<Guid>> GetChildListAsync(Guid parentId)
{
    var entities = await _DbQueryable.ToChildListAsync(x => x.ParentId, parentId);
    return entities.Select(x => x.Id).ToList();
}
```

## Common Interfaces

- `ISoftDelete` - For logical deletion
- `IAuditedObject` - For audit fields (CreationTime, CreatorId, etc.)
- `IOrderNum` - For sorting
- `IState` - For enable/disable state

## SqlSugar Attributes

- `[SugarTable("TableName")]` - Maps class to database table
- `[SugarColumn(ColumnName = "ColumnName")]` - Maps property to column
- `[SugarColumn(IsPrimaryKey = true)]` - Marks primary key
- `[SugarColumn(IsIgnore = true)]` - Ignores property in database mapping
- `[SugarColumn(IsOwnsOne = true)]` - For owned entity types
