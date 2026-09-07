using System.Reflection;
using Yi.Framework.Auditing.Abstractions.Attributes;
using Yi.Framework.Auditing.Abstractions.Enums;

namespace Yi.Module.AuditLogging.Domain;

/// <summary>
/// 在 Action 执行阶段解析业务操作审计元数据
/// </summary>
internal static class OperationMetadataResolver
{
    /// <summary>
    /// 从当前服务方法、基类方法和接口方法解析操作元数据
    /// </summary>
    public static OperationMetadata? Resolve(Type? serviceType, MethodInfo methodInfo)
    {
        if (serviceType == null || HasAttribute<IgnoreOperLogAttribute>(serviceType, methodInfo))
        {
            return null;
        }

        var attribute = GetMethodAttribute<OperLogAttribute>(serviceType, methodInfo);
        if (attribute == null)
        {
            return null;
        }

        var title = attribute.Title;
        if (string.IsNullOrWhiteSpace(title))
        {
            var entityAttribute = serviceType.GetCustomAttribute<OperLogEntityAttribute>(inherit: true);
            title = entityAttribute == null ? string.Empty : InferTitle(entityAttribute.DisplayName, attribute.OperType);
        }

        return string.IsNullOrWhiteSpace(title)
            ? null
            : new OperationMetadata(title, attribute.OperType, attribute.IsSaveRequestData, true);
    }

    /// <summary>
    /// 判断服务或方法是否声明指定审计特性
    /// </summary>
    private static bool HasAttribute<TAttribute>(Type serviceType, MethodInfo methodInfo)
        where TAttribute : Attribute
    {
        return serviceType.IsDefined(typeof(TAttribute), inherit: true) ||
               GetMethodAttribute<TAttribute>(serviceType, methodInfo) != null;
    }

    /// <summary>
    /// 从实现方法、基类方法或接口方法读取审计特性
    /// </summary>
    private static TAttribute? GetMethodAttribute<TAttribute>(Type serviceType, MethodInfo methodInfo)
        where TAttribute : Attribute
    {
        var attribute = methodInfo.GetCustomAttribute<TAttribute>(inherit: true);
        if (attribute != null)
        {
            return attribute;
        }

        var parameterTypes = methodInfo.GetParameters().Select(parameter => parameter.ParameterType).ToArray();
        var implementationMethod = serviceType.GetMethod(
            methodInfo.Name,
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
            binder: null,
            types: parameterTypes,
            modifiers: null);

        attribute = implementationMethod?.GetCustomAttribute<TAttribute>(inherit: true);
        if (attribute != null)
        {
            return attribute;
        }

        foreach (var interfaceType in serviceType.GetInterfaces())
        {
            var interfaceMethod = interfaceType.GetMethod(methodInfo.Name, parameterTypes);
            attribute = interfaceMethod?.GetCustomAttribute<TAttribute>(inherit: true);
            if (attribute != null)
            {
                return attribute;
            }
        }

        return null;
    }

    /// <summary>
    /// 根据标准操作类型和实体显示名生成业务标题
    /// </summary>
    private static string InferTitle(string entityDisplayName, OperEnum operationType)
    {
        var verb = operationType switch
        {
            OperEnum.Insert => "添加",
            OperEnum.Update => "更新",
            OperEnum.Delete => "删除",
            OperEnum.Export => "导出",
            OperEnum.Import => "导入",
            OperEnum.Clear => "清空",
            _ => string.Empty
        };

        return string.IsNullOrEmpty(verb) ? string.Empty : $"{verb}{entityDisplayName}";
    }
}
