using Reloaded.Hooks.Definitions.Enums;
using Reloaded.Hooks.Definitions;
using Reloaded.Memory.SigScan.ReloadedII.Interfaces;
using P5R.CostumeFramework.Models;
using System.Runtime.InteropServices;

namespace P5R.CostumeFramework.Hooks;

internal unsafe class VirtualOutfitsHook
{
    private IAsmHook? virtualOutfitsHook;
    private readonly nint virtualOutfitsPtr;

    private IAsmHook? nextSectionHook;
    private readonly nint normalSectionSize;

    public VirtualOutfitsHook(IStartupScanner scanner, IReloadedHooks hooks)
    {
        this.virtualOutfitsPtr = Marshal.AllocHGlobal(sizeof(VirtualOutfitsSection));
        Marshal.StructureToPtr(new VirtualOutfitsSection(), virtualOutfitsPtr, false);
        this.normalSectionSize = Marshal.AllocHGlobal(sizeof(nint));
        
        scanner.Scan(
            "Use Virtual Outfit Section",
            "E8 ?? ?? ?? ?? 4C 63 45 ?? 48 8B D3 49 8B C8 48 89 05 ?? ?? ?? ?? 48 C1 E9 05",
            result =>
            {
                var sectionSizePatch = new string[]
                {
                    "use64",
        
                    // Save normal section size
                    "push rdi",
                    $"lea rdi, [qword {this.normalSectionSize}]",
                    "mov [rdi], eax",
                    "pop rdi",
        
                    // Get modded section size and make it use that
                    $"lea rax, [qword {virtualOutfitsPtr}]",
                    "mov ecx, [qword rax]",
                    "bswap ecx",
                    "mov [rbp + 0x10], ecx",
                };
        
                this.virtualOutfitsHook = hooks.CreateAsmHook(sectionSizePatch, result, AsmHookBehaviour.ExecuteFirst).Activate();
        
                // Fix pointer so it points to the next section
                // in the original item TBL.
                var nextSectionAddress = result + 0x24;
                var nextSectionPatch = new string[]
                {
                    "use64",

                    // Set pointer to virtual outfits section
                    $"lea rdx, [qword {virtualOutfitsPtr} + 4]",

                    // Set section length back to what it normally would be (used after this to determine next section location)
                    $"lea rax, [qword {this.normalSectionSize}]",
                    "mov eax, [rax]",
                    "mov [rbp + 0x10], eax",
                };
        
                this.nextSectionHook = hooks.CreateAsmHook(nextSectionPatch, nextSectionAddress, AsmHookBehaviour.ExecuteFirst).Activate();
            });
    }
}
