using P5R.CostumeFramework.Models;

namespace P5R.CostumeFramework.Tests;

public class ItemTblTests
{
    [Fact]
    public void ItemTbl_AddCostume()
    {
        var originalFile = "./P5REssentials/CPK/DATA.CPK/BATTLE/TABLE/ITEM_original.tbl";
        var itemTbl = new ItemTbl(originalFile);
        itemTbl.AddCostumeSet();
        itemTbl.WriteItemTbl(originalFile.Replace("_original", string.Empty));
    }
}
