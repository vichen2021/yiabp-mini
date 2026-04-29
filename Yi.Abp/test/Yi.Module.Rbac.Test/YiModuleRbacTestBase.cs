using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Volo.Abp;
using Yi.Module.Rbac.Domain.Repositories;
using Yi.Module.Rbac.SqlSugarCore.Repositories;

namespace Yi.Module.Rbac.Test
{
    public class YiModuleRbacTestBase : AbpTestBaseWithServiceProvider
    {
        public ILogger Logger { get; private set; }
        protected IServiceScope TestServiceScope { get; }
        public YiModuleRbacTestBase()
        {
            //在启动之前，清除sqlite全库，由于非常危险，建议使用sqlite
            //Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
            //var dbPath = "yi-rbac-test.db";
            //if (File.Exists(dbPath))
            //{
            //    File.Delete(dbPath);
            //}
            IHost host = Host.CreateDefaultBuilder()
               .UseAutofac()
               .ConfigureServices((host, service) =>
               {
                   ConfigureServices(host, service);
                   service.AddLogging(builder => builder.ClearProviders().AddConsole().AddDebug());
                   /*application= */
                   service.AddApplicationAsync<YiModuleRbacTestModule>().Wait();
               })
               .ConfigureAppConfiguration(ConfigureAppConfiguration)
               .Build();

            ServiceProvider = host.Services;
            TestServiceScope = ServiceProvider.CreateScope();
            Logger = (ILogger)ServiceProvider.GetRequiredService(typeof(ILogger<>).MakeGenericType(GetType()));

            host.InitializeAsync().Wait();
        }


        public virtual void ConfigureServices(HostBuilderContext host, IServiceCollection service)
        {
        }
        protected virtual void ConfigureAppConfiguration(IConfigurationBuilder configurationBuilder)
        {
            configurationBuilder.AddJsonFile("appsettings.json");
            //configurationBuilder.AddJsonFile("appsettings.Development.json");

        }
    }
}
