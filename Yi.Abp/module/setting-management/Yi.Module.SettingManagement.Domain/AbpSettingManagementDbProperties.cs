using Volo.Abp.Data;

namespace Yi.Module.SettingManagement.Domain;

/// <summary>
/// 设置管理模块的数据库属性常量（表前缀 / Schema / 连接串名称）。
/// </summary>
public static class AbpSettingManagementDbProperties
{
    /// <summary>
    /// 数据库表名前缀，默认继承 ABP 公共前缀。
    /// </summary>
    public static string DbTablePrefix { get; set; } = AbpCommonDbProperties.DbTablePrefix;

    /// <summary>
    /// 数据库 Schema，默认继承 ABP 公共 Schema。
    /// </summary>
    public static string DbSchema { get; set; } = AbpCommonDbProperties.DbSchema!;

    /// <summary>
    /// 设置管理模块对应的连接字符串名称。
    /// </summary>
    public const string ConnectionStringName = "AbpSettingManagement";
}
