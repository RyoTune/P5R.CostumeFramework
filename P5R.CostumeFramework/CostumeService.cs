using AtlusScriptLibrary.Common.Libraries;
using AtlusScriptLibrary.Common.Text.Encodings;
using AtlusScriptLibrary.MessageScriptLanguage;
using AtlusScriptLibrary.MessageScriptLanguage.Compiler;
using BGME.BattleThemes.Interfaces;
using BGME.Framework.Interfaces;
using P5R.CostumeFramework.Characters;
using P5R.CostumeFramework.Configuration;
using P5R.CostumeFramework.Costumes;
using P5R.CostumeFramework.Hooks;
using P5R.CostumeFramework.Interfaces;
using p5rpc.lib.interfaces;
using Reloaded.Hooks.Definitions;
using Reloaded.Memory.SigScan.ReloadedII.Interfaces;
using Reloaded.Mod.Interfaces;
using Reloaded.Mod.Interfaces.Internal;
using System.Text;

namespace P5R.CostumeFramework;

internal unsafe class CostumeService
{
    private readonly IModLoader modLoader;
    private readonly IBgmeApi bgme;
    private readonly IP5RLib p5rLib;
    private readonly IBattleThemesApi battleThemes;

    private readonly CostumeGmdHook costumeGmdHook;
    private readonly VirtualOutfitsHook outfitsHook;
    private readonly ItemCountHook itemCountHook;
    private readonly GoodbyeHook goodbyeHook;
    private readonly EquippedItemHook equippedItemHook;
    private readonly EmtGapHook emtGapHook;
    private readonly FieldChangeHook fieldChangeHook;
    private readonly CostumeMusicService costumeMusic;

    private readonly List<IGameHook> gameHooks = new();

    public CostumeService(IModV1 owner, IModLoader modLoader, IReloadedHooks hooks, Config config)
    {
        this.modLoader = modLoader;

        IStartupScanner scanner;
        this.modLoader.GetController<IStartupScanner>().TryGetTarget(out scanner!);
        this.modLoader.GetController<IBgmeApi>().TryGetTarget(out this.bgme!);
        this.modLoader.GetController<IP5RLib>().TryGetTarget(out this.p5rLib!);
        this.modLoader.GetController<IBattleThemesApi>().TryGetTarget(out this.battleThemes!);

        var modDir = modLoader.GetDirectoryForModId("P5R.CostumeFramework");

        AtlusEncoding.SetCharsetDirectory(Path.Join(modDir, "Charsets"));
        LibraryLookup.SetLibraryPath(Path.Join(modDir, "Libraries"));
        var compiler = new MessageScriptCompiler(FormatVersion.Version1BigEndian, AtlusEncoding.Persona5RoyalEFIGS)
        {
            Library = LibraryLookup.GetLibrary("p5r")
        };

        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance); // Needed for shift_jis encoding to be available

        var assetSettings = new CharacterAssetsSettings(config);
        CharacterAssetsLoader.Init(modLoader, assetSettings);

        var costumes = new CostumeRegistry(modLoader, config, assetSettings);
        this.modLoader.AddOrReplaceController<ICostumeApi>(owner, costumes);

        this.costumeMusic = new(bgme, battleThemes, p5rLib, config, costumes);

        this.gameHooks.Add(new ItemNameHook(costumes));
        this.gameHooks.Add(new CharacterAssetsHook(p5rLib, costumes));
        this.gameHooks.Add(new OutfitDescriptionHook(modLoader, compiler, costumes));
        this.gameHooks.Add(new PartyHook(costumeMusic));
        foreach (var hook in this.gameHooks)
        {
            hook.Initialize(scanner, hooks);
        }

        this.outfitsHook = new(scanner, hooks);
        this.itemCountHook = new(scanner, hooks, config, costumes);
        this.goodbyeHook = new(scanner, hooks, p5rLib, costumes);
        this.emtGapHook = new(scanner, hooks, p5rLib);
        this.fieldChangeHook = new(scanner, hooks, p5rLib, config, costumes, this.costumeMusic);
        this.equippedItemHook = new(scanner, hooks, p5rLib, costumes, this.costumeMusic);
        this.costumeGmdHook = new(scanner, hooks, bgme, p5rLib, config, costumes, equippedItemHook);
    }
}
