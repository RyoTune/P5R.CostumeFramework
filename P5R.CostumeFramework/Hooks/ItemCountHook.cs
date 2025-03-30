using P5R.CostumeFramework.Configuration;
using P5R.CostumeFramework.Costumes;
using P5R.CostumeFramework.Models;
using Reloaded.Hooks.Definitions;
using Reloaded.Hooks.Definitions.X64;
using Reloaded.Memory.SigScan.ReloadedII.Interfaces;
using static Reloaded.Hooks.Definitions.X64.FunctionAttribute;

namespace P5R.CostumeFramework.Hooks;

#pragma warning disable CS8602 // Dereference of a possibly null reference.
internal unsafe class ItemCountHook
{
    [Function(new[] { Register.rbx, Register.rax }, Register.rax, true)]
    private delegate int GetItemCountFunction(int itemId, int itemCount);
    private IReverseWrapper<GetItemCountFunction>? itemCountWrapper;
    private IAsmHook? itemCountHook;

    /// <summary>
    /// Sets an item's count.
    /// </summary>
    /// <param name="itemId">Item ID.</param>
    /// <param name="itemCount">Item count.</param>
    /// <param name="param3">Related to whether an item is labeled "New" when swapping equipped item.</param>
    [Function(CallingConventions.Microsoft)]
    private delegate void SetItemCountFunction(int itemId, int itemCount, nint param3);
    private IHook<SetItemCountFunction>? setItemCountHook;

    private readonly Config config;
    private readonly CostumeRegistry costumes;

    public ItemCountHook(
        IStartupScanner scanner,
        IReloadedHooks hooks,
        Config config,
        CostumeRegistry costumes)
    {
        this.config = config;
        this.costumes = costumes;

        scanner.Scan("Get Item Count Hook", "84 C0 0F 84 ?? ?? ?? ?? 0F B7 FB C1 EF 0C", result =>
        {
            var patch = new string[]
            {
                "use64",
                Utilities.PushCallerRegisters,
                hooks.Utilities.GetAbsoluteCallMnemonics(this.GetItemCount, out this.itemCountWrapper),
                Utilities.PopCallerRegisters,
            };

            this.itemCountHook = hooks.CreateAsmHook(patch, result).Activate();
        });

        scanner.Scan("Set Item Count Function", "4C 8B DC 49 89 5B ?? 57 48 83 EC 70 48 8D 05", result =>
        {
            this.setItemCountHook = hooks.CreateHook<SetItemCountFunction>(this.SetItemCount, result).Activate();
        });
    }

    private void SetItemCount(int itemId, int itemCount, nint param3)
    {
        if (VirtualOutfitsSection.IsOutfit(itemId))
        {
            Log.Verbose($"SetItemCount || Item ID: {itemId} || Count: {itemCount} || param3: {param3}");
            Log.Verbose("Ignoring SetItemCount for costume.");
        }
        else
        {
            Log.Verbose($"SetItemCount || Item ID: {itemId} || Count: {itemCount} || param3: {param3}");
            this.setItemCountHook.OriginalFunction(itemId, itemCount, param3);
        }
    }

    private int GetItemCount(int itemId, int itemCount)
    {
        if (this.config.UnlockAllItems)
        {
            return 1;
        }

        if (VirtualOutfitsSection.IsOutfit(itemId))
        {
            if (this.costumes.IsActiveCostume(itemId))
            {
                Log.Verbose($"GetItemCount || Item ID: {itemId} || Count: {itemCount} || Overwriting costume count with 1.");
                return 1;
            }

            // Initial item count is from garbage data for mod costumes.
            return 0;
        }

        Log.Verbose($"GetItemCount || Item ID: {itemId} || Count: {itemCount}");
        return itemCount;
    }
}
