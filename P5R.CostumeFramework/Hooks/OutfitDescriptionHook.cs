using AtlusScriptLibrary.MessageScriptLanguage.Compiler;
using P5R.CostumeFramework.Costumes;
using Reloaded.Hooks.Definitions;
using Reloaded.Hooks.Definitions.X64;
using Reloaded.Memory.SigScan.ReloadedII.Interfaces;
using Reloaded.Mod.Interfaces;
using System.Runtime.InteropServices;
using static Reloaded.Hooks.Definitions.X64.FunctionAttribute;

namespace P5R.CostumeFramework.Hooks;

internal unsafe class OutfitDescriptionHook : IGameHook
{
    [Function(new Register[] { Register.rsi, Register.rcx }, Register.rax, true)]
    private delegate nint LoadGameBmd(nint fileStrPtr, nint bmdPtr);
    private IReverseWrapper<LoadGameBmd>? gameBmdWrapper;
    private IAsmHook? gameBmdHook;

    [Function(CallingConventions.Microsoft)]
    private delegate int InitializeBmdFunction(nint bmdPtr);
    private IFunction<InitializeBmdFunction>? initializeBmd;
    private IHook<InitializeBmdFunction>? initalizeBmdHook;

    [Function(new Register[] { Register.rcx, Register.r9 }, Register.rax, true)]
    private delegate nint GetDescriptionPtr(nint bmdPtr, int entryIndex);
    private IReverseWrapper<GetDescriptionPtr>? getDescriptionWrapper;
    private IAsmHook? getDescriptionHook;

    private readonly nint* originalDressPtr;
    private readonly nint* customDressPtr;

    public OutfitDescriptionHook(IModLoader modLoader, MessageScriptCompiler compiler, CostumeRegistry costumes)
    {
        this.originalDressPtr = (nint*)Marshal.AllocHGlobal(sizeof(nint));
        this.customDressPtr = (nint*)Marshal.AllocHGlobal(sizeof(nint));

        // Build descriptions binary once all mods have loaded.
        modLoader.OnModLoaderInitialized += () =>
        {
            var descriptionsBin = CostumeDescriptions.Build(costumes.CostumesList, compiler);
            *this.customDressPtr = Marshal.AllocHGlobal(descriptionsBin.Length);
            Marshal.Copy(descriptionsBin, 0, *customDressPtr, descriptionsBin.Length);
            Log.Debug($"Mod costumes datDressHelp.bmd created at: {*this.customDressPtr:X}");
        };
    }

    public void Initialize(IStartupScanner scanner, IReloadedHooks hooks)
    {
        scanner.Scan("Load Game BMD Hook", "E8 ?? ?? ?? ?? 4C 8B 3D ?? ?? ?? ?? 8B C8", result =>
        {
            var patch = new string[]
            {
                "use64",
                Utilities.PushCallerRegisters,
                hooks.Utilities.GetAbsoluteCallMnemonics(this.LoadGameBmdImpl, out this.gameBmdWrapper),
                Utilities.PopCallerRegisters,
                "cmp rax, 0",
                "jz original",
                "mov rcx, rax",
                "original:"
            };

            this.gameBmdHook = hooks.CreateAsmHook(patch, result).Activate();
        });

        scanner.Scan("Initialize BMD Function", "48 83 EC 28 66 83 79 ?? 00", result =>
        {
            this.initalizeBmdHook = hooks.CreateHook<InitializeBmdFunction>(this.InitializeBmdImpl, result).Activate();
        });

        scanner.Scan(
            "Use Custom Descriptions Hook",
            "44 39 49 ?? 72 ?? F3 0F 10 84 24", result =>
            {
                var patch = new string[]
                {
                    "use64",
                    "push r14",

                    "start:",
                    $"mov r14, {(nint)this.customDressPtr}",
                    "mov r14, [r14]",
                    "cmp rcx, r14",
                    "je datDressHelp",

                    $"mov r14, {(nint)this.originalDressPtr}",
                    "mov r14, [r14]",
                    "cmp rcx, r14",
                    "jne original",

                    "datDressHelp:",
                    "cmp r9d, 286",
                    $"mov r14, {(nint)this.originalDressPtr}",
                    "mov r14, [r14]",
                    "jl setPtr",
                    $"mov r14, {(nint)this.customDressPtr}",
                    "mov r14, [r14]",
                    "setPtr:",
                    "mov [rax + 8], r14",
                    "mov rcx, r14",

                    "original:",
                    "pop r14"
                };


                this.getDescriptionHook = hooks.CreateAsmHook(patch, result).Activate();
            });
    }

    private int InitializeBmdImpl(nint bmdPtr)
    {
        return this.initalizeBmdHook!.OriginalFunction(bmdPtr);
    }

    private nint LoadGameBmdImpl(nint fileStrPtr, nint bmdPtr)
    {
        var fileName = Marshal.PtrToStringAnsi(fileStrPtr);
        
        if (fileName == "datDressHelp.bmd")
        {
            *this.originalDressPtr = bmdPtr;
            this.InitializeBmdImpl(*this.originalDressPtr);

            Log.Debug("Using custom datDressHelp.bmd.");
            return *this.customDressPtr;
        }

        return bmdPtr;
    }
}
