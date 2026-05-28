using Volo.Abp.Settings;

namespace Yi.Module.FileManagement.Domain.Shared.Settings;

/// <summary>
/// 文件管理模块 OSS Setting 定义提供者。
/// 注册所有 OSS 相关的 Setting 键，并设置默认值和继承规则。
/// <para>isInherited = true：租户未设置时自动回退到 Global → Configuration → Default 链。</para>
/// </summary>
public class FileManagementSettingDefinitionProvider : SettingDefinitionProvider
{
    public override void Define(ISettingDefinitionContext context)
    {
        context.Add(
            new SettingDefinition(FileManagementSettingNames.Provider, isInherited: true),
            new SettingDefinition(FileManagementSettingNames.PathPrefix, isInherited: true),
            new SettingDefinition(FileManagementSettingNames.Aliyun.AccessKeyId, isInherited: true),
            new SettingDefinition(FileManagementSettingNames.Aliyun.AccessKeySecret, isInherited: true, isEncrypted: true),
            new SettingDefinition(FileManagementSettingNames.Aliyun.Endpoint, isInherited: true),
            new SettingDefinition(FileManagementSettingNames.Aliyun.ContainerName, isInherited: true),
            new SettingDefinition(FileManagementSettingNames.Aliyun.CustomDomain, isInherited: true),
            new SettingDefinition(FileManagementSettingNames.Aliyun.CreateContainerIfNotExists, "false", isInherited: true)
        );
    }
}
