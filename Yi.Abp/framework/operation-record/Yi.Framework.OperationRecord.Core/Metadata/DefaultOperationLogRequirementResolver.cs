using System.Reflection;
using Microsoft.AspNetCore.Mvc.Controllers;
using Yi.Framework.ActionMetadata.Abstractions.Metadata;
using Yi.Framework.ActionMetadata.Core.Metadata;
using Yi.Framework.OperationRecord.Abstractions.Attributes;
using Yi.Framework.OperationRecord.Abstractions.Enums;
using Yi.Framework.OperationRecord.Abstractions.Metadata;

namespace Yi.Framework.OperationRecord.Core.Metadata
{
    /// <summary>
    /// 默认操作记录要求解析器。仅解析操作记录决策所需信息，不涉及权限。
    /// </summary>
    public class DefaultOperationLogRequirementResolver : IOperationLogRequirementResolver
    {
        private readonly IActionIdentityResolver _identityResolver;

        public DefaultOperationLogRequirementResolver(IActionIdentityResolver identityResolver)
        {
            _identityResolver = identityResolver;
        }

        public OperationLogRequirement Resolve(ControllerActionDescriptor descriptor)
        {
            var identity = _identityResolver.Resolve(descriptor);
            return ResolveLog(identity, descriptor.MethodInfo);
        }

        public OperationLogRequirement Resolve(Type serviceType, MethodInfo methodInfo)
        {
            var identity = _identityResolver.Resolve(serviceType, methodInfo);
            return ResolveLog(identity, methodInfo);
        }

        private static OperationLogRequirement ResolveLog(ActionIdentity identity, MethodInfo methodInfo)
        {
            var requirement = new OperationLogRequirement();

            if (HasIgnoreOperLog(methodInfo, identity.ServiceType))
            {
                requirement.Ignore = true;
                requirement.ShouldLog = false;
                requirement.Source = "IgnoreOperLog";
                return requirement;
            }

            var operLogAttr = ActionReflectionHelper.GetMethodAttribute<OperLogAttribute>(methodInfo, identity.ServiceType);
            if (operLogAttr == null)
            {
                return requirement;
            }

            requirement.OperType = operLogAttr.OperType;
            requirement.Title = ResolveTitle(identity.ServiceType, operLogAttr);
            requirement.SaveRequestData = operLogAttr.IsSaveRequestData;
            requirement.SaveResponseData = operLogAttr.IsSaveResponseData;
            requirement.ShouldLog = true;
            requirement.IsWriteOperation = IsWriteOperation(operLogAttr.OperType);
            requirement.Source = "OperLog";
            return requirement;
        }

        private static bool HasIgnoreOperLog(MethodInfo methodInfo, Type serviceType)
        {
            if (ActionReflectionHelper.GetMethodAttribute<IgnoreOperLogAttribute>(methodInfo, serviceType) != null)
            {
                return true;
            }

            return serviceType.GetCustomAttribute<IgnoreOperLogAttribute>() != null;
        }

        private static string ResolveTitle(Type serviceType, OperLogAttribute operLogAttr)
        {
            if (!string.IsNullOrWhiteSpace(operLogAttr.Title))
            {
                return operLogAttr.Title;
            }

            var entityAttr = serviceType.GetCustomAttribute<OperLogEntityAttribute>();
            if (entityAttr == null)
            {
                return string.Empty;
            }

            return InferLogTitle(entityAttr.DisplayName, operLogAttr.OperType) ?? string.Empty;
        }

        private static bool IsWriteOperation(OperEnum operType)
        {
            return operType is OperEnum.Insert or OperEnum.Update or OperEnum.Delete
                or OperEnum.Import or OperEnum.Export or OperEnum.Clear;
        }

        private static string? InferLogTitle(string entityDisplayName, OperEnum operType)
        {
            var actionDesc = operType switch
            {
                OperEnum.Insert => "添加",
                OperEnum.Update => "更新",
                OperEnum.Delete => "删除",
                OperEnum.Export => "导出",
                OperEnum.Import => "导入",
                OperEnum.Clear => "清空",
                _ => null
            };

            return actionDesc == null ? null : $"{actionDesc}{entityDisplayName}";
        }
    }
}
