using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Shouldly;
using Yi.Framework.Authorization.Abstractions.Attributes;
using Yi.Module.Rbac.Application.Services;
using Xunit;

namespace Yi.Module.Rbac.Test.Authorization;

/// <summary>
/// 公共选择数据接口授权边界测试
/// </summary>
public class CommonLookupPermissionTest
{
    /// <summary>
    /// 验证部门选择接口跳过部门管理权限但不允许匿名访问
    /// </summary>
    [Fact]
    public void DeptSelectDataList_ShouldIgnoreResourcePermissionButRequireAuthentication()
    {
        var method = typeof(DeptService).GetMethod(nameof(DeptService.GetSelectDataListAsync));

        method.ShouldNotBeNull();
        method.GetCustomAttribute<IgnorePermissionAttribute>().ShouldNotBeNull();
        method.GetCustomAttribute<AllowAnonymousAttribute>().ShouldBeNull();
    }

    /// <summary>
    /// 验证按类型读取字典接口跳过字典管理权限但不允许匿名访问
    /// </summary>
    [Fact]
    public void DictionaryByType_ShouldIgnoreResourcePermissionButRequireAuthentication()
    {
        var method = typeof(DictionaryService).GetMethod(nameof(DictionaryService.GetDicType));

        method.ShouldNotBeNull();
        method.GetCustomAttribute<IgnorePermissionAttribute>().ShouldNotBeNull();
        method.GetCustomAttribute<AllowAnonymousAttribute>().ShouldBeNull();
    }
}
