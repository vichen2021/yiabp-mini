using Volo.Abp;
using Volo.Abp.BlobStoring;
using Volo.Abp.Domain.Services;
using Volo.Abp.MultiTenancy;
using Yi.Framework.FileManagement.Domain.Shared.Consts;

namespace Yi.Framework.FileManagement.Files;

public class FileManager : DomainService
{
    private readonly IBlobContainer<YiFileManagementContainer> _blobContainer;
    private readonly IFileRepository _fileRepository;
    private readonly ICurrentTenant _currentTenant;

    public FileManager(
        IBlobContainer<YiFileManagementContainer> blobContainer,
        IFileRepository fileRepository,
        ICurrentTenant currentTenant)
    {
        _blobContainer = blobContainer;
        _fileRepository = fileRepository;
        _currentTenant = currentTenant;
    }

    public async Task<List<FileDto>> GetListAsync(string? fileName, DateTime? startDateTime = null, DateTime? endDateTime = null, int maxResultCount = 10, int skipCount = 0)
    {
        var list = await _fileRepository.GetListAsync(fileName, startDateTime, endDateTime, maxResultCount, skipCount);
        return list.Select(x => new FileDto
        {
            Id = x.Id,
            CreationTime = x.CreationTime,
            FileSize = x.FileSize,
            ContentType = x.ContentType,
            FileName = x.FileName
        }).ToList();
    }

    public async Task<long> GetCountAsync(string? fileName, DateTime? startDateTime = null, DateTime? endDateTime = null)
    {
        return await _fileRepository.GetCountAsync(fileName, startDateTime, endDateTime);
    }

    /// <summary>
    /// 获取文件元数据
    /// </summary>
    public async Task<FileDto> GetAsync(Guid id)
    {
        var entity = await _fileRepository.FindAsync(x => x.Id == id);
        if (entity == null)
            throw new UserFriendlyException(FileManagementConsts.FileNotFound);
        return ToDto(entity);
    }

    /// <summary>
    /// 创建文件（元数据 + 二进制存储），返回实际落库的 dto（含 Id）
    /// </summary>
    public virtual async Task<FileDto> CreateAsync(
        Guid id,
        string fileName,
        long fileSize,
        string contentType,
        byte[] content,
        bool overwrite = false
    )
    {
        var entity = await _fileRepository.FindAsync(fileName);
        if (entity != null)
        {
            if (!overwrite)
                throw new UserFriendlyException(FileManagementConsts.FileAlreadyExist);
            entity.Update(fileSize, contentType, fileName);
            await _fileRepository.UpdateAsync(entity);
        }
        else
        {
            entity = new FileAggregateRoot(id, fileName, fileSize, contentType, _currentTenant?.Id);
            await _fileRepository.InsertAsync(entity);
        }

        await _blobContainer.SaveAsync(entity.Id.ToString(), content, overwrite);
        return ToDto(entity);
    }

    /// <summary>
    /// 删除文件（元数据 + 二进制）
    /// </summary>
    public virtual async Task DeleteAsync(Guid id)
    {
        var entity = await _fileRepository.FindAsync(x => x.Id == id);
        if (entity == null)
            throw new UserFriendlyException(FileManagementConsts.FileNotFound);
        await _fileRepository.DeleteAsync(entity);
        await _blobContainer.DeleteAsync(id.ToString());
    }

    private static FileDto ToDto(FileAggregateRoot entity)
    {
        return new FileDto
        {
            Id = entity.Id,
            CreationTime = entity.CreationTime,
            FileSize = entity.FileSize,
            ContentType = entity.ContentType,
            FileName = entity.FileName
        };
    }
}
