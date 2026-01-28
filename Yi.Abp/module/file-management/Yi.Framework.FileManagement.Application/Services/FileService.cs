using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.BlobStoring;
using Yi.Framework.Ddd.Application;
using Yi.Framework.FileManagement.Application.Contracts;
using Yi.Framework.FileManagement.Application.Contracts.Dtos;
using Yi.Framework.FileManagement.Domain.File;
using Yi.Framework.FileManagement.Files;
using Yi.Framework.SqlSugarCore.Abstractions;

namespace Yi.Framework.FileManagement.Application.Services;

/// <summary>
/// 文件应用服务
/// </summary>
public class FileService : YiCrudAppService<FileAggregateRoot, FileGetListOutputDto, Guid, FileGetListInputVo>,
    IFileService
{
    private readonly ISqlSugarRepository<FileAggregateRoot, Guid> _repository;
    private readonly FileManager _fileManager;
    private readonly IBlobContainer<FileManagementContainer> _blobContainer;

    public FileService(
        ISqlSugarRepository<FileAggregateRoot, Guid> repository,
        FileManager fileManager,
        IBlobContainer<FileManagementContainer> blobContainer)
        : base(repository)
    {
        _repository = repository;
        _fileManager = fileManager;
        _blobContainer = blobContainer;
    }

    /// <summary>
    /// 多查
    /// </summary>
    public override async Task<PagedResultDto<FileGetListOutputDto>> GetListAsync(FileGetListInputVo input)
    {
        RefAsync<int> total = 0;

        var entities = await _repository._DbQueryable.WhereIF(!string.IsNullOrEmpty(input.FileName),
                x => x.FileName.Contains(input.FileName!))
            .WhereIF(input.StartTime.HasValue && input.EndTime.HasValue,
                x => x.CreationTime >= input.StartTime && x.CreationTime <= input.EndTime)
            .ToPageListAsync(input.SkipCount, input.MaxResultCount, total);
        return new PagedResultDto<FileGetListOutputDto>(total, await MapToGetListOutputDtosAsync(entities));
    }

    /// <summary>
    /// 单查
    /// </summary>
    public override async Task<FileGetListOutputDto> GetAsync(Guid id)
    {
        var dto = await _fileManager.GetAsync(id);
        return new FileGetListOutputDto
        {
            Id = dto.Id,
            FileSize = dto.FileSize,
            ContentType = dto.ContentType,
            FileName = dto.FileName,
            CreationTime = dto.CreationTime
        };
    }

    /// <summary>
    /// 删除（元数据 + 二进制）
    /// </summary>
    public override Task DeleteAsync(Guid id)
    {
        return _fileManager.DeleteAsync(id);
    }

    /// <summary>
    /// 创建占位，实际使用 Upload 上传
    /// </summary>
    public override Task<FileGetListOutputDto> CreateAsync(FileGetListOutputDto input)
    {
        throw new UserFriendlyException("请使用 Upload 接口上传文件");
    }

    /// <summary>
    /// 更新占位，文件不支持更新
    /// </summary>
    public override Task<FileGetListOutputDto> UpdateAsync(Guid id, FileGetListOutputDto input)
    {
        throw new UserFriendlyException("文件不支持更新");
    }

    /// <summary>
    /// 上传文件，返回落库后的文件 id 列表（与入参 files 顺序一致）
    /// </summary>
    public async Task<List<Guid>> UploadAsync(List<IFormFile> files)
    {
        var ids = new List<Guid>();
        foreach (var formFile in files)
        {
            using var memoryStream = new MemoryStream();
            await formFile.CopyToAsync(memoryStream);
            var fileBytes = memoryStream.ToArray();
            var dto = await _fileManager.CreateAsync(
                GuidGenerator.Create(),
                formFile.FileName,
                formFile.Length,
                formFile.ContentType,
                fileBytes,
                overwrite: true);
            ids.Add(dto.Id);
        }
        return ids;
    }

    /// <summary>
    /// 下载文件
    /// </summary>
    public async Task<FileStreamResult> DownloadAsync(Guid id)
    {
        var fileObject = await _fileManager.GetAsync(id);
        var stream = await _blobContainer.GetAsync(id.ToString());
        return new FileStreamResult(stream, fileObject.ContentType)
        {
            FileDownloadName = fileObject.FileName
        };
    }
}
