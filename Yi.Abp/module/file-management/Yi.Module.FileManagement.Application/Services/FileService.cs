using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.BlobStoring;
using Yi.Module.FileManagement.Application.BlobStoring;
using Yi.Framework.Ddd.Application;
using Yi.Framework.Authorization.Abstractions.Attributes;
using Yi.Framework.Authorization.Abstractions.Enums;
using Yi.Framework.OperationRecord.Abstractions.Attributes;
using Yi.Module.FileManagement.Application.Contracts.Dtos;
using Yi.Module.FileManagement.Application.Contracts.IServices;
using Yi.Module.FileManagement.Domain.Entities;
using Yi.Module.FileManagement.Domain.File;
using Yi.Framework.SqlSugarCore.Abstractions;

namespace Yi.Module.FileManagement.Application.Services;

/// <summary>
/// 文件应用服务
/// </summary>
[PermissionResource("system", "file")]
[OperLogEntity("文件")]
public class FileService : YiCrudAppService<FileAggregateRoot, FileGetListOutputDto, Guid, FileGetListInputVo>,
    IFileService
{
    private readonly ISqlSugarRepository<FileAggregateRoot, Guid> _repository;
    private readonly FileManager _fileManager;
    private readonly IBlobContainer<FileManagementContainer> _blobContainer;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly FileStorageOptionsResolver _optionsResolver;

    public FileService(
        ISqlSugarRepository<FileAggregateRoot, Guid> repository,
        FileManager fileManager,
        IBlobContainer<FileManagementContainer> blobContainer,
        IHttpContextAccessor httpContextAccessor,
        FileStorageOptionsResolver optionsResolver)
        : base(repository) =>
        (_repository, _fileManager, _blobContainer, _httpContextAccessor, _optionsResolver) =
        (repository, fileManager, blobContainer, httpContextAccessor, optionsResolver);

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
    [HttpGet("file/get/{id}")]
    [PermissionAction(PermissionActionEnum.Query)]
    public new async Task<FileStreamResult> GetAsync(Guid id)
    {
        var fileObject = await _fileManager.GetAsync(id);
        var stream = await _blobContainer.GetAsync(fileObject.StorageKey);
        return new FileStreamResult(stream, fileObject.ContentType);
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
    [PermissionAction(PermissionActionEnum.Add)]
    public async Task<List<Guid>> BatchUploadAsync(List<IFormFile> files)
    {
        var ids = new List<Guid>();
        foreach (var formFile in files)
        {
            using var memoryStream = new MemoryStream();
            await formFile.CopyToAsync(memoryStream);
            var fileBytes = memoryStream.ToArray();
            var id = GuidGenerator.Create();
            var dto = await _fileManager.CreateAsync(
                id,
                formFile.FileName,
                formFile.Length,
                formFile.ContentType,
                fileBytes,
                _optionsResolver.CreateStorageKey(id),
                _optionsResolver.ResolveProvider(),
                overwrite: false);
            ids.Add(dto.Id);
        }
        return ids;
    }

    /// <summary>
    /// 上传单个文件，返回文件访问链接
    /// </summary>
    [PermissionAction(PermissionActionEnum.Add)]
    public async Task<string> UploadAsync(IFormFile file)
    {
        using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream);
        var fileBytes = memoryStream.ToArray();
        var id = GuidGenerator.Create();
        var dto = await _fileManager.CreateAsync(
            id,
            file.FileName,
            file.Length,
            file.ContentType,
            fileBytes,
            _optionsResolver.CreateStorageKey(id),
            _optionsResolver.ResolveProvider(),
            overwrite: false);

        var request = _httpContextAccessor.HttpContext?.Request;
        var baseUrl = $"{request?.Scheme}://{request?.Host.Value}";
        return $"{baseUrl}/api/file/get/{dto.Id}";
    }

    /// <summary>
    /// 下载文件
    /// </summary>
    [PermissionAction(PermissionActionEnum.Query)]
    public async Task<FileStreamResult> DownloadAsync(Guid id)
    {
        var fileObject = await _fileManager.GetAsync(id);
        var stream = await _blobContainer.GetAsync(fileObject.StorageKey);
        return new FileStreamResult(stream, fileObject.ContentType)
        {
            FileDownloadName = fileObject.FileName
        };
    }

}

