using Shouldly;
using Xunit;
using Yi.Module.Rbac.Domain.Authorization;
using Yi.Module.Rbac.Domain.Shared.Enums;
using Yi.Module.Rbac.Domain.Shared.Model;

namespace Yi.Module.Rbac.Test.Authorization;

public class DataPermissionScopeCalculatorTest
{
    /// <summary>
    /// 验证任一角色拥有全部数据范围时，最终结果不再附加其他限制
    /// </summary>
    [Fact]
    public void Should_Allow_All_When_Any_Role_Has_All_Scope()
    {
        var result = DataPermissionScopeCalculator.Calculate(
            Guid.NewGuid(),
            Guid.NewGuid(),
            [
                Role(DataScopeEnum.USER),
                Role(DataScopeEnum.ALL),
                Role(DataScopeEnum.CUSTOM)
            ]);

        result.IsAll.ShouldBeTrue();
    }

    /// <summary>
    /// 验证多角色的本人、部门树和自定义范围按并集合并，并去重自定义角色标识
    /// </summary>
    [Fact]
    public void Should_Union_Multiple_Role_Scopes()
    {
        var userId = Guid.NewGuid();
        var deptId = Guid.NewGuid();
        var customRoleId = Guid.NewGuid();

        var result = DataPermissionScopeCalculator.Calculate(
            userId,
            deptId,
            [
                Role(DataScopeEnum.USER),
                Role(DataScopeEnum.DEPT_FOLLOW),
                Role(DataScopeEnum.CUSTOM, customRoleId),
                Role(DataScopeEnum.CUSTOM, customRoleId)
            ]);

        result.IsAll.ShouldBeFalse();
        result.IncludesSelf.ShouldBeTrue();
        result.IncludesCurrentDept.ShouldBeTrue();
        result.IncludesDescendantDepts.ShouldBeTrue();
        result.CurrentUserId.ShouldBe(userId);
        result.CurrentDeptId.ShouldBe(deptId);
        result.CustomRoleIds.ShouldBe([customRoleId]);
    }

    /// <summary>
    /// 验证角色声明缺失时采用仅本人范围，防止异常令牌退化为全量权限
    /// </summary>
    [Fact]
    public void Should_Default_To_Self_When_Role_Info_Is_Missing()
    {
        var userId = Guid.NewGuid();

        var result = DataPermissionScopeCalculator.Calculate(userId, null, null);

        result.IsAll.ShouldBeFalse();
        result.IncludesSelf.ShouldBeTrue();
        result.CurrentUserId.ShouldBe(userId);
        result.IncludesCurrentDept.ShouldBeFalse();
        result.CustomRoleIds.ShouldBeEmpty();
    }

    /// <summary>
    /// 验证用户未归属部门时只保留部门范围声明，不生成不存在的部门标识
    /// </summary>
    [Fact]
    public void Should_Not_Add_Department_When_Current_User_Has_No_Department()
    {
        var result = DataPermissionScopeCalculator.Calculate(
            Guid.NewGuid(),
            null,
            [Role(DataScopeEnum.DEPT)]);

        result.IncludesCurrentDept.ShouldBeTrue();
        result.CurrentDeptId.ShouldBeNull();
        result.IncludesSelf.ShouldBeFalse();
    }

    /// <summary>
    /// 构造指定数据范围的角色令牌模型
    /// </summary>
    /// <param name="scope">角色数据范围</param>
    /// <param name="id">可选的固定角色标识，用于验证去重和自定义范围</param>
    /// <returns>角色令牌模型</returns>
    private static RoleTokenInfoModel Role(DataScopeEnum scope, Guid? id = null)
    {
        return new RoleTokenInfoModel
        {
            Id = id ?? Guid.NewGuid(),
            DataScope = scope
        };
    }
}
