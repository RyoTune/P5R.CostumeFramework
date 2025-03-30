using CriFs.V2.Hook.Interfaces;
using P5R.CostumeFramework.Models;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace P5R.CostumeFramework.Costumes;

internal class CostumeFactory
{
    private readonly ICriFsRedirectorApi criFsApi;
    private readonly GameCostumes costumes;

    private readonly IDeserializer deserializer = new DeserializerBuilder()
        .WithNamingConvention(UnderscoredNamingConvention.Instance)
        .Build();

    public CostumeFactory(
        ICriFsRedirectorApi criFsApi,
        GameCostumes costumes)
    {
        this.criFsApi = criFsApi;
        this.costumes = costumes;
    }

    public Costume? Create(string modId, string costumesDir, Character character, string gmdFile)
    {
        var costume = this.GetAvailableModCostume(character);
        if (costume == null)
        {
            Log.Warning($"No available costume slots for: {character}");
            return null;
        }

        costume.Name = Path.GetFileNameWithoutExtension(gmdFile);
        costume.IsEnabled = true;
        this.AddGmdFile(costume, gmdFile, costumesDir);
        this.AddCostumeFiles(costume, costumesDir, modId);

        Log.Information($"Costume created: {costume.Character} || Item ID: {costume.ItemId} || Bind: {costume.GmdBindPath}");
        return costume;
    }

    public Costume? CreateFromExisting(Character character, string name, string bindPath)
    {
        var costume = this.GetAvailableModCostume(character);
        if (costume == null)
        {
            Log.Warning($"No available costume slots for: {character}");
            return null;
        }

        costume.Name = name;
        costume.IsEnabled = true;
        costume.GmdBindPath = bindPath;
        Log.Information($"Costume created: {costume.Character} || Item ID: {costume.ItemId} || Bind: {costume.GmdBindPath}");
        return costume;
    }

    public Costume? CreateFromExisting(Character character, string name, int modelId)
    {
        var bindPath = $@"MODEL\CHARACTER\{(int)character:D4}\C{(int)character:D4}_{modelId:D3}_00.GMD";
        return this.CreateFromExisting(character, name, bindPath);
    }

    public void AddCostumeFiles(Costume costume, string costumesDir, string modId)
    {
        this.LoadConfig(costume, costumesDir);
        this.AddGmdFile(costume, costumesDir);
        this.AddDescription(costume, costumesDir);
        this.AddMusic(costume, costumesDir, modId);
        this.AddCostumeCharAssets(costume, costumesDir);
        this.AddCutin(costume, costumesDir);
        this.AddGui(costume, costumesDir);
        this.AddWeapons(costume, costumesDir);
    }

    private void AddGmdFile(Costume costume, string gmdFile, string costumesDir)
    {
        costume.GmdFilePath = gmdFile;
        costume.GmdBindPath = Path.GetRelativePath(costumesDir, gmdFile);
        costume.IsEnabled = true;
        this.criFsApi.AddBind(costume.GmdFilePath, costume.GmdBindPath, "Costume Framework");
    }

    /// <summary>
    /// Adds a costume GMD from the costume folder.
    /// Mostly useful for overriding and enabling existing costumes.
    /// </summary>
    private void AddGmdFile(Costume costume, string costumesDir)
    {
        var costumeFile = Path.Join(this.GetCostumeDir(costume, costumesDir), "costume.gmd");
        if (File.Exists(costumeFile))
        {
            this.AddGmdFile(costume, costumeFile, costumesDir);
        }
    }

    private void LoadConfig(Costume costume, string costumesDir)
    {
        var configFile = Path.Join(this.GetCostumeDir(costume, costumesDir), "config.yaml");
        if (File.Exists(configFile))
        {
            try
            {
                var config = this.deserializer.Deserialize<CostumeConfig>(File.ReadAllText(configFile)) ?? new();
                costume.Config = config;
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Failed to load costume config for: {costume.Name}\nFile: {configFile}");
            }
        }
    }

    private void AddDescription(Costume costume, string costumesDir)
    {
        var descriptionFile = Path.Join(this.GetCostumeDir(costume, costumesDir), "description.msg");
        if (File.Exists(descriptionFile))
        {
            costume.DescriptionMsg = File.ReadAllText(descriptionFile);
        }
    }

    private void AddMusic(Costume costume, string costumesDir, string modId)
    {
        var musicFile = Path.Join(this.GetCostumeDir(costume, costumesDir), "music.pme");
        if (File.Exists(musicFile))
        {
            costume.MusicScriptFile = musicFile;
        }

        var battleThemeFile = Path.Join(this.GetCostumeDir(costume, costumesDir), "battle.theme.pme");
        if (File.Exists(battleThemeFile))
        {
            costume.OwnerModId = modId;
            costume.BattleThemeFile = battleThemeFile;
        }
    }

    private void AddCostumeCharAssets(Costume costume, string costumesDir)
    {
        var costumeDir = this.GetCostumeDir(costume, costumesDir);

        // Bind costume files in the bind dir.
        var costumeBindDir = Path.Join(costumeDir, "bind");
        if (Directory.Exists(costumeBindDir))
        {
            //var baseBindPath = Path.GetRelativePath(costumesDir, costumeDir);
            foreach (var file in Directory.EnumerateFiles(costumeBindDir, "*", SearchOption.AllDirectories))
            {
                this.criFsApi.AddBind(file, Path.GetRelativePath(costumeBindDir, file), "Costume Framework");
            }
        }

        var goodbyeFile = Path.Join(costumeDir, "aoa_goodbye.bcd");
        if (File.Exists(goodbyeFile))
        {
            costume.GoodbyeBindPath = Path.GetRelativePath(costumesDir, goodbyeFile);
            this.criFsApi.AddBind(goodbyeFile, costume.GoodbyeBindPath, "Costume Framework");
        }

        var futabaSkillFile = Path.Join(costumeDir, "futaba_skill.bcd");
        if (File.Exists(futabaSkillFile))
        {
            costume.FutabaSkillBindPath = Path.GetRelativePath(costumesDir, futabaSkillFile);
            this.criFsApi.AddBind(futabaSkillFile, costume.FutabaSkillBindPath, "Costume Framework");
        }

        var futabaGoodbyeFile_1 = Path.Join(costumeDir, "futaba_goodbye_1.bcd");
        if (File.Exists(futabaGoodbyeFile_1))
        {
            costume.FutabaGoodbyeBindPath_1 = Path.GetRelativePath(costumesDir, futabaGoodbyeFile_1);
            this.criFsApi.AddBind(futabaGoodbyeFile_1, costume.FutabaGoodbyeBindPath_1, "Costume Framework");
        }

        var futabaGoodbyeFile_2 = Path.Join(costumeDir, "futaba_goodbye_2.bcd");
        if (File.Exists(futabaGoodbyeFile_2))
        {
            costume.FutabaGoodbyeBindPath_2 = Path.GetRelativePath(costumesDir, futabaGoodbyeFile_2);
            this.criFsApi.AddBind(futabaGoodbyeFile_2, costume.FutabaGoodbyeBindPath_2, "Costume Framework");
        }

        var futabaGoodbyeFile_3 = Path.Join(costumeDir, "futaba_goodbye_3.bcd");
        if (File.Exists(futabaGoodbyeFile_3))
        {
            costume.FutabaGoodbyeBindPath_3 = Path.GetRelativePath(costumesDir, futabaGoodbyeFile_3);
            this.criFsApi.AddBind(futabaGoodbyeFile_3, costume.FutabaGoodbyeBindPath_3, "Costume Framework");
        }
    }

    private void AddCutin(Costume costume, string costumesDir)
    {
        var cutinFile = Path.Join(this.GetCostumeDir(costume, costumesDir), "battle_cutin.dds");
        if (File.Exists(cutinFile))
        {
            costume.CutinBindPath = Path.GetRelativePath(costumesDir, cutinFile);
            this.criFsApi.AddBind(cutinFile, costume.CutinBindPath, "Costume Framework");
        }
    }

    private void AddGui(Costume costume, string costumesDir)
    {
        var guiFile = Path.Join(this.GetCostumeDir(costume, costumesDir), "aoa_portrait.dds");
        if (File.Exists(guiFile))
        {
            costume.GuiBindPath = Path.GetRelativePath(costumesDir, guiFile);
            this.criFsApi.AddBind(guiFile, costume.GuiBindPath, "Costume Framework");
        }
    }

    private void AddWeapons(Costume costume, string costumesDir)
    {
        var weaponFile = Path.Join(this.GetCostumeDir(costume, costumesDir), "melee_weapon.gmd");
        if (File.Exists(weaponFile))
        {
            costume.WeaponBindPath = Path.GetRelativePath(costumesDir, weaponFile);
            this.criFsApi.AddBind(weaponFile, costume.WeaponBindPath, "Costume Framework");
        }

        var weaponRFile = Path.Join(this.GetCostumeDir(costume, costumesDir), "melee_weapon_r.gmd");
        if (File.Exists(weaponRFile))
        {
            costume.WeaponRBindPath = Path.GetRelativePath(costumesDir, weaponRFile);
            this.criFsApi.AddBind(weaponRFile, costume.WeaponRBindPath, "Costume Framework");
        }

        var weaponLFile = Path.Join(this.GetCostumeDir(costume, costumesDir), "melee_weapon_l.gmd");
        if (File.Exists(weaponLFile))
        {
            costume.WeaponLBindPath = Path.GetRelativePath(costumesDir, weaponLFile);
            this.criFsApi.AddBind(weaponLFile, costume.WeaponLBindPath, "Costume Framework");
        }

        var rangedFile = Path.Join(this.GetCostumeDir(costume, costumesDir), "ranged_weapon.gmd");
        if (File.Exists(rangedFile))
        {
            costume.RangedBindPath = Path.GetRelativePath(costumesDir, rangedFile);
            this.criFsApi.AddBind(rangedFile, costume.RangedBindPath, "Costume Framework");
        }

        var rangedRFile = Path.Join(this.GetCostumeDir(costume, costumesDir), "ranged_weapon_r.gmd");
        if (File.Exists(rangedRFile))
        {
            costume.RangedRBindPath = Path.GetRelativePath(costumesDir, rangedRFile);
            this.criFsApi.AddBind(rangedRFile, costume.RangedRBindPath, "Costume Framework");
        }

        var rangedLFile = Path.Join(this.GetCostumeDir(costume, costumesDir), "ranged_weapon_l.gmd");
        if (File.Exists(rangedLFile))
        {
            costume.RangedLBindPath = Path.GetRelativePath(costumesDir, rangedLFile);
            this.criFsApi.AddBind(rangedLFile, costume.RangedLBindPath, "Costume Framework");
        }
    }

    private string GetCostumeDir(Costume costume, string costumesDir)
        => Path.Join(costumesDir, costume.Character.ToString(), costume.Name);

    private Costume? GetAvailableModCostume(Character character)
        => this.costumes
        .FirstOrDefault(x =>
        x.Character == character
        && VirtualOutfitsSection.IsModOutfit(x.ItemId)
        && x.GmdBindPath == null);
}
