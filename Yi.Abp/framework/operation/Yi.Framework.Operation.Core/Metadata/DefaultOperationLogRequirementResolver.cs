using System.Reflection;
using Microsoft.AspNetCore.Mvc.Controllers;
using Yi.Framework.Operation.Abstractions.Attributes;
using Yi.Framework.Operation.Abstractions.Enums;
using Yi.Framework.Operation.Abstractions.Metadata;

namespace Yi.Framework.Operation.Core.Metadata
{
    /// <summary>
    /// 默认操作日志要求解析器
    /// 仅解析日志决策所需信息，不涉及权限
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

        private OperationLogRequirement ResolveLog(ActionIdentity identity, MethodInfo methodInfo)
        {
            var requirement = new OperationLogRequirement();

            // 1. 检查 [IgnoreOperLog]（方法级 + 类级）
            if (HasIgnoreOperLog(methodInfo, identity.ServiceType))
            {
                requirement.Ignore = true;
                requirement.ShouldLog = false;
                return requirement;
            }

            // 2. 检查显式 [OperLog]
            var operLogAttr = methodInfo.GetCustomAttribute<OperLogAttribute>();
            if (operLogAttr != null)
            {
                requirement.OperType = operLogAttr.OperType;
                requirement.Title = operLogAttr.Title;
                requirement.SaveRequestData = operLogAttr.IsSaveRequestData;
                requirement.SaveResponseData = operLogAttr.IsSaveResponseData;
                requirement.ShouldLog = true;
                requirement.IsWriteOperation = IsWriteOperation(operLogAttr.OperType);
                return requirement;
            }

            // 3. 根据 CrudAction 推断日志信息
            if (!string.IsNullOrEmpty(identity.CrudAction))
            {
                requirement.OperType = InferOperType(identity.CrudAction);
                requirement.IsWriteOperation = IsWriteOperation(identity.CrudAction);

                // 4. 推断标题（需要 OperLogEntity）
                var operLogEntityAttr = identity.ServiceType.GetCustomAttribute<OperLogEntityAttribute>();
                if (operLogEntityAttr != null && requirement.OperType != null)
                {
                    requirement.Title = InferLogTitle(operLogEntityAttr.DisplayName, requirement.OperType.Value);
                    requirement.ShouldLog = requirement.IsWriteOperation; // 默认只记录写操作
                }
            }

            return requirement;
        }

        /// <summary>
        /// 检查是否有 [IgnoreOperLog] 特性
        /// </summary>
        private bool HasIgnoreOperLog(MethodInfo methodInfo, Type serviceType)
        {
            // 方法级优先
            if (methodInfo.GetCustomAttribute<IgnoreOperLogAttribute>() != null)
            {
                return true;
            }

            // 类级
            if (serviceType.GetCustomAttribute<IgnoreOperLogAttribute>() != null)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// 推断操作类型（根据 CrudAction）
        /// </summary>
        private OperEnum? InferOperType(string crudAction)
        {
            return crudAction switch
            {
                "add" => OperEnum.Insert,
                "edit" => OperEnum.Update,
                "remove" => OperEnum.Delete,
                "export" => OperEnum.Export,
                "import" => OperEnum.Import,
                "query" => null, // 查询不自动记录
                _ => null
            };
        }

        /// <summary>
        /// 判断是否写操作（根据 CrudAction）
        /// </summary>
        private bool IsWriteOperation(string crudAction)
        {
            return crudAction is "add" or "edit" or "remove" or "import" or "export";
        }

        /// <summary>
        /// 判断是否写操作（根据 OperType）
        /// </summary>
        private bool IsWriteOperation(OperEnum operType)
        {
            return operType is OperEnum.Insert or OperEnum.Update or OperEnum.Delete
                or OperEnum.Import or OperEnum.Export or OperEnum.Clear;
        }

        /// <summary>
        /// 推断日志标题
        /// </summary>
        private string? InferLogTitle(string entityDisplayName, OperEnum operType)
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

            if (actionDesc == null) return null;

            return $"{actionDesc}{entityDisplayName}";
        }
    }
}