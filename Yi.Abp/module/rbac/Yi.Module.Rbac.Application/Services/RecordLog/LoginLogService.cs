using Microsoft.AspNetCore.Mvc;
using MiniExcelLibs;
using SqlSugar;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Yi.Framework.Ddd.Application;
using Yi.Framework.Authorization.Abstractions.Attributes;
using Yi.Framework.OperationRecord.Abstractions.Attributes;
using Yi.Framework.OperationRecord.Abstractions.Enums;
using Yi.Framework.SqlSugarCore.Abstractions;
using Yi.Module.Rbac.Application.Contracts.Dtos.LoginLog;
using Yi.Module.Rbac.Domain.Entities;

namespace Yi.Module.Rbac.Application.Services.RecordLog
{
    [PermissionResource("monitor", "loginlog")]
    [OperLogEntity("登录日志")]
    public class LoginLogService : YiCrudAppService<LoginLogAggregateRoot, LoginLogGetListOutputDto, Guid, LoginLogGetListInputVo>
    {
        private readonly ISqlSugarRepository<LoginLogAggregateRoot> _repository;

        public LoginLogService(ISqlSugarRepository<LoginLogAggregateRoot, Guid> repository) : base(repository)
        {
            _repository = repository;
        }

        public override async Task<PagedResultDto<LoginLogGetListOutputDto>> GetListAsync(LoginLogGetListInputVo input)
        {
            RefAsync<int> total = 0;
            var entities = await BuildQuery(input)
                .OrderByDescending(it => it.CreationTime)
                .ToPageListAsync(input.SkipCount, input.MaxResultCount, total);
            return new PagedResultDto<LoginLogGetListOutputDto>(total, await MapToGetListOutputDtosAsync(entities));
        }

        [RemoteService(false)]
        public override Task<LoginLogGetListOutputDto> UpdateAsync(Guid id, LoginLogGetListOutputDto input)
        {
            return base.UpdateAsync(id, input);
        }

        [HttpDelete("clean")]
        [Permission("monitor:loginlog:remove")]
        [OperLog("清空登录日志", OperEnum.Clear)]
        public async Task DeleteCleanAsync()
        {
            await _repository._Db.Deleteable<LoginLogAggregateRoot>().ExecuteCommandAsync();
        }

        [HttpPost("export")]
        [Permission("monitor:loginlog:export")]
        [OperLog("导出登录日志", OperEnum.Export)]
        public async Task<IActionResult> PostExportAsync(LoginLogGetListInputVo input)
        {
            var entities = await BuildQuery(input)
                .OrderByDescending(x => x.CreationTime)
                .ToListAsync();
            var output = await MapToGetListOutputDtosAsync(entities);

            var tempPath = Path.Combine(AppContext.BaseDirectory, "temp", "exports");
            if (!Directory.Exists(tempPath))
            {
                Directory.CreateDirectory(tempPath);
            }

            var filePath = Path.Combine(tempPath, $"LoginLog_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}_{Guid.NewGuid()}.xlsx");
            await MiniExcel.SaveAsAsync(filePath, output);
            return new PhysicalFileResult(filePath, "application/vnd.ms-excel");
        }

        [HttpGet("unlock/{userName}")]
        [Permission("monitor:loginlog:edit")]
        public Task UnlockAsync(string userName)
        {
            return Task.CompletedTask;
        }

        private ISugarQueryable<LoginLogAggregateRoot> BuildQuery(LoginLogGetListInputVo input)
        {
            return _repository._DbQueryable
                .WhereIF(!string.IsNullOrEmpty(input.LoginIp), x => x.LoginIp.Contains(input.LoginIp!))
                .WhereIF(!string.IsNullOrEmpty(input.LoginUser), x => x.LoginUser!.Contains(input.LoginUser!))
                .WhereIF(input.StartTime is not null && input.EndTime is not null,
                    x => x.CreationTime >= input.StartTime && x.CreationTime <= input.EndTime);
        }
    }
}
