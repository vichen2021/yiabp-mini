using AutoMapper;
using Yi.Module.AuditLogging.Application.Contracts.Dtos.OperationLog;
using Yi.Module.AuditLogging.Domain.Entities;

namespace Yi.Module.AuditLogging.Application;

public class AuditLoggingApplicationAutoMapperProfile : Profile
{
    public AuditLoggingApplicationAutoMapperProfile()
    {
        CreateMap<OperationLogEntity, OperationLogGetOutputDto>();
        CreateMap<OperationLogEntity, OperationLogGetListOutputDto>();
    }
}