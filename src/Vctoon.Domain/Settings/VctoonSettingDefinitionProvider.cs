using Volo.Abp.Settings;

namespace Vctoon.Settings;

public class VctoonSettingDefinitionProvider : SettingDefinitionProvider
{
    public override void Define(ISettingDefinitionContext context)
    {
        //Define your own settings here. Example:
        //context.Add(new SettingDefinition(VctoonSettings.MySetting1));
    }
}