using P5R.CostumeFramework.Costumes;
using P5R.CostumeFramework.Models;
using Reloaded.Hooks.Definitions;
using Reloaded.Hooks.Definitions.X64;
using Reloaded.Memory.SigScan.ReloadedII.Interfaces;

namespace P5R.CostumeFramework.Hooks;

internal unsafe class ItemNameHook : IGameHook
{
    [Function(CallingConventions.Microsoft)]
    private delegate nint GetItemNameFunction(int itemId);
    private IHook<GetItemNameFunction>? getItemNameHook;

    private readonly CostumeRegistry costumes;

    private readonly nint fallbackNameStrPtr;

    public ItemNameHook(CostumeRegistry costumes)
    {
        this.costumes = costumes;
        this.fallbackNameStrPtr = StringsCache.GetStringPtr("UNUSED (Equipping will break game!)");
    }

    public void Initialize(IStartupScanner scanner, IReloadedHooks hooks)
    {
        scanner.Scan("Get Item Name Function", "bb 41 38 c9 8c cc cc cc cc cc cc cc cc cc cc", result =>
        {
            this.getItemNameHook = hooks.CreateHook<GetItemNameFunction>(this.GetItemName, result + 15).Activate();
        });
    }

    private nint GetItemName(int itemId)
    {
        if (VirtualOutfitsSection.IsOutfit(itemId))
        {
            if (this.costumes.TryGetCostume(itemId, out var costume))
            {
                // Get name from config.
                if (costume.Config.Name != null)
                {
                    return StringsCache.GetStringPtr(costume.Config.Name);
                }

                // Get name from costume object.
                // Only for mod outfits so NAME.TBL edits work.
                if (costume.Name != null && VirtualOutfitsSection.IsModOutfit(itemId))
                {
                    return StringsCache.GetStringPtr(costume.Name);
                }

                return this.getItemNameHook!.OriginalFunction(itemId);
            }

            return this.fallbackNameStrPtr;
        }

        return this.getItemNameHook!.OriginalFunction(itemId);
    }
}
