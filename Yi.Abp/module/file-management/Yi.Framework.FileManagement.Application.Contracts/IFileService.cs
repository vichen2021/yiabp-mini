using Microsoft.AspNetCore.Http;
using Volo.Abp.Application.Services;
using Volo.Abp.Content;
using Yi.Framework.Ddd.Application.Contracts;
using Yi.Framework.FileManagement.Application.Contracts.Dtos.File;

namespace Yi.Framework.FileManagement.Application.Contracts;

/// <summary>
/// 文件应用服务接口
/// </summary>
public interface IFileService : IYiCrudAppService<FileGetListOutputDto, Guid, FileGetListInputVo>
{
    /// <summary>
    /// 上传文件，返回创建的文件 id 列表（与入参 files 顺序一致）
    /// </summary>
    Task<List<Guid>> UploadAsync(List<IFormFile> files);

    /// <summary>
    /// 下载文件
    /// </summary>
    Task<IRemoteStreamContent> DownloadAsync(Guid id);
}
