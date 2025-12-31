using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yi.Framework.Rbac.Domain.Shared.Enums
{
    public enum NoticeTypeEnum
    {
        [Description("通知")]
        Notice = 0,
        [Description("公告")]
        Announcement = 1
    }
}
