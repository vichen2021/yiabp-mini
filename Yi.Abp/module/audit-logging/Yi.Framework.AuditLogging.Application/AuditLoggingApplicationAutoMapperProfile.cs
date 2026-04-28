using AutoMapper;
using Yi.Framework.AuditLogging.Application.Contracts.Dtos.OperationLog;
using Yi.Framework.AuditLogging.Domain.Entities;

namespace Yi.Framework.AuditLogging.Application;

public class AuditLoggingApplicationAutoMapperProfile : Profile
{
    public AuditLoggingApplicationAutoMapperProfile()
    {
        CreateMap<OperationLogEntity, OperationLogGetOutputDto>();
        CreateMap<OperationLogEntity, OperationLogGetListOutputDto>();
    }
}