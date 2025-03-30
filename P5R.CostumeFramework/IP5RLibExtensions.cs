using P5R.CostumeFramework.Models;
using p5rpc.lib.interfaces;

namespace P5R.CostumeFramework;

internal static class IP5RLibExtensions
{
    public static int GET_EQUIP(this IP5RLib lib, Character character, EquipSlot slot)
        => lib.FlowCaller.GET_EQUIP((int)character, (int)slot);
    public static void SET_EQUIP(this IP5RLib lib, Character character, EquipSlot slot, int itemId)
        => lib.FlowCaller.SET_EQUIP((int)character, (int)slot, itemId);

    public static int GET_PARTY(this IP5RLib lib, int partySlot)
        => lib.FlowCaller.GET_PARTY(partySlot);
}
