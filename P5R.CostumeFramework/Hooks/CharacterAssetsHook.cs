using P5R.CostumeFramework.Costumes;
using P5R.CostumeFramework.Models;
using p5rpc.lib.interfaces;
using Reloaded.Hooks.Definitions;
using Reloaded.Hooks.Definitions.X64;
using Reloaded.Memory.SigScan.ReloadedII.Interfaces;
using static Reloaded.Hooks.Definitions.X64.FunctionAttribute;

namespace P5R.CostumeFramework.Hooks;

internal class CharacterAssetsHook : IGameHook
{
    [Function(Register.r8, Register.rax, true)]
    private delegate nint RedirectCharAsset(ushort param);

    private IReverseWrapper<RedirectCharAsset>? setGuiWrapper;
    private IAsmHook? setGuiHook;

    private IReverseWrapper<RedirectCharAsset>? setCutinWrapper;
    private IAsmHook? setCutinHook;

    private IReverseWrapper<RedirectCharAsset>? setFutabaSkillWrapper;
    private IAsmHook? setFutabaSkillHook;

    private IReverseWrapper<RedirectCharAsset>? setFutabaGoodbyeWrapper;
    private IAsmHook? setFutabaGoodbyeHook;

    private readonly IP5RLib p5rLib;
    private readonly CostumeRegistry costumes;

    public CharacterAssetsHook(IP5RLib p5rLib, CostumeRegistry costumes)
    {
        this.p5rLib = p5rLib;
        this.costumes = costumes;
    }

    public void Initialize(IStartupScanner scanner, IReloadedHooks hooks)
    {
        scanner.Scan(
            "Costume GUI Hook",
            "48 8D 4D ?? E8 ?? ?? ?? ?? 45 33 C0 44 8B F6",
            result =>
            {
                var patch = AssembleRedirectPatch(hooks, character => this.RedirectCharAssetFile((Character)character, AssetType.Gui), out this.setGuiWrapper);
                this.setGuiHook = hooks.CreateAsmHook(patch, result).Activate();
            });

        scanner.Scan(
            "Costume Cutin Hook",
            "E8 ?? ?? ?? ?? 45 33 C0 48 8D 4C 24 ?? 41 8D 50 ?? E8 ?? ?? ?? ?? 48 89 47 ??",
            result =>
            {
                var patch = AssembleRedirectPatch(hooks, character => this.RedirectCharAssetFile((Character)character, AssetType.Cutin), out this.setCutinWrapper);
                this.setCutinHook = hooks.CreateAsmHook(patch, result).Activate();
            });

        scanner.Scan(
            "Futaba Skill Hook",
            "E8 ?? ?? ?? ?? 48 8D 0D ?? ?? ?? ?? 48 89 85 ?? ?? ?? ?? 48 89 4D ?? 0F 57 C0",
            result =>
            {
                var patch = AssembleRedirectPatch(hooks, character => this.RedirectCharAssetFile(Character.Futaba, AssetType.Futaba_Skill), out this.setFutabaSkillWrapper, Register.rcx);
                this.setFutabaSkillHook = hooks.CreateAsmHook(patch, result).Activate();
            });

        scanner.Scan(
            "Futaba Goodbye Hook",
            "E8 ?? ?? ?? ?? EB ?? 48 8D 15 ?? ?? ?? ?? EB ?? 45 85 ED",
            result =>
            {
                var patch = AssembleRedirectPatch(hooks, goodbyePersonaId => this.RedirectFutabaGoodbye(goodbyePersonaId), out this.setFutabaGoodbyeWrapper);
                this.setFutabaGoodbyeHook = hooks.CreateAsmHook(patch, result).Activate();
            });
    }

    private static string[] AssembleRedirectPatch(
        IReloadedHooks hooks,
        RedirectCharAsset func,
        out IReverseWrapper<RedirectCharAsset> wrapper,
        Register redirectRegister = Register.rdx)
    {
        return new string[]
        {
            "use64",
            Utilities.PushCallerRegisters,
            hooks.Utilities.GetAbsoluteCallMnemonics(func, out wrapper),
            Utilities.PopCallerRegisters,
            "test rax, rax",
            "jz original",
            $"mov {redirectRegister}, rax",
            "original:",
        };
    }

    private nint RedirectFutabaGoodbye(ushort goodbyePersonaId)
    {
        var currentOutfitId = this.p5rLib.GET_EQUIP(Character.Futaba, EquipSlot.Costume);
        string? redirectPath = null;

        if (this.costumes.TryGetCostume(currentOutfitId, out var costume))
        {
            switch (goodbyePersonaId)
            {
                case 1:
                    if (costume.FutabaGoodbyeBindPath_1 != null)
                    {
                        redirectPath = costume.FutabaGoodbyeBindPath_1;
                    }
                    break;
                case 2:
                    if (costume.FutabaGoodbyeBindPath_2 != null)
                    {
                        redirectPath = costume.FutabaGoodbyeBindPath_2;
                    }
                    break;
                case 3:
                    if (costume.FutabaGoodbyeBindPath_3 != null)
                    {
                        redirectPath = costume.FutabaGoodbyeBindPath_3;
                    }
                    break;
                default:
                    Log.Error($"Unknown Futaba goodbye persona ID: {goodbyePersonaId}");
                    break;
            }
        }

        if (redirectPath != null)
        {
            Log.Debug($"Character asset redirected: {Character.Futaba} || {AssetType.Futaba_Goodbye} || {redirectPath}");
            return StringsCache.GetStringPtr(redirectPath);
        }

        return IntPtr.Zero;
    }

    private nint RedirectCharAssetFile(Character character, AssetType type)
    {
        if (!Enum.IsDefined(character))
        {
            return IntPtr.Zero;
        }

        var currentOutfitId = this.p5rLib.GET_EQUIP(character, EquipSlot.Costume);
        string? redirectPath = null;

        if (this.costumes.TryGetCostume(currentOutfitId, out var costume))
        {
            switch (type)
            {
                case AssetType.Gui:
                    if (costume.GuiBindPath != null)
                    {
                        redirectPath = costume.GuiBindPath;
                    }
                    break;
                case AssetType.Cutin:
                    if (costume.CutinBindPath != null)
                    {
                        redirectPath = costume.CutinBindPath;
                    }
                    break;
                case AssetType.Futaba_Skill:
                    if (costume.FutabaSkillBindPath != null)
                    {
                        redirectPath = costume.FutabaSkillBindPath;
                    }
                    break;
                default:
                    Log.Error($"Unknown asset redirection value: {type}");
                    break;
            }
        }

        if (redirectPath != null)
        {
            Log.Debug($"Character asset redirected: {character} || {type} || {redirectPath}");
            return StringsCache.GetStringPtr(redirectPath);
        }

        return IntPtr.Zero;
    }

    private enum AssetType
    {
        Gui,
        Cutin,
        Futaba_Skill,
        Futaba_Goodbye,
    }
}
