using System.Reflection;

namespace Yi.Framework.Authorization.Core.Metadata;

/// <summary>
/// 权限声明反射辅助类
/// </summary>
internal static class PermissionReflectionHelper
{
    /// <summary>
    /// 从实现方法、基类方法或接口方法读取继承权限特性
    /// </summary>
    /// <typeparam name="TAttribute">权限特性类型</typeparam>
    /// <param name="methodInfo">当前调用方法</param>
    /// <param name="serviceType">服务实现类型</param>
    /// <returns>匹配到的权限特性</returns>
    public static TAttribute? GetMethodAttribute<TAttribute>(MethodInfo methodInfo, Type serviceType)
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
            InterfaceMapping interfaceMap;
            try
            {
                interfaceMap = serviceType.GetInterfaceMap(interfaceType);
            }
            catch (ArgumentException)
            {
                continue;
            }

            for (var index = 0; index < interfaceMap.TargetMethods.Length; index++)
            {
                if (!MethodsMatch(interfaceMap.TargetMethods[index], implementationMethod ?? methodInfo))
                {
                    continue;
                }

                attribute = interfaceMap.InterfaceMethods[index].GetCustomAttribute<TAttribute>(inherit: true);
                if (attribute != null)
                {
                    return attribute;
                }
            }
        }

        return null;
    }

    /// <summary>
    /// 判断两个方法是否表达同一个调用签名
    /// </summary>
    private static bool MethodsMatch(MethodInfo left, MethodInfo right)
    {
        if (!string.Equals(left.Name, right.Name, StringComparison.Ordinal))
        {
            return false;
        }

        var leftParameters = left.GetParameters();
        var rightParameters = right.GetParameters();
        return leftParameters.Length == rightParameters.Length &&
               leftParameters.Select(parameter => parameter.ParameterType)
                   .SequenceEqual(rightParameters.Select(parameter => parameter.ParameterType));
    }
}
