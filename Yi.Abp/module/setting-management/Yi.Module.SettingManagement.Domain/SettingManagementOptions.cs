using Volo.Abp.Collections;

namespace Yi.Module.SettingManagement.Domain;

public class SettingManagementOptions
{
    public ITypeList<ISettingManagementProvider> Providers { get; }

    public SettingManagementOptions()
    {
        Providers = new TypeList<ISettingManagementProvider>();
    }
}
