using Volo.Abp.DependencyInjection;
using Yi.Framework.SqlSugarCore;

namespace Yi.Module.FileManagement.SqlSugarCore;

public class YiFileManagementDbContext : SqlSugarDbContext
{
    public YiFileManagementDbContext(IAbpLazyServiceProvider lazyServiceProvider) : base(lazyServiceProvider)
    {
    }
}
