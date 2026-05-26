using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Volo.Abp;
using Yi.Abp.DbMigrator;

Console.WriteLine("Yi DbMigrator 启动...");

var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json")
    .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Production"}.json", optional: true)
    .AddEnvironmentVariables()
    .Build();

using var application = await AbpApplicationFactory.CreateAsync<DbMigratorModule>(options =>
{
    options.UseAutofac();
    options.Services.ReplaceConfiguration(configuration);
    options.Services.AddLogging(b =>
    {
        b.AddConsole();
        b.SetMinimumLevel(LogLevel.Information);
    });
});

await application.InitializeAsync();

Console.WriteLine("Yi DbMigrator 完成：CodeFirst + 种子数据 + 租户数据库结构同步已执行。");

await application.ShutdownAsync();
