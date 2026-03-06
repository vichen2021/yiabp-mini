using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace Yi.Framework.TenantManagement.Application.Contracts.Dtos
{
    public class TenantGetOutputDto : EntityDto<Guid>
    {
        public string Name { get; set; }
        public int EntityVersion { get; set; }
        public string TenantConnectionString { get; set; }
        public SqlSugar.DbType DbType { get; set; }
        public bool State { get; set; }
        public DateTime CreationTime { get; set; }

        public string? ContactUserName { get; set; }
        public string? ContactPhone { get; set; }
        public DateTime? ExpireTime { get; set; }
        public int AccountCount { get; set; }
        public string? Domain { get; set; }
        public string? Address { get; set; }
        public string? LicenseNumber { get; set; }
        public string? Intro { get; set; }
        public string? Remark { get; set; }
    }
}

