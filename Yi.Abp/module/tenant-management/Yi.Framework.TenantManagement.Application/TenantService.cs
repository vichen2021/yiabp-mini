using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SqlSugar;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Data;
using Volo.Abp.Modularity;
using Volo.Abp.Uow;
using Yi.Framework.Ddd.Application;
using Yi.Framework.Rbac.Domain.Entities;
using Yi.Framework.Rbac.Domain.Managers;
using Yi.Framework.SqlSugarCore.Abstractions;
using Yi.Framework.TenantManagement.Application.Contracts;
using Yi.Framework.TenantManagement.Application.Contracts.Dtos;
using Yi.Framework.TenantManagement.Domain;

namespace Yi.Framework.TenantManagement.Application
{
    /// <summary>
    /// 租户管理
    /// </summary>
    public class TenantService :
        YiCrudAppService<TenantAggregateRoot, TenantGetOutputDto, TenantGetListOutputDto, Guid, TenantGetListInput,
            TenantCreateInput, TenantUpdateInput>, ITenantService
    {
        private ISqlSugarRepository<TenantAggregateRoot, Guid> _repository;
        private IDataSeeder _dataSeeder;
        private readonly DbConnOptions _dbConnOptions;
        private readonly SqlSugarAndConfigurationTenantStore _tenantStore;
        private readonly UserManager _userManager;

        public TenantService(ISqlSugarRepository<TenantAggregateRoot, Guid> repository, IDataSeeder dataSeeder,
            IOptions<DbConnOptions> dbConnOptions, SqlSugarAndConfigurationTenantStore tenantStore, UserManager userManager) :
            base(repository)
        {
            _repository = repository;
            _dataSeeder = dataSeeder;
            _dbConnOptions = dbConnOptions.Value;
            _tenantStore = tenantStore;
            _userManager = userManager;
        }

        /// <summary>
        /// 租户单查
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public override Task<TenantGetOutputDto> GetAsync(Guid id)
        {
            return base.GetAsync(id);
        }

        /// <summary>
        /// 租户多查
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public override async Task<PagedResultDto<TenantGetListOutputDto>> GetListAsync(TenantGetListInput input)
        {
            RefAsync<int> total = 0;

            var entities = await _repository._DbQueryable
                .WhereIF(!string.IsNullOrEmpty(input.Name), x => x.Name.Contains(input.Name!))
                .WhereIF(!string.IsNullOrEmpty(input.ContactUserName), x => x.ContactUserName!.Contains(input.ContactUserName!))
                .WhereIF(!string.IsNullOrEmpty(input.ContactPhone), x => x.ContactPhone!.Contains(input.ContactPhone!))
                .WhereIF(input.StartTime is not null && input.EndTime is not null,
                    x => x.CreationTime >= input.StartTime && x.CreationTime <= input.EndTime)
                .ToPageListAsync(input.SkipCount, input.MaxResultCount, total);
            return new PagedResultDto<TenantGetListOutputDto>(total, await MapToGetListOutputDtosAsync(entities));
        }

        /// <summary>
        /// 租户选项
        /// </summary>
        /// <returns></returns>
        public async Task<List<TenantSelectOutputDto>> GetSelectAsync()
        {
            var entites = await _repository._DbQueryable.ToListAsync();
            return entites.Select(x => new TenantSelectOutputDto { Id = x.Id, Name = x.Name }).ToList();
        }


        /// <summary>
        /// 创建租户
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public override async Task<TenantGetOutputDto> CreateAsync(TenantCreateInput input)
        {
            if (!_dbConnOptions.EnabledSaasMultiTenancy)
            {
                throw new UserFriendlyException("创建失败，系统未开启多租户功能，请在配置文件中启用");
            }

            if (await _repository.IsAnyAsync(x => x.Name == input.Name))
            {
                throw new UserFriendlyException("创建失败，当前租户已存在");
            }

            return await base.CreateAsync(input);
        }

        /// <summary>
        /// 更新租户
        /// </summary>
        /// <param name="id"></param>
        /// <param name="input"></param>
        /// <returns></returns>
        public override async Task<TenantGetOutputDto> UpdateAsync(Guid id, TenantUpdateInput input)
        {
            var oldTenant = await _repository.GetByIdAsync(id);

            if (oldTenant.Name != input.Name && await _repository.IsAnyAsync(x => x.Name == input.Name))
            {
                throw new UserFriendlyException("租户名已经存在");
            }

            var result = await base.UpdateAsync(id, input);

            await _tenantStore.RemoveCacheAsync(id, oldTenant.Name);

            return result;
        }


        /// <summary>
        /// 租户删除
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public override Task DeleteAsync(IEnumerable<Guid> ids)
        {
            return base.DeleteAsync(ids);
        }


        /// <summary>
        /// 初始化租户
        /// </summary>
        /// <param name="id"></param>
        /// <param name="input">初始化参数（包含管理员账号密码）</param>
        /// <returns></returns>
        [HttpPut("tenant/init/{id}")]
        public async Task<TenantInitOutputDto> InitAsync([FromRoute] Guid id, [FromBody] TenantInitInput input)
        {
            var tenant = await _repository.GetByIdAsync(id);
            if (tenant is null)
            {
                throw new UserFriendlyException("租户不存在");
            }

            await CurrentUnitOfWork.SaveChangesAsync();

            bool databaseExists = false;
            int tableCount = 0;

            // 检查数据库状态（在租户上下文中）
            using (CurrentTenant.Change(id))
            {
                using (var checkUow = UnitOfWorkManager.Begin(requiresNew: true, isTransactional: false))
                {
                    ISqlSugarClient db = await _repository.GetDbContextAsync();
                    try
                    {
                        var dbs = db.DbMaintenance.GetDataBaseList();
                        databaseExists = dbs.Any(x => x?.ToString() == tenant.Name);
                    }
                    catch
                    {
                        databaseExists = false;
                    }

                    if (databaseExists)
                    {
                        var tables = db.DbMaintenance.GetTableInfoList();
                        tableCount = tables.Count;
                    }

                    await checkUow.CompleteAsync();
                }
            }

            // 如果数据库有数据且不是强制初始化，返回需要确认
            if (databaseExists && tableCount > 0 && !input.IsForce)
            {
                return new TenantInitOutputDto { NeedForce = true };
            }

            // 执行初始化（在新的租户上下文中）
            using (CurrentTenant.Change(id))
            {
                await CodeFirst(this.LazyServiceProvider, tenant.Name);
                await _dataSeeder.SeedAsync(id);

                // 创建租户管理员账号
                if (!string.IsNullOrEmpty(input.Username) && !string.IsNullOrEmpty(input.Password))
                {
                    var adminUser = new UserAggregateRoot(input.Username, input.Password, null, "租户管理员");
                    await _userManager.CreateAsync(adminUser);
                    await _userManager.SetDefautRoleAsync(adminUser.Id);
                }
            }

            return new TenantInitOutputDto { NeedForce = false };
        }

        private async Task CodeFirst(IServiceProvider service, string databaseName)
        {
            var moduleContainer = service.GetRequiredService<IModuleContainer>();

            //没有数据库，不能创工作单元，创建库，先关闭
            using (var uow = UnitOfWorkManager.Begin(requiresNew: true, isTransactional: false))
            {
                ISqlSugarClient db = await _repository.GetDbContextAsync();
                db.DbMaintenance.CreateDatabase(databaseName);

                List<Type> types = new List<Type>();
                foreach (var module in moduleContainer.Modules)
                {
                    types.AddRange(module.Assembly.GetTypes()
                        .Where(x => x.GetCustomAttribute<IgnoreCodeFirstAttribute>() == null)
                        .Where(x => x.GetCustomAttribute<SugarTable>() != null)
                        .Where(x => x.GetCustomAttribute<DefaultTenantTableAttribute>() is null)
                        .Where(x => x.GetCustomAttribute<SplitTableAttribute>() is null));
                }

                if (types.Count > 0)
                {
                    db.CodeFirst.InitTables(types.ToArray());
                }

                await uow.CompleteAsync();
            }
        }
    }
}