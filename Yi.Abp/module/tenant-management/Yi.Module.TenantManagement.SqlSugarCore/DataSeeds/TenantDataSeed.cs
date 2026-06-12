using Microsoft.Extensions.Options;
using SqlSugar;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Yi.Framework.SqlSugarCore.Abstractions;
using Yi.Module.TenantManagement.Domain.Entities;

namespace Yi.Module.TenantManagement.SqlSugarCore.DataSeeds;

public class TenantDataSeed : IDataSeedContributor, ITransientDependency
{
    public const string DefaultTenantName = "testTenant";

    private readonly DbConnOptions _dbConnOptions;
    private readonly ISqlSugarRepository<TenantAggregateRoot, Guid> _tenantRepository;
    private readonly ISqlSugarRepository<TenantPackageAggregateRoot, Guid> _tenantPackageRepository;

    public TenantDataSeed(
        ISqlSugarRepository<TenantAggregateRoot, Guid> tenantRepository,
        ISqlSugarRepository<TenantPackageAggregateRoot, Guid> tenantPackageRepository,
        IOptions<DbConnOptions> dbConnOptions)
    {
        _tenantRepository = tenantRepository;
        _tenantPackageRepository = tenantPackageRepository;
        _dbConnOptions = dbConnOptions.Value;
    }

    public async Task SeedAsync(DataSeedContext context)
    {
        if (context.TenantId is not null || !_dbConnOptions.EnabledSaasMultiTenancy)
        {
            return;
        }

        var defaultPackage = await _tenantPackageRepository.GetFirstAsync(x =>
            x.PackageName == TenantPackageDataSeed.DefaultPackageName);
        var tenant = await _tenantRepository.GetFirstAsync(x => x.Name == DefaultTenantName);
        if (tenant is null)
        {
            tenant = TenantAggregateRoot.Create(Guid.NewGuid(), DefaultTenantName);
            tenant.SetConnectionString(
                _dbConnOptions.DbType ?? DbType.Sqlite,
                BuildTenantConnectionString(DefaultTenantName));
            tenant.PackageId = defaultPackage?.Id;
            tenant.ContactUserName = "租户管理员";
            tenant.ContactPhone = "13800000000";
            tenant.AccountCount = -1;
            tenant.Remark = "默认租户";
            tenant.State = true;
            await _tenantRepository.InsertAsync(tenant);
            return;
        }

        if (defaultPackage is not null && tenant.PackageId != defaultPackage.Id)
        {
            tenant.PackageId = defaultPackage.Id;
            await _tenantRepository.UpdateAsync(tenant);
        }
    }

    private string BuildTenantConnectionString(string tenantName)
    {
        if (_dbConnOptions.DbType != DbType.Sqlite)
        {
            return _dbConnOptions.Url ?? string.Empty;
        }

        var hostConnectionString = _dbConnOptions.Url;
        if (string.IsNullOrWhiteSpace(hostConnectionString))
        {
            return $"DataSource=tenant-{tenantName}.db";
        }

        var parts = hostConnectionString.Split(';', StringSplitOptions.RemoveEmptyEntries);
        for (var i = 0; i < parts.Length; i++)
        {
            var keyValue = parts[i].Split('=', 2);
            if (keyValue.Length == 2 &&
                keyValue[0].Trim().Equals("DataSource", StringComparison.OrdinalIgnoreCase))
            {
                parts[i] = $"DataSource=tenant-{tenantName}.db";
                return string.Join(';', parts);
            }
        }

        return $"DataSource=tenant-{tenantName}.db";
    }
}
