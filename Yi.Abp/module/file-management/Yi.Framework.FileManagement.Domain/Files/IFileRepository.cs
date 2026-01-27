using Yi.Framework.SqlSugarCore.Abstractions;

namespace Yi.Framework.FileManagement.Files;

public interface IFileRepository : ISqlSugarRepository<FileAggregateRoot, Guid>
{
    Task<List<FileAggregateRoot>> GetListAsync(string? fileName, DateTime? startDateTime = null, DateTime? endDateTime = null, int maxResultCount = 10, int skipCount = 0);

    Task<long> GetCountAsync(string? fileName, DateTime? startDateTime = null, DateTime? endDateTime = null);

    Task<FileAggregateRoot?> FindAsync(string fileName);
}
