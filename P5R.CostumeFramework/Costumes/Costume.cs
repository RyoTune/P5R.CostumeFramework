using P5R.CostumeFramework.Models;

namespace P5R.CostumeFramework.Costumes;

internal class Costume
{
    public const string DEFAULT_DESCRIPTION = "[f 0 5 65278][f 2 1]Outfit added with Costume Framework.[n][e]";
    private static readonly string[] shoutouts = new string[]
    {
        "Consider checking out SCArkadia's difficulty mod,[n]P5R Reimagined to experience P5R in a new way![n]",
        "DeathChaos's mod, Custom Bonus Tweaks, adds[n]new features, outfits, and BGM as well as[n]implementing unused content![n]",
        "Try out Ercar's AiO Costume Pack which[n]adds over 100 new outfits, some including BGM![n]",
    };

    private string descriptionMessage = DEFAULT_DESCRIPTION;

    public Costume(Character character, int itemId)
    {
        this.Character = character;
        this.ItemId = itemId;
    }

    public Character Character { get; }

    public int ItemId { get; }

    public bool IsEnabled { get; set; }

    public string? Name { get; set; }

    public CostumeConfig Config { get; set; } = new();

    public string? OwnerModId { get; set; }

    public string? GmdFilePath { get; set; }

    public string? GmdBindPath { get; set; }

    public string? MusicScriptFile { get; set; }

    public string? BattleThemeFile { get; set; }

    public string DescriptionMsg
    {
        get
        {
            if (this.descriptionMessage == DEFAULT_DESCRIPTION && Random.Shared.Next(0, 3) == 0)
            {
                var randomShout = shoutouts[Random.Shared.Next(0, shoutouts.Length)];
                return $"[f 0 5 65278][f 2 1]{randomShout}[e]";
            }

            return this.descriptionMessage;
        }
        set => this.descriptionMessage = value;
    }

    /// <summary>
    /// AOA character animation ending.
    /// </summary>
    public string? GoodbyeBindPath { get; set; }

    /// <summary>
    /// Crit/weakness cutin image.
    /// </summary>
    public string? CutinBindPath { get; set; }

    /// <summary>
    /// AOA portrait.
    /// </summary>
    public string? GuiBindPath { get; set; }

    public string? WeaponBindPath { get; set; }

    public string? WeaponRBindPath { get; set; }

    public string? WeaponLBindPath { get; set; }

    public string? RangedBindPath { get; set; }

    public string? RangedRBindPath { get; set; }

    public string? RangedLBindPath { get; set; }

    /// <summary>
    /// Futaba skill BCD.
    /// </summary>
    public string? FutabaSkillBindPath { get; set; }

    public string? FutabaGoodbyeBindPath_1 { get; set; }

    public string? FutabaGoodbyeBindPath_2 { get; set; }

    public string? FutabaGoodbyeBindPath_3 { get; set; }
}
