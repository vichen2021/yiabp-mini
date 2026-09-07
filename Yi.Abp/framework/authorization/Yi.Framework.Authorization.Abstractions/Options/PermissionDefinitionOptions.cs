using System.Reflection;

namespace Yi.Framework.Authorization.Abstractions.Options;

/// <summary>
/// Yi 权限定义扫描配置
/// </summary>
public class PermissionDefinitionOptions
{
    /// <summary>
    /// 显式登记的应用服务程序集
    /// </summary>
    public HashSet<Assembly> ApplicationAssemblies { get; } = new();

    /// <summary>
    /// 登记需要解析权限声明的应用服务程序集
    /// </summary>
    /// <param name="assembly">应用服务程序集</param>
    public void AddApplicationAssembly(Assembly assembly)
    {
        ApplicationAssemblies.Add(assembly);
    }
}
