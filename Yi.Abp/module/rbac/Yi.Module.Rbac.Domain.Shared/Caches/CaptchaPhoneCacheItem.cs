using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yi.Module.Rbac.Domain.Shared.Enums;

namespace Yi.Module.Rbac.Domain.Shared.Caches
{
    public class CaptchaPhoneCacheItem
    {
        public CaptchaPhoneCacheItem(string code) { Code = code; }
        public string Code { get; set; }
    }

    public class CaptchaPhoneCacheKey
    {
        public CaptchaPhoneCacheKey(ValidationPhoneTypeEnum validationPhoneType,string phone) { Phone = phone;
            ValidationPhoneType = validationPhoneType;
        }
        public ValidationPhoneTypeEnum ValidationPhoneType { get; set; }
        public string Phone { get; set; }

        public override string ToString()
        {
            return $"Phone:{ValidationPhoneType.ToString()}:{Phone}";
        }
    }
}
