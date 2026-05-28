namespace Yi.Module.FileManagement.Application.Contracts.FileUrl;

public interface IFileUrlResolver
{
    string? Resolve(Guid? fileId, string? storageKey = null);
}
