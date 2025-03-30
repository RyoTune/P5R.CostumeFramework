using P5R.CostumeFramework.Costumes;
using Reloaded.Hooks.Definitions;
using Reloaded.Hooks.Definitions.X64;
using Reloaded.Memory.SigScan.ReloadedII.Interfaces;

namespace P5R.CostumeFramework.Hooks;

internal unsafe class PartyHook : IGameHook
{
    [Function(CallingConventions.Microsoft)]
    private delegate void UpdateFromMenu(nint dataPtr);
    private IHook<UpdateFromMenu>? updateMenuHook;

    [Function(CallingConventions.Microsoft)]
    private delegate void UpdatePartyPostBattle(nint param1);
    private IHook<UpdatePartyPostBattle>? updateBattleHook;

    private readonly CostumeMusicService costumeMusic;

    public PartyHook(CostumeMusicService costumeMusic)
    {
        this.costumeMusic = costumeMusic;
    }

    public void Initialize(IStartupScanner scanner, IReloadedHooks hooks)
    {
        scanner.Scan("Update from Menu Function", "48 89 5C 24 ?? 48 89 74 24 ?? 57 48 83 EC 40 33 F6", result =>
        {
            this.updateMenuHook = hooks.CreateHook<UpdateFromMenu>(this.UpdateFromMenuImpl, result).Activate();
        });

        scanner.Scan("Update Party Post Battle Function", "45 31 C9 48 8D 15 ?? ?? ?? ?? 45 89 C8", result =>
        {
            this.updateBattleHook = hooks.CreateHook<UpdatePartyPostBattle>(this.UpdatePartyPostBattleImpl, result).Activate();
        });
    }
    private void UpdatePartyPostBattleImpl(nint param1)
    {
        this.updateBattleHook!.OriginalFunction(param1);
        this.costumeMusic.Refresh();
    }

    private void UpdateFromMenuImpl(nint dataPtr)
    {
        this.updateMenuHook!.OriginalFunction(dataPtr);
        this.costumeMusic.Refresh();

        //for (int i = 0; i < 10; i++)
        //{
        //    var character = (Character)(*(ushort*)(dataPtr + 0x1f0 + i * 2));
        //    var inParty = *(byte*)(dataPtr + 0x20b8 + i) != 0 || character == Character.Futaba;

        //    Log.Debug($"{character}: {inParty}");
        //}
    }
}
