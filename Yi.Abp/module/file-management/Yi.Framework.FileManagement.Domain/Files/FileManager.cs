using SqlSugar;
using Volo.Abp;
using Volo.Abp.BlobStoring;
using Volo.Abp.Domain.Services;
using Volo.Abp.MultiTenancy;
using Yi.Framework.FileManagement.Domain.Shared.Consts;
using Yi.Framework.SqlSugarCore.Abstractions;

namespace Yi.Framework.FileManagement.Files;

public class FileManager : DomainService
{
    private readonly IBlobContainer<FileManagementContainer> _blobContainer;
    private readonly ISqlSugarRepository<FileAggregateRoot, Guid> _repository;
    private readonly ICurrentTenant _currentTenant;

    public FileManager(
        IBlobContainer<FileManagementContainer> blobContainer,
        ISqlSugarRepository<FileAggregateRoot, Guid> repository,
        ICurrentTenant currentTenant)
    {
        _blobContainer = blobContainer;
        _repository = repository;
        _currentTenant = currentTenant;
    }

    /// <summary>
    /// 获取文件元数据
    /// </summary>
    public async Task<FileDto> GetAsync(Guid id)
    {
        var entity = await _repository.FindAsync(x => x.Id == id);
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
        var entity = (await _repository._DbQueryable.Where(x => x.FileName == fileName).Take(1).ToListAsync()).FirstOrDefault();
        if (entity != null)
        {
            if (!overwrite)
                throw new UserFriendlyException(FileManagementConsts.FileAlreadyExist);
            entity.Update(fileSize, contentType, fileName);
            await _repository.UpdateAsync(entity);
        }
        else
        {
            entity = new FileAggregateRoot(id, fileName, fileSize, contentType, _currentTenant?.Id);
            await _repository.InsertAsync(entity);
        }

        await _blobContainer.SaveAsync(entity.Id.ToString(), content, overwrite);
        return ToDto(entity);
    }

    /// <summary>
    /// 删除文件（元数据 + 二进制）
    /// </summary>
    public virtual async Task DeleteAsync(Guid id)
    {
        var entity = await _repository.FindAsync(x => x.Id == id);
        if (entity == null)
            throw new UserFriendlyException(FileManagementConsts.FileNotFound);
        await _repository.DeleteAsync(entity);
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
