using P5R.CostumeFramework.Characters;
using P5R.CostumeFramework.Template.Configuration;
using System.ComponentModel;

namespace P5R.CostumeFramework.Configuration;

public class Config : Configurable<Config>
{
    [Category("Settings")]
    [DisplayName("Current Party BGM Only")]
    [Description("Whether to limit outfit BGM to only members in the current party.\nJoker and Futaba's outfit BGM is always enabled.")]
    [DefaultValue(true)]
    public bool CurrentPartyBgmOnly { get; set; } = true;

    [Category("Settings")]
    [DisplayName("Extra Outfits")]
    [Description("Add new outfits from existing game files.")]
    [DefaultValue(true)]
    public bool ExtraOutfits { get; set; } = true;

    [Category("Integrations")]
    [DisplayName("BGME Framework")]
    [DefaultValue(true)]
    public bool Integration_BGME { get; set; } = true;

    [Category("Integrations")]
    [DisplayName("BGME Battle Themes")]
    [DefaultValue(true)]
    public bool Integration_BattleThemes { get; set; } = true;

    [Category("Settings")]
    [DisplayName("Randomize Outfits")]
    [Description("Outfits will randomize when moving between areas.")]
    [DefaultValue(false)]
    public bool RandomizeCostumes { get; set; } = false;

    [Category("Settings")]
    [DisplayName("Overworld Outfits")]
    [Description("Outfits will apply in the overworld too.\nThis is just a for fun feature, expect bugs.")]
    [DefaultValue(false)]
    public bool OverworldCostumes { get; set; } = false;

    [Category("Character Assets")]
    [DisplayName("Joker")]
    public CharacterAssets Joker_Assets { get; set; }

    [Category("Character Assets")]
    [DisplayName("Ryuji")]
    public CharacterAssets Ryuji_Assets { get; set; }

    [Category("Character Assets")]
    [DisplayName("Morgana")]
    public CharacterAssets Morgana_Assets { get; set; }

    [Category("Character Assets")]
    [DisplayName("Ann")]
    public CharacterAssets Ann_Assets { get; set; }

    [Category("Character Assets")]
    [DisplayName("Yusuke")]
    public CharacterAssets Yusuke_Assets { get; set; }

    [Category("Character Assets")]
    [DisplayName("Makoto")]
    public CharacterAssets Makoto_Assets { get; set; }

    [Category("Character Assets")]
    [DisplayName("Haru")]
    public CharacterAssets Haru_Assets { get; set; }

    [Category("Character Assets")]
    [DisplayName("Futaba")]
    public CharacterAssets Futaba_Assets { get; set; }

    [Category("Character Assets")]
    [DisplayName("Akechi")]
    public CharacterAssets Akechi_Assets { get; set; }

    [Category("Character Assets")]
    [DisplayName("Sumire")]
    public CharacterAssets Sumire_Assets { get; set; }

    [Category("Debugging")]
    [DisplayName("Log Level")]
    [DefaultValue(LogLevel.Information)]
    public LogLevel LogLevel { get; set; } = LogLevel.Information;

    [Category("Debugging")]
    [DisplayName("Unlock All Items")]
    [DefaultValue(false)]
    public bool UnlockAllItems { get; set; } = false;
}

/// <summary>
/// Allows you to override certain aspects of the configuration creation process (e.g. create multiple configurations).
/// Override elements in <see cref="ConfiguratorMixinBase"/> for finer control.
/// </summary>
public class ConfiguratorMixin : ConfiguratorMixinBase
{
    // 
}