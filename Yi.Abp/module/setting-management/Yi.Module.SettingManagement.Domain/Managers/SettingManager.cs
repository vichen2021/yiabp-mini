using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Settings;
using Yi.Module.SettingManagement.Domain.Shared;

namespace Yi.Module.SettingManagement.Domain;

/// <summary>
/// <see cref="ISettingManager"/> 的核心实现，单例注册。
/// 按 <see cref="SettingManagementOptions.Providers"/> 注册的顺序（从低到高）遍历 Provider 链，
/// 读取时取最高优先级的非空值，写入时仅写入目标 Provider 维度，并自动处理加密/解密。
/// </summary>
public class SettingManager : ISettingManager, ISingletonDependency
{
    /// <summary>Setting 定义管理器，用于获取 <see cref="Volo.Abp.Settings.SettingDefinition"/>。</summary>
    protected ISettingDefinitionManager SettingDefinitionManager { get; }
    /// <summary>Setting 加密服务，负责对标记 isEncrypted 的 Setting 进行加解密。</summary>
    protected ISettingEncryptionService SettingEncryptionService { get; }
    /// <summary>已注册的 Provider 列表（懒加载，首次访问时从 DI 容器解析）。</summary>
    protected List<ISettingManagementProvider> Providers => _lazyProviders.Value;
    /// <summary>Provider 注册选项。</summary>
    protected SettingManagementOptions Options { get; }
    private readonly Lazy<List<ISettingManagementProvider>> _lazyProviders;

    /// <summary>注入依赖，Provider 列表采用懒加载避免循环依赖。</summary>
    public SettingManager(
        IOptions<SettingManagementOptions> options,
        IServiceProvider serviceProvider,
        ISettingDefinitionManager settingDefinitionManager,
        ISettingEncryptionService settingEncryptionService)
    {
        SettingDefinitionManager = settingDefinitionManager;
        SettingEncryptionService = settingEncryptionService;
        Options = options.Value;

        //TODO: Instead, use IServiceScopeFactory and create a scope..?

        _lazyProviders = new Lazy<List<ISettingManagementProvider>>(
            () => Options
                .Providers
                .Select(c => serviceProvider.GetRequiredService(c) as ISettingManagementProvider)
                .Where(x => x != null)
                .ToList()!,
            true
        );
    }

    /// <summary>
    /// 读取指定 Provider 维度下的 Setting 值；<paramref name="fallback"/> 为 true 时自动回退到低优先级 Provider。
    /// </summary>
    public virtual Task<string?> GetOrNullAsync(string name, string providerName, string? providerKey, bool fallback = true)
    {
        Check.NotNull(name, nameof(name));
        Check.NotNull(providerName, nameof(providerName));

        return GetOrNullInternalAsync(name, providerName, providerKey, fallback);
    }

    /// <summary>
    /// 读取指定 Provider 维度下所有 Setting 的值；<paramref name="fallback"/> 为 true 时对每个 Setting 自动回退到低优先级 Provider。
    /// </summary>
    public virtual async Task<List<SettingValue>> GetAllAsync(string providerName, string? providerKey, bool fallback = true)
    {
        Check.NotNull(providerName, nameof(providerName));

        var settingDefinitions =await SettingDefinitionManager.GetAllAsync();
        var providers = Enumerable.Reverse(Providers)
            .SkipWhile(c => c.Name != providerName);

        if (!fallback)
        {
            providers = providers.TakeWhile(c => c.Name == providerName);
        }

        var providerList = providers.Reverse().ToList();

        if (!providerList.Any())
        {
            return new List<SettingValue>();
        }

        var settingValues = new Dictionary<string, SettingValue>();

        foreach (var setting in settingDefinitions)
        {
            string? value = null;

            if (setting.IsInherited)
            {
                foreach (var provider in providerList)
                {
                    var providerValue = await provider.GetOrNullAsync(
                        setting,
                        provider.Name == providerName ? providerKey : null
                    );
                    if (providerValue != null)
                    {
                        value = providerValue;
                    }
                }
            }
            else
            {
                value = await providerList[0].GetOrNullAsync(
                    setting,
                    providerKey
                );
            }

            if (setting.IsEncrypted)
            {
                value = SettingEncryptionService.Decrypt(setting, value);
            }

            if (value != null)
            {
                settingValues[setting.Name] = new SettingValue(setting.Name, value);
            }
        }

        return settingValues.Values.ToList();
    }

    /// <summary>
    /// 写入 Setting 值到指定 Provider 维度。
    /// 若值与下一级 Provider 的回退值相同，且 <paramref name="forceToSet"/> 为 false，则自动清除当前维度的存储。
    /// </summary>
    public virtual async Task SetAsync(string name, string? value, string providerName, string? providerKey, bool forceToSet = false)
    {
        Check.NotNull(name, nameof(name));
        Check.NotNull(providerName, nameof(providerName));

        var setting =await SettingDefinitionManager.GetAsync(name);

        var providers = Enumerable
            .Reverse(Providers)
            .SkipWhile(p => p.Name != providerName)
            .ToList();

        if (!providers.Any())
        {
            return;
        }

        if (setting.IsEncrypted)
        {
            value = SettingEncryptionService.Encrypt(setting, value);
        }

        if (providers.Count > 1 && !forceToSet && setting.IsInherited && value != null)
        {
            var fallbackValue = await GetOrNullInternalAsync(name, providers[1].Name, null);
            if (fallbackValue == value)
            {
                //Clear the value if it's same as it's fallback value
                value = null;
            }
        }

        providers = providers
            .TakeWhile(p => p.Name == providerName)
            .ToList(); //Getting list for case of there are more than one provider with same providerName

        if (value == null)
        {
            foreach (var provider in providers)
            {
                await provider.ClearAsync(setting, providerKey);
            }
        }
        else
        {
            foreach (var provider in providers)
            {
                await provider.SetAsync(setting, value, providerKey);
            }
        }
    }

    /// <summary>
    /// 内部实现：遍历 Provider 链并返回最高优先级的非空值，同时封装加密解密逻辑。
    /// </summary>
    protected virtual async Task<string?> GetOrNullInternalAsync(string name, string providerName, string? providerKey, bool fallback = true)
    {
        var setting =await SettingDefinitionManager.GetAsync(name);
        var providers = Enumerable
            .Reverse(Providers);

        if (providerName != null)
        {
            providers = providers.SkipWhile(c => c.Name != providerName);
        }

        if (!fallback || !setting.IsInherited)
        {
            providers = providers.TakeWhile(c => c.Name == providerName);
        }

        string value = null;
        foreach (var provider in providers)
        {
            value = await provider.GetOrNullAsync(
                setting,
                provider.Name == providerName ? providerKey : null
            );

            if (value != null)
            {
                break;
            }
        }

        if (setting.IsEncrypted)
        {
            value = SettingEncryptionService.Decrypt(setting, value);
        }

        return value;
    }
}
