using P5R.CostumeFramework.Configuration;
using P5R.CostumeFramework.Costumes;
using P5R.CostumeFramework.Models;
using p5rpc.lib.interfaces;
using Reloaded.Hooks.Definitions;
using Reloaded.Hooks.Definitions.Enums;
using Reloaded.Hooks.Definitions.X64;
using Reloaded.Memory.SigScan.ReloadedII.Interfaces;

namespace P5R.CostumeFramework.Hooks;

internal class FieldChangeHook
{
    [Function(CallingConventions.Microsoft)]
    private delegate void FieldChangedFunction();
    private IReverseWrapper<FieldChangedFunction> fieldChangedWrapper;
    private IAsmHook? fieldChangedHook;

    private readonly IP5RLib p5rLib;
    private readonly Config config;
    private readonly CostumeRegistry costumes;
    private readonly CostumeMusicService costumeMusic;

    public Action? FieldChanged;

    public FieldChangeHook(
        IStartupScanner scanner,
        IReloadedHooks hooks,
        IP5RLib p5rLib,
        Config config,
        CostumeRegistry costumes,
        CostumeMusicService costumeMusic)
    {
        this.p5rLib = p5rLib;
        this.config = config;
        this.costumes = costumes;
        this.costumeMusic = costumeMusic;

        var wrapperAsm = hooks.Utilities.GetAbsoluteCallMnemonics(() => this.FieldChanged?.Invoke(), out this.fieldChangedWrapper);
        var patch = new string[]
        {
                "use64",
                Utilities.PushCallerRegisters,
                "sub rsp, 32",
                wrapperAsm,
                "add rsp, 32",
                Utilities.PopCallerRegisters,
        };

        scanner.Scan("Field Change (FBN) Hook", "E8 ?? ?? ?? ?? 48 89 87 ?? ?? ?? ?? E8 ?? ?? ?? ?? 44 0F B7 0F", result =>
        {
            this.fieldChangedHook = hooks.CreateAsmHook(patch, result, AsmHookBehaviour.ExecuteFirst).Activate();
        });

        this.FieldChanged += this.RandomizeCostumes;
    }

    private void RandomizeCostumes()
    {
        if (!this.config.RandomizeCostumes)
        {
            return;
        }

        foreach (var character in Enum.GetValues<Character>())
        {
            var randomCostume = this.costumes.GetRandomCostume(character);
            if (randomCostume != null)
            {
                this.p5rLib.FlowCaller.SET_EQUIP((int)character, (int)EquipSlot.Costume, randomCostume.ItemId);
                Log.Debug($"Randomized costume: {character} || {randomCostume.GmdBindPath}");
            }
        }

        this.costumeMusic.Refresh();
    }
}
