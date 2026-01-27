using Microsoft.Extensions.Logging;
using Volo.Abp;
using Volo.Abp.Logging;

namespace Yi.Framework.FileManagement.Files;

public class DomainFileManagementException : BusinessException
{
    public DomainFileManagementException(string? code = null, string? message = null, string? details = null, Exception? innerException = null,
        LogLevel logLevel = LogLevel.Warning) : base(code, message, details, innerException, logLevel)
    {
    }
}
