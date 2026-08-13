using Microsoft.AspNetCore.Mvc;
using SqlSugar;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;
using Yi.Framework.Ddd.Application;
using Yi.Framework.Authorization.Abstractions.Attributes;
using Yi.Framework.Authorization.Abstractions.Enums;
using Yi.Framework.OperationRecord.Abstractions.Attributes;
using Yi.Module.Rbac.Application.Contracts.Dtos.Dictionary;
using Yi.Module.Rbac.Application.Contracts.IServices;
using Yi.Module.Rbac.Domain.Entities;
using Yi.Framework.SqlSugarCore.Abstractions;


namespace Yi.Module.Rbac.Application.Services
{
    /// <summary>
    /// Dictionary服务实现
    /// </summary>
    [PermissionResource("system", "dict")]
    [OperLogEntity("字典数据")]
    public class DictionaryService : YiCrudAppService<DictionaryEntity, DictionaryGetOutputDto, DictionaryGetListOutputDto, Guid, DictionaryGetListInputVo, DictionaryCreateInputVo, DictionaryUpdateInputVo>,
       IDictionaryService
    {
        private ISqlSugarRepository<DictionaryEntity, Guid> _repository;
        public DictionaryService(ISqlSugarRepository<DictionaryEntity, Guid> repository) : base(repository)
        {
            _repository= repository;
        }

        /// <summary>
        /// 查询
        /// </summary>

        public override async Task<PagedResultDto<DictionaryGetListOutputDto>> GetListAsync(DictionaryGetListInputVo input)
        {
            RefAsync<int> total = 0;
            var entities = await _repository._DbQueryable
                .WhereIF(input.DictType is not null, x => x.DictType == input.DictType)
                .WhereIF(input.DictLabel is not null, x => x.DictLabel!.Contains(input.DictLabel!))
                .WhereIF(input.State is not null, x => x.State == input.State)
                .OrderByDescending(x => x.OrderNum)
                .ToPageListAsync(input.SkipCount, input.MaxResultCount, total);
            return new PagedResultDto<DictionaryGetListOutputDto>
            {
                TotalCount = total,
                Items = await MapToGetListOutputDtosAsync(entities)
            };
        }


        /// <summary>
        /// 根据字典类型获取业务页面所需的启用字典项
        /// </summary>
        /// <param name="dictType">字典类型编码</param>
        /// <returns>启用状态的字典项列表</returns>
        /// <remarks>字典项是状态、类型等通用展示依赖，不要求调用方拥有字典管理权限；仅跳过资源权限校验，ABP 默认认证仍然生效，框架授权行为升级时需要复核</remarks>
        [Route("dictionary/dict-type/{dictType}")]
        [IgnorePermission]
        public async Task<List<DictionaryGetListOutputDto>> GetDicType([FromRoute] string dictType)
        {
            var entities = await _repository.GetListAsync(u => u.DictType == dictType && u.State == true);
            var result = await MapToGetListOutputDtosAsync(entities);
            return result;
        }
    }
}
