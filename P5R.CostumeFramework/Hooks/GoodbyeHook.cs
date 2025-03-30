using P5R.CostumeFramework.Costumes;
using P5R.CostumeFramework.Models;
using p5rpc.lib.interfaces;
using Reloaded.Hooks.Definitions;
using Reloaded.Hooks.Definitions.X64;
using Reloaded.Memory.SigScan.ReloadedII.Interfaces;
using System.Runtime.InteropServices;
using static Reloaded.Hooks.Definitions.X64.FunctionAttribute;

namespace P5R.CostumeFramework.Hooks;

internal class GoodbyeHook
{
    [Function(Register.rdx, Register.rax, true)]
    private delegate nint GoodbyeHookFunction(nint goodbyeStrPtr);
    private IReverseWrapper<GoodbyeHookFunction>? goodbyeWrapper;
    private IAsmHook? goodbyeHook;

    private readonly IP5RLib p5rLib;
    private readonly CostumeRegistry costumes;

    private readonly Dictionary<nint, Character> goodbyeCache = new();

    public GoodbyeHook(
        IStartupScanner scanner,
        IReloadedHooks hooks,
        IP5RLib p5rLib,
        CostumeRegistry costumes)
    {
        this.p5rLib = p5rLib;
        this.costumes = costumes;
        scanner.Scan("Goodbye BCD Hook", "E8 ?? ?? ?? ?? 80 BD ?? ?? ?? ?? 00 0F 84 ?? ?? ?? ?? 4C 8D 8D", result =>
        {
            var patch = new string[]
            {
                "use64",
                Utilities.PushCallerRegisters,
                hooks.Utilities.GetAbsoluteCallMnemonics(this.GoodbyeHookImpl, out this.goodbyeWrapper),
                Utilities.PopCallerRegisters,
                "cmp rax, 0",
                "jz original",
                "mov rdx, rax",
                "original:",
            };

            this.goodbyeHook = hooks.CreateAsmHook(patch, result).Activate();
        });
    }

    private nint GoodbyeHookImpl(nint goodbyeStrPtr)
    {
        if (!this.goodbyeCache.TryGetValue(goodbyeStrPtr, out var character))
        {
            var goodbyeStr = Marshal.PtrToStringAnsi(goodbyeStrPtr)!;
            character = GetCharacter(goodbyeStr);
            this.goodbyeCache[goodbyeStrPtr] = character;
        }

        Log.Debug($"Getting Goodbye BMD for: {character}");

        var outfitItemId = this.p5rLib.FlowCaller.GET_EQUIP((int)character, (int)EquipSlot.Costume);
        if (this.costumes.TryGetCostume(outfitItemId, out var costume)
            && costume.GoodbyeBindPath != null)
        {
            return StringsCache.GetStringPtr(costume.GoodbyeBindPath);
        }

        return IntPtr.Zero;
    }

    private static Character GetCharacter(string goodbyeFile)
        => goodbyeFile switch
        {
            "battle/event/BCD/goodbye/bksk_hero.BCD" => Character.Joker,
            "battle/event/BCD/goodbye/bksk_reiji.BCD" => Character.Ryuji,
            "battle/event/BCD/goodbye/bksk_morgana.BCD" => Character.Morgana,
            "battle/event/BCD/goodbye/bksk_an.BCD" => Character.Ann,
            "battle/event/BCD/goodbye/bksk_yugo.BCD" => Character.Yusuke,
            "battle/event/BCD/goodbye/bksk_makoto.BCD" => Character.Makoto,
            "battle/event/BCD/goodbye/bksk_ranko.BCD" => Character.Haru,
            "battle/event/BCD/goodbye/bksk_futaba.BCD" => Character.Futaba,
            "battle/event/BCD/goodbye/bksk_ranko.BCD"
            or "battle/event/BCD/goodbye/bksk_aketi_b.BCD" => Character.Akechi,
            "battle/event/BCD/goodbye/bksk_uzuki.BCD" => Character.Sumire,
            _ => Character.Joker,
        };
}
