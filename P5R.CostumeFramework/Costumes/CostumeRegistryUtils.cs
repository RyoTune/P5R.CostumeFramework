using P5R.CostumeFramework.Models;

namespace P5R.CostumeFramework.Costumes;

internal static class CostumeRegistryUtils
{
    public static Costume[] AddRandomizedCostumes(CostumeFactory factory)
        => Enum.GetValues<Character>()
        .Select(x => factory.CreateFromExisting(x, "Randomized Costumes", 51)!)
        .ToArray();

    public static void AddExistingCostumes(CostumeFactory factory)
    {
        factory.CreateFromExisting(Character.Akechi, "Messy Hair Akechi", 73);
        factory.CreateFromExisting(Character.Akechi, "Ratkechi", 99);

        foreach (var i in Enumerable.Range(21, 7).Concat(Enumerable.Range(71, 14)))
        {
            if (i == 83)
            {
                factory.CreateFromExisting(Character.Sumire, $"Dripsumi", i);
            }
            else
            {
                factory.CreateFromExisting(Character.Sumire, $"Sumire ({i})", i);
            }
        }

        factory.CreateFromExisting(Character.Sumire, "Sumire (Persona Awakening)", 104);
        factory.CreateFromExisting(Character.Ann, "Ann (Thanos Snap)", 108);
        factory.CreateFromExisting(Character.Haru, "Haru (Thanos Snap)", 105);
    }
}
