using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;
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
    /// 压缩图片（使用 ImageSharp 直接压缩，性能更好）
    /// </summary>
    private async Task<byte[]> CompressImageAsync(FileAggregateRoot file, byte[] originalContent)
    {
        // 小于 100KB 的图片不压缩，避免浪费时间
        const int minSizeForCompression = 100 * 1024; // 100KB
        if (originalContent.Length < minSizeForCompression)
        {
            Logger.LogInformation("图片大小小于100KB，跳过压缩,文件id：{FileId}, 大小：{Size}字节",
                file.Id, originalContent.Length);
            return originalContent;
        }

        try
        {
            // 使用 Task.Run 将 CPU 密集型压缩操作放到后台线程
            var compressTask = Task.Run(() =>
            {
                using var inputStream = new MemoryStream(originalContent);
                using var image = Image.Load(inputStream);
                using var outputStream = new MemoryStream();

                // 根据文件类型选择压缩格式
                var mimeType = file.GetMimeMapping().ToLowerInvariant();

                if (mimeType.Contains("jpeg") || mimeType.Contains("jpg"))
                {
                    // JPEG 压缩，质量 75（平衡质量和大小）
                    image.SaveAsJpeg(outputStream, new JpegEncoder { Quality = 75 });
                }
                else if (mimeType.Contains("png"))
                {
                    // PNG 压缩，使用最佳压缩级别
                    image.SaveAsPng(outputStream, new PngEncoder
                    {
                        CompressionLevel = PngCompressionLevel.BestCompression
                    });
                }
                else if (mimeType.Contains("webp"))
                {
                    // WebP 压缩，质量 75
                    image.SaveAsWebp(outputStream, new WebpEncoder { Quality = 75 });
                }
                else
                {
                    // 不支持的格式，返回 null
                    return null;
                }

                return outputStream.ToArray();
            });

            // 设置 5 秒超时
            var completedTask = await Task.WhenAny(compressTask, Task.Delay(5000));

            if (completedTask == compressTask)
            {
                var compressedBytes = await compressTask;

                if (compressedBytes != null && compressedBytes.Length < originalContent.Length)
                {
                    Logger.LogInformation("图片压缩成功,文件id：{FileId}, 原始大小：{OriginalSize}字节, 压缩后大小：{CompressedSize}字节, 压缩率：{Ratio:P2}",
                        file.Id, originalContent.Length, compressedBytes.Length,
                        1.0 - (double)compressedBytes.Length / originalContent.Length);

                    return compressedBytes;
                }
                else
                {
                    Logger.LogInformation("压缩后图片更大，使用原始图片,文件id：{FileId}", file.Id);
                }
            }
            else
            {
                Logger.LogWarning("图片压缩超时（5秒），使用原始图片,文件id：{FileId}, 大小：{Size}字节",
                    file.Id, originalContent.Length);
            }
        }
        catch (UnknownImageFormatException exception)
        {
            Logger.LogInformation(exception, "不支持的图片格式,文件id：{FileId}", file.Id);
        }
        catch (Exception exception)
        {
            Logger.LogError(exception, "图片压缩异常,文件id：{FileId}", file.Id);
        }

        // 压缩失败或超时，返回原始内容
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
