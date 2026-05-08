using Microsoft.Extensions.DependencyInjection;
using SqlSugar;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Caching;
using Yi.Framework.Ddd.Application;
using Yi.Framework.Operation.Abstractions.Attributes;
using Yi.Module.Rbac.Application.Contracts.Dtos.DictionaryType;
using Yi.Module.Rbac.Application.Contracts.IServices;
using Yi.Module.Rbac.Domain.Entities;
using Yi.Module.Rbac.Domain.Shared.Consts;
using Yi.Framework.SqlSugarCore.Abstractions;

namespace Yi.Module.Rbac.Application.Services
{
    /// <summary>
    /// DictionaryType服务实现
    /// </summary>
    [PermissionResource("system", "dict")]
    [OperLogEntity("字典类型")]
    public class DictionaryTypeService : YiCrudAppService<DictionaryTypeAggregateRoot, DictionaryTypeGetOutputDto, DictionaryTypeGetListOutputDto, Guid, DictionaryTypeGetListInputVo, DictionaryTypeCreateInputVo, DictionaryTypeUpdateInputVo>,
       IDictionaryTypeService
    {
        private ISqlSugarRepository<DictionaryTypeAggregateRoot, Guid> _repository;
        public DictionaryTypeService(ISqlSugarRepository<DictionaryTypeAggregateRoot, Guid> repository) : base(repository)
        {
            _repository = repository;
        }

        public override async Task<PagedResultDto<DictionaryTypeGetListOutputDto>> GetListAsync(DictionaryTypeGetListInputVo input)
        {

            RefAsync<int> total = 0;
            var entities = await _repository._DbQueryable.WhereIF(input.DictName is not null, x => x.DictName.Contains(input.DictName!))
                      .WhereIF(input.DictType is not null, x => x.DictType!.Contains(input.DictType!))
                      .WhereIF(input.State is not null, x => x.State == input.State)
                      .WhereIF(input.StartTime is not null && input.EndTime is not null, x => x.CreationTime >= input.StartTime && x.CreationTime <= input.EndTime)
                      .ToPageListAsync(input.SkipCount, input.MaxResultCount, total);

            return new PagedResultDto<DictionaryTypeGetListOutputDto>
            {
                TotalCount = total,
                Items = await MapToGetListOutputDtosAsync(entities)
            };
        }

        protected override async Task CheckCreateInputDtoAsync(DictionaryTypeCreateInputVo input)
        {
            var isExist =
                await _repository.IsAnyAsync(x => x.DictType == input.DictType);
            if (isExist)
            {
                throw new UserFriendlyException(DictionaryConst.Exist);
            }
        }

        protected override async Task CheckUpdateInputDtoAsync(DictionaryTypeAggregateRoot entity, DictionaryTypeUpdateInputVo input)
        {
            var isExist = await _repository._DbQueryable.Where(x => x.Id != entity.Id)
                .AnyAsync(x => x.DictType == input.DictType);
            if (isExist)
            {
                throw new UserFriendlyException(DictionaryConst.Exist);
            }
        }
    }
}
