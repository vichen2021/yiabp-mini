using Microsoft.AspNetCore.Mvc;
using MiniExcelLibs;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Yi.Framework.Authorization.Abstractions.Attributes;
using Yi.Framework.Ddd.Application;
using Yi.Framework.OperationRecord.Abstractions.Attributes;
using Yi.Framework.OperationRecord.Abstractions.Enums;
using Yi.Module.Rbac.Application.Contracts.Dtos.LoginLog;
using Yi.Module.Rbac.Domain.Entities;
using Yi.Module.Rbac.Domain.Repositories;

namespace Yi.Module.Rbac.Application.Services.RecordLog
{
    [PermissionResource("monitor", "loginlog")]
    [OperLogEntity("登录日志")]
    public class LoginLogService : YiCrudAppService<LoginLogAggregateRoot, LoginLogGetListOutputDto, Guid, LoginLogGetListInputVo>
    {
        private readonly ILoginLogRepository _repository;

        public LoginLogService(ILoginLogRepository repository) : base(repository)
        {
            _repository = repository;
        }

        public override async Task<PagedResultDto<LoginLogGetListOutputDto>> GetListAsync(LoginLogGetListInputVo input)
        {
            var (entities, total) = await _repository.GetFilteredPagedAsync(
                input.LoginIp, input.LoginUser, input.StartTime, input.EndTime,
                input.SkipCount, input.MaxResultCount);
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
            await _repository.DeleteAllAsync();
        }

        [HttpPost("export")]
        [Permission("monitor:loginlog:export")]
        [OperLog("导出登录日志", OperEnum.Export)]
        public async Task<IActionResult> PostExportAsync(LoginLogGetListInputVo input)
        {
            var entities = await _repository.GetFilteredListAsync(
                input.LoginIp, input.LoginUser, input.StartTime, input.EndTime);
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

    }
}
