using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SqlSugar;
using Volo.Abp;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;
using Yi.Abp.SqlsugarCore;
using Yi.Framework.SqlSugarCore;
using Yi.Framework.SqlSugarCore.Abstractions;
using Yi.Module.TenantManagement.Domain.Entities;

namespace Yi.Abp.DbMigrator;

[DependsOn(
    typeof(YiAbpSqlSugarCoreModule),
    typeof(AbpAutofacModule)
)]
public class DbMigratorModule : AbpModule
{
    public override async Task OnApplicationInitializationAsync(ApplicationInitializationContext context)
    {
        var serviceProvider = context.ServiceProvider;
        var options = serviceProvider.GetRequiredService<IOptions<DbConnOptions>>().Value;
        var logger = serviceProvider.GetRequiredService<ILogger<DbMigratorModule>>();

        if (!options.EnabledSaasMultiTenancy)
        {
            logger.LogInformation("多租户未启用，跳过租户数据库同步。");
            return;
        }

        // 收集需要 CodeFirst 的实体类型（过滤规则与宿主 CodeFirst 一致）
        var moduleContainer = serviceProvider.GetRequiredService<IModuleContainer>();
        var entityTypes = moduleContainer.Modules
            .SelectMany(m => m.Assembly.GetTypes())
            .Where(t => t.GetCustomAttribute<IgnoreCodeFirstAttribute>() == null
                && t.GetCustomAttribute<SugarTable>() != null
                && t.GetCustomAttribute<DefaultTenantTableAttribute>() == null
                && t.GetCustomAttribute<SplitTableAttribute>() == null)
            .ToArray();

        // 直接用根容器的 ISqlSugarDbContext 查询宿主库全量租户列表（与框架 InitializeDatabase 写法一致）
        var serializeService = serviceProvider.GetRequiredService<ISerializeService>();
        var dbContext = serviceProvider.GetRequiredService<ISqlSugarDbContext>();
        var tenants = await dbContext.SqlSugarClient.CopyNew()
            .Queryable<TenantAggregateRoot>()
            .ToListAsync();

        if (tenants.Count == 0)
        {
            logger.LogInformation("未找到任何租户，跳过租户数据库同步。");
            return;
        }

        logger.LogInformation("开始同步 {Count} 个租户的数据库结构...", tenants.Count);

        foreach (var tenant in tenants)
        {
            if (string.IsNullOrWhiteSpace(tenant.TenantConnectionString))
            {
                logger.LogWarning("租户 [{Name}] 未配置独立连接字符串，跳过。", tenant.Name);
                continue;
            }

            try
            {
                logger.LogInformation("正在同步租户 [{Name}] 数据库结构...", tenant.Name);

                var tenantDb = new SqlSugarClient(new ConnectionConfig
                {
                    ConnectionString = tenant.TenantConnectionString,
                    DbType = tenant.DbType,
                    IsAutoCloseConnection = true,
                    ConfigureExternalServices = new ConfigureExternalServices
                    {
                        SerializeService = serializeService,
                        EntityNameService = (type, entity) =>
                        {
                            if (options.EnableUnderLine && !entity.DbTableName.Contains('_'))
                                entity.DbTableName = UtilMethods.ToUnderLine(entity.DbTableName);
                        },
                        EntityService = (prop, col) =>
                        {
                            if (new NullabilityInfoContext().Create(prop).WriteState is NullabilityState.Nullable)
                                col.IsNullable = true;
                            if (options.EnableUnderLine && !col.IsIgnore && !col.DbColumnName.Contains('_'))
                                col.DbColumnName = UtilMethods.ToUnderLine(col.DbColumnName);
                        }
                    }
                });

                tenantDb.DbMaintenance.CreateDatabase(tenant.Name);
                tenantDb.CodeFirst.InitTables(entityTypes);

                logger.LogInformation("租户 [{Name}] 数据库结构同步完成。", tenant.Name);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "租户 [{Name}] 数据库结构同步失败。", tenant.Name);
            }
        }

        logger.LogInformation("所有租户数据库结构同步完成。");
    }
}
