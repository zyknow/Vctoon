using EasyAbp.Abp.SettingUi.Localization;
using Volo.Abp.Localization;
using Volo.Abp.Settings;

namespace Vctoon.Settings;

public class VctoonSettingDefinitionProvider : SettingDefinitionProvider
{
    public override void Define(ISettingDefinitionContext context)
    {
        //Define your own settings here. Example:
        //context.Add(new SettingDefinition(VctoonSettings.MySetting1));

        // Medium.ComicDetailVisibleImages
        context.Add(
            new SettingDefinition(
                    "Medium.ComicDetailVisibleImages",
                    "false",
                    L(@$"DisplayName:Medium.ComicDetailVisibleImages"),
                    L(@$"Description:Medium.ComicDetailVisibleImages"),
                    isVisibleToClients: true
                )
                .WithProperty("Group1", "Medium")
                .WithProperty("Group2", "Comic")
                .WithProperty("Type", "checkbox")
        );
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<SettingUiResource>(name);
    }
}