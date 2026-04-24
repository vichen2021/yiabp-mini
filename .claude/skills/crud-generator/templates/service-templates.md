# Service Templates

## IService Template (普通实体)

⚠️ 架构边界：Application.Contracts 层不能引用 Domain.Entities，IYiCrudAppService 泛型参数只包含 DTO 类型

```csharp
using Yi.Framework.Ddd.Application.Contracts;
using Yi.Framework.{{moduleNamespace}}.Application.Contracts.Dtos.{{entityName}};
{{#if hasEnums}}
using Yi.Framework.{{moduleNamespace}}.Domain.Shared.Enums;
{{/if}}

namespace Yi.Framework.{{moduleNamespace}}.Application.Contracts.IServices
{
    /// <summary>
    /// {{entityComment}}服务接口
    /// </summary>
    public interface I{{entityName}}Service : IYiCrudAppService<{{entityName}}GetOutputDto, {{entityName}}GetListOutputDto, Guid,
        {{entityName}}GetListInputVo, {{entityName}}CreateInputVo, {{entityName}}UpdateInputVo>
    {
    }
}
```

---

## IService Template (树形实体)

```csharp
using Yi.Framework.Ddd.Application.Contracts;
using Yi.Framework.{{moduleNamespace}}.Application.Contracts.Dtos.{{entityName}};
using Yi.Framework.{{moduleNamespace}}.Domain.Shared.Dtos;

namespace Yi.Framework.{{moduleNamespace}}.Application.Contracts.IServices
{
    /// <summary>
    /// {{entityComment}}服务接口
    /// </summary>
    public interface I{{entityName}}Service : IYiCrudAppService<{{entityName}}GetOutputDto, {{entityName}}GetListOutputDto, Guid,
        {{entityName}}GetListInputVo, {{entityName}}CreateInputVo, {{entityName}}UpdateInputVo>
    {
        /// <summary>
        /// 获取树形结构
        /// </summary>
        Task<List<{{entityName}}TreeDto>> GetTreeAsync();
        
        /// <summary>
        /// 获取排除指定节点及其子节点的列表
        /// </summary>
        Task<List<{{entityName}}GetListOutputDto>> GetListExcludeAsync(Guid id);
    }
}
```

---

## Service Template (普通实体 - 基础)

```csharp
using Yi.Framework.Ddd.Application;
using Yi.Framework.{{moduleNamespace}}.Application.Contracts.Dtos.{{entityName}};
using Yi.Framework.{{moduleNamespace}}.Application.Contracts.IServices;
using Yi.Framework.{{moduleNamespace}}.Domain.Entities;
using Yi.Framework.SqlSugarCore.Abstractions;

namespace Yi.Framework.{{moduleNamespace}}.Application.Services
{
    /// <summary>
    /// {{entityComment}}服务实现
    /// </summary>
    public class {{entityName}}Service : YiCrudAppService<{{entityName}}AggregateRoot, {{entityName}}GetOutputDto, {{entityName}}GetListOutputDto, Guid,
        {{entityName}}GetListInputVo, {{entityName}}CreateInputVo, {{entityName}}UpdateInputVo>, I{{entityName}}Service
    {
        private readonly ISqlSugarRepository<{{entityName}}AggregateRoot, Guid> _repository;

        public {{entityName}}Service(ISqlSugarRepository<{{entityName}}AggregateRoot, Guid> repository) : base(repository) =>
            _repository = repository;
    }
}
```

---

## Service Template (树形实体)

```csharp
using Microsoft.AspNetCore.Mvc;
using SqlSugar;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Yi.Framework.Core.Helper;
using Yi.Framework.Ddd.Application;
using Yi.Framework.{{moduleNamespace}}.Application.Contracts.Dtos.{{entityName}};
using Yi.Framework.{{moduleNamespace}}.Application.Contracts.IServices;
using Yi.Framework.{{moduleNamespace}}.Domain.Entities;
using Yi.Framework.{{moduleNamespace}}.Domain.Repositories;
using Yi.Framework.{{moduleNamespace}}.Domain.Shared.Dtos;

namespace Yi.Framework.{{moduleNamespace}}.Application.Services
{
    /// <summary>
    /// {{entityComment}}服务实现
    /// </summary>
    public class {{entityName}}Service : YiCrudAppService<{{entityName}}AggregateRoot, {{entityName}}GetOutputDto, {{entityName}}GetListOutputDto, Guid,
        {{entityName}}GetListInputVo, {{entityName}}CreateInputVo, {{entityName}}UpdateInputVo>, I{{entityName}}Service
    {
        private readonly I{{entityName}}Repository _repository;

        public {{entityName}}Service(I{{entityName}}Repository repository) : base(repository) =>
            _repository = repository;

        [RemoteService(false)]
        public async Task<List<Guid>> GetChildListAsync(Guid {{entityNameLower}}Id)
        {
            return await _repository.GetChildListAsync({{entityNameLower}}Id);
        }

        /// <summary>
        /// 获取列表（树形实体不分页）
        /// </summary>
        [Route("{{entityNameLower}}/list")]
        public new async Task<List<{{entityName}}GetListOutputDto>> GetListAsync({{entityName}}GetListInputVo input)
        {
            var result = await _repository._DbQueryable
                {{#each fields}}
                {{#unless isTreeField}}
                .WhereIF(!string.IsNullOrEmpty(input.{{name}}), x => x.{{name}}.Contains(input.{{name}}!))
                {{/unless}}
                {{/each}}
                .WhereIF(input.State is not null, x => x.State == input.State)
                .OrderBy(x => x.OrderNum, OrderByType.Asc)
                .Select(x => new {{entityName}}GetListOutputDto
                {
                    Id = x.Id,
                    CreationTime = x.CreationTime,
                    CreatorId = x.CreatorId,
                    State = x.State,
                    {{#each fields}}
                    {{#unless isTreeField}}
                    {{name}} = x.{{name}},
                    {{/unless}}
                    {{/each}}
                    ParentId = x.ParentId,
                    Remark = x.Remark,
                    OrderNum = x.OrderNum
                })
                .ToListAsync();

            return result;
        }

        protected override async Task CheckCreateInputDtoAsync({{entityName}}CreateInputVo input)
        {
            {{#each fields}}
            {{#if isIndex}}
            if (!string.IsNullOrEmpty(input.{{name}}))
            {
                var isExist = await _repository.IsAnyAsync(x => x.{{name}} == input.{{name}});
                if (isExist)
                {
                    throw new UserFriendlyException("{{comment}}已存在");
                }
            }
            {{/if}}
            {{/each}}
        }

        protected override async Task CheckUpdateInputDtoAsync({{entityName}}AggregateRoot entity, {{entityName}}UpdateInputVo input)
        {
            {{#each fields}}
            {{#if isIndex}}
            if (!string.IsNullOrEmpty(input.{{name}}))
            {
                var isExist = await _repository._DbQueryable.Where(x => x.Id != entity.Id)
                    .AnyAsync(x => x.{{name}} == input.{{name}});
                if (isExist)
                {
                    throw new UserFriendlyException("{{comment}}已存在");
                }
            }
            {{/if}}
            {{/each}}

            // 校验上级不能是自己或自己的子孙
            if (input.ParentId.HasValue && input.ParentId.Value != Guid.Empty)
            {
                if (input.ParentId.Value == entity.Id)
                {
                    throw new UserFriendlyException("上级不能是自己");
                }

                var childrenIds = await GetAllChildrenIdsAsync(entity.Id);
                if (childrenIds.Contains(input.ParentId.Value))
                {
                    throw new UserFriendlyException("上级不能是当前节点的子孙节点，这会造成循环引用");
                }
            }
        }

        /// <summary>
        /// 获取树形结构
        /// </summary>
        public async Task<List<{{entityName}}TreeDto>> GetTreeAsync()
        {
            var entities = await _repository._DbQueryable
                .Where(x => x.State == true)
                .OrderBy(x => x.OrderNum, OrderByType.Asc)
                .ToListAsync();
            return entities.{{entityName}}TreeBuild();
        }

        /// <summary>
        /// 获取排除指定节点及其子节点的列表
        /// </summary>
        [HttpGet]
        [Route("{{entityNameLower}}/list/exclude/{id}")]
        public async Task<List<{{entityName}}GetListOutputDto>> GetListExcludeAsync(Guid id)
        {
            var excludeIds = await GetAllChildrenIdsAsync(id);
            excludeIds.Add(id);

            var result = await _repository._DbQueryable
                .Where(x => !excludeIds.Contains(x.Id))
                .OrderBy(x => x.OrderNum, OrderByType.Asc)
                .Select(x => new {{entityName}}GetListOutputDto
                {
                    Id = x.Id,
                    CreationTime = x.CreationTime,
                    CreatorId = x.CreatorId,
                    State = x.State,
                    {{#each fields}}
                    {{#unless isTreeField}}
                    {{name}} = x.{{name}},
                    {{/unless}}
                    {{/each}}
                    ParentId = x.ParentId,
                    Remark = x.Remark,
                    OrderNum = x.OrderNum
                })
                .ToListAsync();

            return result;
        }

        private async Task<List<Guid>> GetAllChildrenIdsAsync(Guid {{entityNameLower}}Id)
        {
            var result = new List<Guid>();
            var allEntities = await _repository._DbQueryable.ToListAsync();
            GetChildrenIdsRecursive({{entityNameLower}}Id, allEntities, result);
            return result;
        }

        private void GetChildrenIdsRecursive(Guid parentId, List<{{entityName}}AggregateRoot> allEntities, List<Guid> result)
        {
            var children = allEntities.Where(x => x.ParentId == parentId).ToList();
            foreach (var child in children)
            {
                result.Add(child.Id);
                GetChildrenIdsRecursive(child.Id, allEntities, result);
            }
        }
    }
}
```