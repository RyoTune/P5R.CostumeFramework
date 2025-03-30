using P5R.CostumeFramework.Models;
using p5rpc.lib.interfaces;
using Reloaded.Hooks.Definitions;
using Reloaded.Hooks.Definitions.Enums;
using Reloaded.Hooks.Definitions.X64;
using Reloaded.Memory.SigScan.ReloadedII.Interfaces;
using static Reloaded.Hooks.Definitions.X64.FunctionAttribute;

namespace P5R.CostumeFramework.Hooks;

internal class EmtGapHook
{
    [Function(new Register[] { Register.r8, Register.rax }, Register.rax, true)]
    private delegate int SetGapEmtModelId(Character character, int originalModelId);
    private IReverseWrapper<SetGapEmtModelId>? setEmtModelIdWrapper;
    private IAsmHook? setEmtModelIdHook;

    private readonly IP5RLib p5rLib;

    public EmtGapHook(
        IStartupScanner scanner,
        IReloadedHooks hooks,
        IP5RLib p5rLib)
    {
        this.p5rLib = p5rLib;
        scanner.Scan("Set GAP EMT Hook", "89 44 24 ?? E8 ?? ?? ?? ?? EB ?? 48 8B 07", result =>
        {
            var patch = new string[]
            {
                "use64",
                Utilities.PushCallerRegisters,
                hooks.Utilities.GetAbsoluteCallMnemonics(this.SetGapEmtModelIdImpl, out this.setEmtModelIdWrapper),
                Utilities.PopCallerRegisters,
            };

            this.setEmtModelIdHook = hooks.CreateAsmHook(patch, result, AsmHookBehaviour.ExecuteFirst).Activate();
        });
    }

    private int SetGapEmtModelIdImpl(Character character, int originalModelId)
    {
        if (!Enum.IsDefined(character) || character != Character.Morgana)
        {
            return originalModelId;
        }

        var morganaOufitItemId = this.p5rLib.FlowCaller.GET_EQUIP((int)character, (int)EquipSlot.Costume);
        if (VirtualOutfitsSection.IsModOutfit(morganaOufitItemId))
        {
            Log.Debug("Set Morgana EMT GAP to 51.");
            return 51;
        }

        return originalModelId;
    }
}
