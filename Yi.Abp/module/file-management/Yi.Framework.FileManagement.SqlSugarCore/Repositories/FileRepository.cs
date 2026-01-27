using SqlSugar;
using Yi.Framework.FileManagement.Files;
using Yi.Framework.SqlSugarCore.Abstractions;
using Yi.Framework.SqlSugarCore.Repositories;

namespace Yi.Framework.FileManagement.SqlSugarCore.Repositories;

/// <summary>
/// 文件仓储 SqlSugar 实现
/// </summary>
public class FileRepository : SqlSugarRepository<FileAggregateRoot, Guid>, IFileRepository
{
    public FileRepository(ISugarDbContextProvider<ISqlSugarDbContext> sugarDbContextProvider) : base(sugarDbContextProvider)
    {
    }

    public async Task<List<FileAggregateRoot>> GetListAsync(string? fileName, DateTime? startDateTime = null, DateTime? endDateTime = null, int maxResultCount = 10, int skipCount = 0)
    {
        var query = _DbQueryable
            .WhereIF(!string.IsNullOrWhiteSpace(fileName), x => x.FileName != null && x.FileName.Contains(fileName!))
            .WhereIF(startDateTime.HasValue, x => x.CreationTime >= startDateTime!.Value)
            .WhereIF(endDateTime.HasValue, x => x.CreationTime <= endDateTime!.Value)
            .OrderByDescending(x => x.CreationTime);

        return await query.Skip(skipCount).Take(maxResultCount).ToListAsync();
    }

    public async Task<long> GetCountAsync(string? fileName, DateTime? startDateTime = null, DateTime? endDateTime = null)
    {
        return await _DbQueryable
            .WhereIF(!string.IsNullOrWhiteSpace(fileName), x => x.FileName != null && x.FileName.Contains(fileName!))
            .WhereIF(startDateTime.HasValue, x => x.CreationTime >= startDateTime!.Value)
            .WhereIF(endDateTime.HasValue, x => x.CreationTime <= endDateTime!.Value)
            .CountAsync();
    }

    public async Task<FileAggregateRoot?> FindAsync(string fileName)
    {
        var list = await _DbQueryable.Where(x => x.FileName == fileName).Take(1).ToListAsync();
        return list.FirstOrDefault();
    }
}
