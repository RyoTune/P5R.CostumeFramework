using CriFs.V2.Hook.Interfaces;
using P5R.CostumeFramework.Characters;
using P5R.CostumeFramework.Configuration;
using P5R.CostumeFramework.Interfaces;
using P5R.CostumeFramework.Models;
using Reloaded.Mod.Interfaces;
using Reloaded.Mod.Interfaces.Internal;
using System.Diagnostics.CodeAnalysis;

namespace P5R.CostumeFramework.Costumes;

internal class CostumeRegistry : ICostumeApi
{
    private readonly IModLoader modLoader;
    private readonly ICriFsRedirectorApi criFsApi;
    private readonly CostumeFactory costumeFactory;
    private readonly CharacterAssetsSettings assetSettings;

    private readonly Dictionary<Character, Costume> randomizedCostumes;

    public CostumeRegistry(
        IModLoader modLoader,
        Config config,
        CharacterAssetsSettings assetSettings)
    {
        this.modLoader = modLoader;
        this.assetSettings = assetSettings;

        this.modLoader.GetController<ICriFsRedirectorApi>().TryGetTarget(out criFsApi!);
        this.modLoader.ModLoading += this.OnModLoading;

        this.costumeFactory = new(criFsApi, this.CostumesList);
        //this.randomizedCostumes = CostumeRegistryUtils.AddRandomizedCostumes(this.costumeFactory)
        //    .GroupBy(x => x.Character)
        //    .ToDictionary(x => x.Key, x => x.First());

        if (config.ExtraOutfits)
        {
            CostumeRegistryUtils.AddExistingCostumes(this.costumeFactory);
        }
    }

    public GameCostumes CostumesList { get; } = new();

    public Costume? GetCostumeById(int itemId)
        => this.CostumesList.FirstOrDefault(x => x.ItemId == itemId);

    public bool TryGetCostume(int itemId, [NotNullWhen(true)] out Costume? costume)
    {
        costume = this.CostumesList.FirstOrDefault(x => x.ItemId == itemId);
        if (costume != null && this.IsActiveCostume(costume))
        {
            return true;
        }

        return false;
    }

    public bool IsActiveCostume(int itemId)
    {
        if (!VirtualOutfitsSection.IsOutfit(itemId))
        {
            return false;
        }

        if (this.CostumesList.FirstOrDefault(x => x.ItemId == itemId) is Costume costume)
        {
            return this.IsActiveCostume(costume);
        }

        return false;
    }

    public bool IsActiveCostume(Costume costume)
    {
        if (costume.IsEnabled
            && costume.Config.CharacterAssets == this.assetSettings[costume.Character])
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Converts <paramref name="itemId"/> to a valid costume item ID based on various factors.
    /// </summary>
    /// <param name="itemId">Item ID to check.</param>
    /// <returns><paramref name="itemId"/> or valid item ID.</returns>
    public int ToValidCostumeItemId(Character character, int itemId)
    {
        if (this.IsActiveCostume(itemId))
        {
            Log.Debug($"Valid costume: {character} || {itemId}");
            return itemId;
        }
        else
        {
            // Current active costumes for character.
            var activeCostumes = this.CostumesList.Where(x => x.Character == character && this.IsActiveCostume(x.ItemId)).ToArray();

            // Use costume marked as default.
            if (activeCostumes.FirstOrDefault(x => x.Config.IsDefault == true) is Costume defaultCostume)
            {
                Log.Debug($"Using default costume: {character} || {defaultCostume.ItemId}");
                return defaultCostume.ItemId;
            }

            // Use first costume for character.
            if (activeCostumes.Length > 0)
            {
                Log.Debug($"Using first costume: {character} || {activeCostumes[0].ItemId}");
                return activeCostumes[0].ItemId;
            }

            // Else fallback to game default costume.
            var gameDefault = this.CostumesList.First(x => x.Character == character).ItemId;
            Log.Debug($"Using game default costume: {character} || {gameDefault}");
            return gameDefault;
        }
    }

    public Costume? GetRandomCostume(Character character)
    {
        var costumes = this.CostumesList
            .Where(x => x.Character == character)
            .Where(x => x.GmdBindPath != null).ToArray();

        if (costumes.Length < 1)
        {
            return null;
        }

        return costumes[Random.Shared.Next(0, costumes.Length)];
    }

    private void OnModLoading(IModV1 mod, IModConfigV1 config)
    {
        if (!config.ModDependencies.Contains("P5R.CostumeFramework"))
        {
            return;
        }

        var modDir = this.modLoader.GetDirectoryForModId(config.ModId);
        this.AddBindFiles(modDir);

        var costumesDir = Path.Join(modDir, "costumes");
        if (Directory.Exists(costumesDir))
        {
            this.AddCostumesFolder(config.ModId, costumesDir);
        }
    }

    private void AddBindFiles(string modDir)
    {
        var bindDir = Path.Join(modDir, "costumes", "bind");
        if (Directory.Exists(bindDir))
        {
            foreach (var file in Directory.EnumerateFiles(bindDir, "*", SearchOption.AllDirectories))
            {
                var relativeFilePath = Path.GetRelativePath(bindDir, file);
                this.criFsApi.AddBind(file, relativeFilePath, "Costume Framework");
                Log.Debug($"Costume file binded: {relativeFilePath}");
            }
        }
    }

    public void AddCostumesFolder(string modId, string costumesDir)
    {
        // Register mod costumes.
        foreach (var character in Enum.GetValues<Character>())
        {
            var characterDir = Path.Join(costumesDir, character.ToString());
            if (!Directory.Exists(characterDir))
            {
                continue;
            }

            // Add costume files for existing costumes.
            foreach (var costume in this.CostumesList.Where(x => x.Character == character && x.Name != null))
            {
                this.costumeFactory.AddCostumeFiles(costume, costumesDir, modId);
            }

            // Build new costumes from GMD files.
            foreach (var file in Directory.EnumerateFiles(characterDir, "*.gmd", SearchOption.TopDirectoryOnly))
            {
                this.costumeFactory.Create(modId, costumesDir, character, file);
            }
        }
    }
}
