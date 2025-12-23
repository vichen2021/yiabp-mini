using Microsoft.AspNetCore.Mvc;
using SqlSugar;
using Volo.Abp.Application.Dtos;
using Yi.Framework.Core.Helper;
using Yi.Framework.Ddd.Application;
using Yi.Framework.Rbac.Application.Contracts.Dtos.Dept;
using Yi.Framework.Rbac.Application.Contracts.IServices;
using Yi.Framework.Rbac.Domain.Entities;
using Yi.Framework.Rbac.Domain.Repositories;
using Yi.Framework.Rbac.Domain.Shared.Consts;
using Yi.Framework.Rbac.Domain.Shared.Dtos;

namespace Yi.Framework.Rbac.Application.Services.System
{
    /// <summary>
    /// Dept服务实现
    /// </summary>
    public class DeptService : YiCrudAppService<DeptAggregateRoot, DeptGetOutputDto, DeptGetListOutputDto, Guid,
        DeptGetListInputVo, DeptCreateInputVo, DeptUpdateInputVo>, IDeptService
    {
        private IDeptRepository _repository;

        public DeptService(IDeptRepository repository) : base(repository)
        {
            _repository = repository;
        }

        [RemoteService(false)]
        public async Task<List<Guid>> GetChildListAsync(Guid deptId)
        {
            return await _repository.GetChildListAsync(deptId);
        }

        /// <summary>
        /// 通过角色id查询该角色全部部门
        /// </summary>
        /// <returns></returns>
        //[Route("{roleId}")]
        public async Task<List<DeptGetListOutputDto>> GetRoleIdAsync(Guid roleId)
        {
            var entities = await _repository.GetListRoleIdAsync(roleId);
            return await MapToGetListOutputDtosAsync(entities);
        }

        /// <summary>
        /// 多查
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [Route("dept/list")]
        public async Task<List<DeptGetListOutputDto>> GetListAsync(DeptGetListInputVo input)
        {
            var entities = await _repository._DbQueryable
                .WhereIF(!string.IsNullOrEmpty(input.DeptName), u => u.DeptName.Contains(input.DeptName!))
                .WhereIF(input.State is not null, u => u.State == input.State)
                .OrderBy(u => u.OrderNum, OrderByType.Asc)
                .ToListAsync();
            
            return await MapToGetListOutputDtosAsync(entities);
        }

        protected override async Task CheckCreateInputDtoAsync(DeptCreateInputVo input)
        {
            var isExist =
                await _repository.IsAnyAsync(x => x.DeptCode == input.DeptCode);
            if (isExist)
            {
                throw new UserFriendlyException(DeptConst.Exist);
            }
        }

        protected override async Task CheckUpdateInputDtoAsync(DeptAggregateRoot entity, DeptUpdateInputVo input)
        {
            var isExist = await _repository._DbQueryable.Where(x => x.Id != entity.Id)
                .AnyAsync(x => x.DeptCode == input.DeptCode);
            if (isExist)
            {
                throw new UserFriendlyException(DeptConst.Exist);
            }
        }

        /// <summary>
        /// 获取部门树形列表
        /// </summary>
        /// <returns>树形结构的部门列表</returns>
        public async Task<List<DeptGetTreeListOutputDto>> GetTreeListAsync()
        {
            // 获取所有启用的部门
            var entities = await _repository._DbQueryable
                .Where(x => x.State == true)
                .OrderBy(x => x.OrderNum, OrderByType.Asc)
                .ToListAsync();

            // 使用扩展方法构建树形结构
            var treeDtos = entities.DeptTreeListBuild();
            
            // 转换为DeptGetTreeListOutputDto
            return ConvertTreeDtoToOutputDto(treeDtos);
        }

        /// <summary>
        /// 将DeptTreeDto转换为DeptGetTreeListOutputDto
        /// </summary>
        /// <param name="treeDtos">树形DTO列表</param>
        /// <returns>输出DTO列表</returns>
        private List<DeptGetTreeListOutputDto> ConvertTreeDtoToOutputDto(List<DeptTreeDto> treeDtos)
        {
            var result = new List<DeptGetTreeListOutputDto>();
            
            foreach (var treeDto in treeDtos)
            {
                var outputDto = new DeptGetTreeListOutputDto
                {
                    Id = treeDto.Id,
                    ParentId = treeDto.ParentId,
                    OrderNum = treeDto.OrderNum,
                    DeptName = treeDto.DeptName,
                    DeptCode = treeDto.DeptCode,
                    Leader = treeDto.Leader,
                    Remark = treeDto.Remark,
                    State = treeDto.State,
                    CreationTime = treeDto.CreationTime,
                    Children = treeDto.Children != null ? ConvertTreeDtoToOutputDto(treeDto.Children) : null
                };
                
                result.Add(outputDto);
            }
            
            return result;
        }

    }
}