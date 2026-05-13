using System.Reflection;

namespace Yi.Framework.ActionMetadata.Core.Metadata
{
    public static class ActionReflectionHelper
    {
        public static TAttribute? GetMethodAttribute<TAttribute>(MethodInfo methodInfo, Type serviceType)
            where TAttribute : Attribute
        {
            var attribute = methodInfo.GetCustomAttribute<TAttribute>();
            if (attribute != null)
            {
                return attribute;
            }

            var baseMethod = methodInfo.GetBaseDefinition();
            if (baseMethod != null && baseMethod != methodInfo)
            {
                attribute = baseMethod.GetCustomAttribute<TAttribute>();
                if (attribute != null)
                {
                    return attribute;
                }
            }

            foreach (var candidate in FindBaseMethods(serviceType, methodInfo))
            {
                attribute = candidate.GetCustomAttribute<TAttribute>();
                if (attribute != null)
                {
                    return attribute;
                }
            }

            foreach (var interfaceMethod in FindInterfaceMethods(serviceType, methodInfo))
            {
                attribute = interfaceMethod.GetCustomAttribute<TAttribute>();
                if (attribute != null)
                {
                    return attribute;
                }
            }

            return null;
        }

        public static IEnumerable<MethodInfo> FindBaseMethods(Type serviceType, MethodInfo methodInfo)
        {
            var current = serviceType.BaseType;
            while (current != null && current != typeof(object))
            {
                foreach (var candidate in current.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                {
                    if (IsSameSignature(candidate, methodInfo))
                    {
                        yield return candidate;
                    }
                }

                current = current.BaseType;
            }
        }

        public static IEnumerable<MethodInfo> FindInterfaceMethods(Type serviceType, MethodInfo methodInfo)
        {
            foreach (var interfaceType in serviceType.GetInterfaces())
            {
                var interfaceMethod = interfaceType.GetMethod(
                    methodInfo.Name,
                    methodInfo.GetParameters().Select(p => p.ParameterType).ToArray());
                if (interfaceMethod != null)
                {
                    yield return interfaceMethod;
                }
            }
        }

        public static bool IsSameSignature(MethodInfo candidate, MethodInfo methodInfo)
        {
            if (!string.Equals(candidate.Name, methodInfo.Name, StringComparison.Ordinal))
            {
                return false;
            }

            var candidateParameters = candidate.GetParameters();
            var methodParameters = methodInfo.GetParameters();
            if (candidateParameters.Length != methodParameters.Length)
            {
                return false;
            }

            for (var i = 0; i < candidateParameters.Length; i++)
            {
                var candidateType = candidateParameters[i].ParameterType;
                var methodType = methodParameters[i].ParameterType;
                if (candidateType == methodType || candidateType.IsGenericParameter)
                {
                    continue;
                }

                if (candidateType.ContainsGenericParameters && methodType.IsConstructedGenericType)
                {
                    continue;
                }

                return false;
            }

            return true;
        }
    }
}
