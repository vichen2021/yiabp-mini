using Microsoft.Extensions.Logging;
using SqlSugar;
using Volo.Abp;
using Volo.Abp.BlobStoring;
using Volo.Abp.Domain.Services;
using Volo.Abp.Imaging;
using Volo.Abp.MultiTenancy;
using Yi.Framework.Core.Enums;
using Yi.Framework.Core.Helper;
using Yi.Framework.Rbac.Domain.Shared.Consts;
using Yi.Framework.Rbac.Domain.Shared.File;
using Yi.Framework.Rbac.Domain.Entities;
using Yi.Framework.SqlSugarCore.Abstractions;

namespace Yi.Framework.Rbac.Domain.File;

public class FileManager : DomainService
{
    private readonly IBlobContainer<FileManagementContainer> _blobContainer;
    private readonly ISqlSugarRepository<FileAggregateRoot, Guid> _repository;
    private readonly ICurrentTenant _currentTenant;
    private readonly IImageCompressor _imageCompressor;

    public FileManager(
        IBlobContainer<FileManagementContainer> blobContainer,
        ISqlSugarRepository<FileAggregateRoot, Guid> repository,
        ICurrentTenant currentTenant,
        IImageCompressor imageCompressor)
    {
        _blobContainer = blobContainer;
        _repository = repository;
        _currentTenant = currentTenant;
        _imageCompressor = imageCompressor;
    }

    /// <summary>
    /// 获取文件元数据
    /// </summary>
    public async Task<FileDto> GetAsync(Guid id)
    {
        var entity = await _repository.FindAsync(x => x.Id == id);
        if (entity == null)
            throw new UserFriendlyException(FileManagementConsts.FileNotFound);
        return EntityMapToDto(entity);
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
        // 如果文件类型是图片，先进行压缩
        var fileType = MimeHelper.GetFileType(fileName);
        byte[] finalContent = content;
        long finalFileSize = fileSize;
        
        if (FileTypeEnum.image == fileType)
        {
            // 创建临时实体用于压缩
            var tempEntity = new FileAggregateRoot(id, fileName, fileSize, contentType, _currentTenant?.Id);
            finalContent = await CompressImageAsync(tempEntity, content);
            finalFileSize = finalContent.Length;
        }

        var entity = (await _repository._DbQueryable.Where(x => x.FileName == fileName).Take(1).ToListAsync()).FirstOrDefault();
        if (entity != null)
        {
            if (!overwrite)
                throw new UserFriendlyException(FileManagementConsts.FileAlreadyExist);
            entity.Update(finalFileSize, contentType, fileName);
            await _repository.UpdateAsync(entity);
        }
        else
        {
            entity = new FileAggregateRoot(id, fileName, finalFileSize, contentType, _currentTenant?.Id);
            await _repository.InsertAsync(entity);
        }

        await _blobContainer.SaveAsync(entity.Id.ToString(), finalContent, overwrite);
        return EntityMapToDto(entity);
    }

    /// <summary>
    /// 压缩图片
    /// </summary>
    private async Task<byte[]> CompressImageAsync(FileAggregateRoot file, byte[] originalContent)
    {
        try
        {
            using var fileStream = new MemoryStream(originalContent);
            // 压缩图片
            var compressResult = await _imageCompressor.CompressAsync(fileStream, file.GetMimeMapping());
            
            if (compressResult.State == ImageProcessState.Done)
            {
                // 读取压缩后的流
                using var compressedStream = compressResult.Result;
                compressedStream.Position = 0;
                var compressedBytes = new byte[compressedStream.Length];
                await compressedStream.ReadAsync(compressedBytes, 0, compressedBytes.Length);
                
                Logger.LogInformation("图片压缩成功,文件id：{FileId}, 原始大小：{OriginalSize}字节, 压缩后大小：{CompressedSize}字节", 
                    file.Id, originalContent.Length, compressedBytes.Length);
                
                return compressedBytes;
            }
            else if (compressResult.State == ImageProcessState.Canceled)
            {
                Logger.LogInformation("当前图片无法再进行压缩,文件id：{FileId}", file.Id);
            }
            else
            {
                Logger.LogInformation("当前图片不支持压缩,文件id：{FileId}", file.Id);
            }
        }
        catch (NotSupportedException exception)
        {
            Logger.LogInformation(exception, "图片压缩失败,文件id：{FileId}", file.Id);
        }
        catch (Exception exception)
        {
            Logger.LogError(exception, "图片压缩异常,文件id：{FileId}", file.Id);
        }
        
        // 压缩失败，返回原始内容
        return originalContent;
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

    private static FileDto EntityMapToDto(FileAggregateRoot entity)
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
