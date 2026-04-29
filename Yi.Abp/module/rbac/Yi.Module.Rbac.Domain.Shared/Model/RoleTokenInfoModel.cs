using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yi.Module.Rbac.Domain.Shared.Enums;

namespace Yi.Module.Rbac.Domain.Shared.Model
{
    public class RoleTokenInfoModel
    {
        public Guid Id { get; set; }
        public DataScopeEnum DataScope { get; set; }
    }
}
