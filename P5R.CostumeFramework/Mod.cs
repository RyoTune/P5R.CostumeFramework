using P5R.CostumeFramework.Configuration;
using P5R.CostumeFramework.Interfaces;
using P5R.CostumeFramework.Template;
using Reloaded.Hooks.ReloadedII.Interfaces;
using Reloaded.Mod.Interfaces;
using System.Diagnostics;

namespace P5R.CostumeFramework;

public class Mod : ModBase, IExports
{
    private readonly IModLoader modLoader;
    private readonly IReloadedHooks hooks;
    private readonly ILogger logger;
    private readonly IMod owner;
    private Config config;
    private readonly IModConfig modConfig;

    private readonly CostumeService costumes;

    public Mod(ModContext context)
    {
        this.modLoader = context.ModLoader;
        this.hooks = context.Hooks!;
        this.logger = context.Logger;
        this.owner = context.Owner;
        this.config = context.Configuration;
        this.modConfig = context.ModConfig;

        Log.Logger = this.logger;
        Log.LogLevel = this.config.LogLevel;

#if DEBUG
        Debugger.Launch();
#endif

        try
        {
            this.costumes = new(this.owner, this.modLoader, this.hooks, this.config);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to start costumes service.");
        }
    }

    #region Standard Overrides
    public override void ConfigurationUpdated(Config configuration)
    {
        // Apply settings from configuration.
        // ... your code here.
        this.config = configuration;
        this.logger.WriteLine($"[{this.modConfig.ModId}] Config Updated: Applying");
    }
    #endregion

    #region For Exports, Serialization etc.
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public Type[] GetTypes() => new[] { typeof(ICostumeApi) };

    public Mod() { }
#pragma warning restore CS8618
    #endregion
}