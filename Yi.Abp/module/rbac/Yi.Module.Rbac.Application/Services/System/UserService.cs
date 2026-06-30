using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MiniExcelLibs;
using MiniExcelLibs.Attributes;
using SqlSugar;
using TencentCloud.Tcr.V20190924.Models;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Caching;
using Volo.Abp.EventBus.Local;
using Volo.Abp.Users;
using Yi.Framework.Ddd.Application;
using Yi.Module.Rbac.Application.Contracts.Dtos.User;
using Yi.Module.Rbac.Application.Contracts.IServices;
using Yi.Framework.Authorization.Abstractions.Attributes;
using Yi.Framework.OperationRecord.Abstractions.Attributes;
using Yi.Module.Rbac.Domain.Entities;
using Yi.Module.Rbac.Domain.Managers;
using Yi.Module.Rbac.Domain.Repositories;
using Yi.Module.Rbac.Domain.Shared.Caches;
using Yi.Module.Rbac.Domain.Shared.Consts;
using Yi.Module.Rbac.Domain.Shared.Etos;
using Yi.Module.Rbac.Domain.Shared.Enums;
using Yi.Framework.OperationRecord.Abstractions.Enums;
using Yi.Framework.SqlSugarCore.Abstractions;

namespace Yi.Module.Rbac.Application.Services
{
    /// <summary>
    /// User服务实现
    /// </summary>
    [PermissionResource("system", "user")]
    [OperLogEntity("用户")]
    public class UserService : YiCrudAppService<UserAggregateRoot, UserGetOutputDto, UserGetListOutputDto, Guid,
        UserGetListInputVo, UserCreateInputVo, UserUpdateInputVo>, IUserService
    //IUserService
    {
        protected ILocalEventBus LocalEventBus => LazyServiceProvider.LazyGetRequiredService<ILocalEventBus>();

        public UserService(ISqlSugarRepository<UserAggregateRoot, Guid> repository, UserManager userManager,
            IUserRepository userRepository, ICurrentUser currentUser, IDeptService deptService,
            ILocalEventBus localEventBus,
            ISqlSugarRepository<DeptAggregateRoot, Guid> deptRepository,
            ISqlSugarRepository<ConfigAggregateRoot, Guid> configRepository,
            IDistributedCache<UserInfoCacheItem, UserInfoCacheKey> userCache) : base(repository)
            =>
                (_userManager, _userRepository, _currentUser, _deptService, _repository, _localEventBus, _deptRepository, _configRepository) =
                (userManager, userRepository, currentUser, deptService, repository, localEventBus, deptRepository, configRepository);

        private UserManager _userManager { get; set; }
        private ISqlSugarRepository<UserAggregateRoot, Guid> _repository;
        private ISqlSugarRepository<DeptAggregateRoot, Guid> _deptRepository;
        private ISqlSugarRepository<ConfigAggregateRoot, Guid> _configRepository;
        private IUserRepository _userRepository { get; set; }
        private IDeptService _deptService { get; set; }

        private ICurrentUser _currentUser { get; set; }

        private ILocalEventBus _localEventBus;

        /// <summary>
        /// 查询用户
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [Permission("system:user:query")]
        public override async Task<PagedResultDto<UserGetListOutputDto>> GetListAsync(UserGetListInputVo input)
        {
            RefAsync<int> total = 0;
            List<Guid> deptIds = null;
            if (input.DeptId is not null)
            {
                deptIds = await _deptService.GetChildListAsync(input.DeptId ?? Guid.Empty);
            }


            List<Guid> ids = input.Ids?.Split(",").Select(x => Guid.Parse(x)).ToList();
            var outPut = await _repository._DbQueryable.WhereIF(!string.IsNullOrEmpty(input.UserName),
                    x => x.UserName.Contains(input.UserName!))
                .WhereIF(input.Phone is not null, x => x.Phone.ToString()!.Contains(input.Phone.ToString()!))
                .WhereIF(!string.IsNullOrEmpty(input.Name), x => x.Name!.Contains(input.Name!))
                .WhereIF(input.State is not null, x => x.State == input.State)
                .WhereIF(input.StartTime is not null && input.EndTime is not null,
                    x => x.CreationTime >= input.StartTime && x.CreationTime <= input.EndTime)

                //这个为过滤当前部门，加入数据权限后，将由数据权限控制
                .WhereIF(input.DeptId is not null, x => deptIds.Contains(x.DeptId ?? Guid.Empty))
                .WhereIF(ids is not null, x => ids.Contains(x.Id))
                .LeftJoin<DeptAggregateRoot>((user, dept) => user.DeptId == dept.Id)
                .OrderByDescending(user => user.CreationTime)
                .Select((user, dept) => new UserGetListOutputDto(), true)
                .ToPageListAsync(input.SkipCount, input.MaxResultCount, total);

            var result = new PagedResultDto<UserGetListOutputDto>();
            result.Items = outPut;
            result.TotalCount = total;
            return result;
        }


        protected override UserAggregateRoot MapToEntity(UserCreateInputVo createInput)
        {
            var output = base.MapToEntity(createInput);
            output.EncryPassword = new Domain.Entities.ValueObjects.EncryPasswordValueObject(createInput.Password);
            return output;
        }

        /// <summary>
        /// 添加用户
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [OperLog("添加用户", OperEnum.Insert)]
        [Permission("system:user:add")]
        public async override Task<UserGetOutputDto> CreateAsync(UserCreateInputVo input)
        {
            var entitiy = await MapToEntityAsync(input);

            await _userManager.CreateAsync(entitiy);
            await _userManager.GiveUserSetRoleAsync(new List<Guid> { entitiy.Id }, input.RoleIds);
            await _userManager.GiveUserSetPostAsync(new List<Guid> { entitiy.Id }, input.PostIds);

            var result = await MapToGetOutputDtoAsync(entitiy);
            return result;
        }

        protected override async Task<UserAggregateRoot> MapToEntityAsync(UserCreateInputVo createInput)
        {
            var entitiy = await base.MapToEntityAsync(createInput);
            entitiy.BuildPassword();
            return entitiy;
        }

        /// <summary>
        /// 单查
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Permission("system:user:query")]
        public override async Task<UserGetOutputDto> GetAsync(Guid id)
        {
            //使用导航树形查询
            var entity = await _repository._DbQueryable.Includes(u => u.Roles).Includes(u => u.Posts)
                .Includes(u => u.Dept).InSingleAsync(id);

            return await MapToGetOutputDtoAsync(entity);
        }

        /// <summary>
        /// 更新用户
        /// </summary>
        /// <param name="id"></param>
        /// <param name="input"></param>
        /// <returns></returns>
        [OperLog("更新用户", OperEnum.Update)]
        [Permission("system:user:edit")]
        public override async Task<UserGetOutputDto> UpdateAsync(Guid id, UserUpdateInputVo input)
        {
            if (input.UserName == UserConst.SuperAdminUserName || input.UserName == UserConst.TenantAdmin)
            {
                throw new UserFriendlyException(UserConst.Name_Not_Allowed);
            }

            if (await _repository.IsAnyAsync(u => input.UserName!.Equals(u.UserName) && !id.Equals(u.Id)))
            {
                throw new UserFriendlyException(UserConst.Exist);
            }

            var entity = await _repository.GetByIdAsync(id);
            //更新密码，特殊处理
            if (!string.IsNullOrWhiteSpace(input.Password))
            {
                entity.EncryPassword.Password = input.Password;
                entity.BuildPassword();
            }

            await MapToEntityAsync(input, entity);

            var res1 = await _repository.UpdateAsync(entity);
            await _userManager.GiveUserSetRoleAsync(new List<Guid> { id }, input.RoleIds);
            await _userManager.GiveUserSetPostAsync(new List<Guid> { id }, input.PostIds);
            return await MapToGetOutputDtoAsync(entity);
        }

        /// <summary>
        /// 更新个人中心
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [IgnorePermission]
        [OperLog("更新个人信息", OperEnum.Update)]
        public async Task<UserGetOutputDto> UpdateProfileAsync(ProfileUpdateInputVo input)
        {
            var entity = await _repository.GetByIdAsync(_currentUser.Id);
            ObjectMapper.Map(input, entity);

            await _repository.UpdateAsync(entity);
            var dto = await MapToGetOutputDtoAsync(entity);

            return dto;
        }

        /// <summary>
        /// 更新状态
        /// </summary>
        /// <param name="id"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        [Route("user/{id}/{state}")]
        [OperLog("更新用户状态", OperEnum.Update)]
        [Permission("system:user:edit")]
        public async Task<UserGetOutputDto> UpdateStateAsync([FromRoute] Guid id, [FromRoute] bool state)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity is null)
            {
                throw new ApplicationException("用户未存在");
            }

            entity.State = state;
            await _repository.UpdateAsync(entity);
            return await MapToGetOutputDtoAsync(entity);
        }

        [OperLog("删除用户", OperEnum.Delete)]
        [Permission("system:user:remove")]
        public override async Task DeleteAsync(Guid id)
        {
            await base.DeleteAsync(id);
        }

        [Permission("system:user:export")]
        public override Task<IActionResult> PostExportAsync(UserGetListInputVo input)
        {
            return base.PostExportAsync(input);
        }

        [Permission("system:user:import")]
        public override Task PostImportExcelAsync(List<UserCreateInputVo> input)
        {
            return base.PostImportExcelAsync(input);
        }

        [HttpPost]
        [Route("user/importData")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [OperLog("导入用户", OperEnum.Import)]
        [Permission("system:user:import")]
        public async Task<object> PostImportDataAsync([FromForm] UserImportForm input)
        {
            if (input.File is null || input.File.Length == 0)
            {
                return new { code = 500, msg = "导入文件不能为空" };
            }

            var successMessages = new List<string>();
            var failureMessages = new List<string>();
            var initPassword = await GetInitPasswordAsync();

            await using var stream = input.File.OpenReadStream();
            var rows = (await MiniExcel.QueryAsync<UserImportRow>(stream)).ToList();
            var rowIndex = 1;

            foreach (var row in rows)
            {
                rowIndex++;
                if (row.IsEmpty())
                {
                    continue;
                }

                try
                {
                    await ImportUserRowAsync(row, rowIndex, initPassword, input.UpdateSupport, successMessages);
                }
                catch (Exception ex)
                {
                    failureMessages.Add($"第 {rowIndex} 行导入失败：{ex.Message}");
                }
            }

            var msg = BuildImportMessage(successMessages, failureMessages);
            return new { code = failureMessages.Count == 0 && successMessages.Count > 0 ? 200 : 500, msg };
        }

        [HttpPost]
        [Route("user/importTemplate")]
        [OperLog("下载用户导入模板", OperEnum.Export)]
        [Permission("system:user:import")]
        public async Task<IActionResult> PostImportTemplateAsync()
        {
            var rows = new List<Dictionary<string, object?>>
            {
                new()
                {
                    ["部门编号"] = null,
                    ["用户账号"] = null,
                    ["用户昵称"] = null,
                    ["用户邮箱"] = null,
                    ["手机号码"] = null,
                    ["用户性别"] = null,
                    ["账号状态"] = null,
                }
            };

            await using var stream = new MemoryStream();
            await MiniExcel.SaveAsAsync(stream, rows);
            return new FileContentResult(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
            {
                FileDownloadName = "用户导入模板.xlsx"
            };
        }

        private async Task ImportUserRowAsync(UserImportRow row, int rowIndex, string initPassword, bool updateSupport, List<string> successMessages)
        {
            var userName = Require(row.UserName, "用户账号");
            if (userName == UserConst.SuperAdminUserName || userName == UserConst.TenantAdmin)
            {
                throw new UserFriendlyException(UserConst.Name_Not_Allowed);
            }

            var phone = ParsePhone(row.Phone);
            var sex = ParseSex(row.Sex);
            var state = ParseState(row.State);
            var deptId = await GetDeptIdAsync(row.DeptCode);
            var entity = await _repository.GetFirstAsync(x => x.UserName == userName);

            if (entity is not null)
            {
                if (!updateSupport)
                {
                    throw new UserFriendlyException($"用户 {userName} 已存在");
                }

                await CheckPhoneRepeatAsync(phone, entity.Id);
                entity.ApplyImportProfile(row.Nick, row.Email, phone, sex, deptId, state);
                await _repository.UpdateAsync(entity);
                successMessages.Add($"第 {rowIndex} 行用户 {userName} 更新成功");
                return;
            }

            entity = new UserAggregateRoot(userName, initPassword, phone, row.Nick);
            entity.ApplyImportProfile(row.Nick, row.Email, phone, sex, deptId, state);

            await _userManager.CreateAsync(entity);

            try
            {
                await _userManager.SetDefautRoleAsync(entity.Id);
                successMessages.Add($"第 {rowIndex} 行用户 {userName} 导入成功");
            }
            catch (Exception ex)
            {
                successMessages.Add($"第 {rowIndex} 行用户 {userName} 导入成功，默认角色分配失败：{ex.Message}");
            }
        }

        private async Task<string> GetInitPasswordAsync()
        {
            var config = await _configRepository.GetFirstAsync(x => x.ConfigKey == "sys.user.initPassword");
            return string.IsNullOrWhiteSpace(config?.ConfigValue) ? "123456" : config.ConfigValue.Trim();
        }

        private async Task<Guid?> GetDeptIdAsync(string? deptCode)
        {
            if (string.IsNullOrWhiteSpace(deptCode))
            {
                return null;
            }

            var code = deptCode.Trim();
            var dept = await _deptRepository.GetFirstAsync(x => x.DeptCode == code);
            if (dept is null)
            {
                throw new UserFriendlyException($"部门编号 {code} 不存在");
            }

            return dept.Id;
        }

        private async Task CheckPhoneRepeatAsync(long? phone, Guid userId)
        {
            if (phone is null)
            {
                return;
            }

            if (await _repository.IsAnyAsync(x => x.Phone == phone && x.Id != userId))
            {
                throw new UserFriendlyException(UserConst.Phone_Repeat);
            }
        }

        private static string Require(string? value, string fieldName)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new UserFriendlyException($"{fieldName}不能为空");
            }

            return value.Trim();
        }

        private static long? ParsePhone(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            var text = value.Trim();
            if (text.EndsWith(".0"))
            {
                text = text[..^2];
            }

            if (!long.TryParse(text, out var phone))
            {
                throw new UserFriendlyException($"手机号码 {value} 格式错误");
            }

            return phone;
        }

        private static SexEnum ParseSex(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return SexEnum.Unknown;
            }

            return value.Trim().ToLower() switch
            {
                "男" or "男性" or "0" or "male" => SexEnum.Male,
                "女" or "女性" or "1" or "woman" or "female" => SexEnum.Woman,
                _ => SexEnum.Unknown,
            };
        }

        private static bool ParseState(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return true;
            }

            return value.Trim().ToLower() switch
            {
                "正常" or "启用" or "开启" or "true" or "1" or "是" or "y" or "yes" => true,
                "停用" or "禁用" or "关闭" or "false" or "0" or "否" or "n" or "no" => false,
                _ => throw new UserFriendlyException($"账号状态 {value} 格式错误"),
            };
        }

        private static string BuildImportMessage(List<string> successMessages, List<string> failureMessages)
        {
            var messages = successMessages.Concat(failureMessages).ToList();
            if (messages.Count == 0)
            {
                return "未读取到可导入的数据";
            }

            return string.Join("<br/>", messages.Select(WebUtility.HtmlEncode));
        }

        private sealed class UserImportRow
        {
            [ExcelColumnName("部门编号")]
            public string? DeptCode { get; set; }

            [ExcelColumnName("用户账号")]
            public string? UserName { get; set; }

            [ExcelColumnName("用户昵称")]
            public string? Nick { get; set; }

            [ExcelColumnName("用户邮箱")]
            public string? Email { get; set; }

            [ExcelColumnName("手机号码")]
            public string? Phone { get; set; }

            [ExcelColumnName("用户性别")]
            public string? Sex { get; set; }

            [ExcelColumnName("账号状态")]
            public string? State { get; set; }

            public bool IsEmpty()
            {
                return string.IsNullOrWhiteSpace(DeptCode)
                       && string.IsNullOrWhiteSpace(UserName)
                       && string.IsNullOrWhiteSpace(Nick)
                       && string.IsNullOrWhiteSpace(Email)
                       && string.IsNullOrWhiteSpace(Phone)
                       && string.IsNullOrWhiteSpace(Sex)
                       && string.IsNullOrWhiteSpace(State);
            }
        }

        public sealed class UserImportForm
        {
            public IFormFile? File { get; set; }

            public bool UpdateSupport { get; set; }
        }

        /// <summary>
        /// 获取指定部门及其所有子部门下的用户列表
        /// </summary>
        /// <param name="deptId">部门ID</param>
        /// <returns>用户列表</returns>
        [HttpGet]
        [Route("user/dept/{deptId}")]
        [Permission("system:user:query")]
        public async Task<List<UserGetListOutputDto>> GetUsersByDeptAsync(Guid deptId)
        {
            // 获取当前部门及其所有子部门的ID列表
            var deptIds = await _deptService.GetChildListAsync(deptId);
            
            // 将当前部门ID也加入列表
            if (!deptIds.Contains(deptId))
            {
                deptIds.Add(deptId);
            }

            // 查询所有这些部门下的用户
            var users = await _repository._DbQueryable
                .Where(u => deptIds.Contains(u.DeptId ?? Guid.Empty))
                .LeftJoin<DeptAggregateRoot>((user, dept) => user.DeptId == dept.Id)
                .OrderByDescending(user => user.CreationTime)
                .Select((user, dept) => new UserGetListOutputDto(), true)
                .ToListAsync();

            return users;
        }
    }
}
