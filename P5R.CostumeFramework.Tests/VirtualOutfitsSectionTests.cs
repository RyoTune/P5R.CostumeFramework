using P5R.CostumeFramework.Models;
using System.Runtime.InteropServices;

namespace P5R.CostumeFramework.Tests;

public class VirtualOutfitsSectionTests
{
    [Fact]
    public unsafe void OutfitsSection_Works()
    {
        var outfits = new VirtualOutfitsSection();
        var buffer = new byte[sizeof(VirtualOutfitsSection)];

        fixed (byte* ptr = buffer)
        {
            Marshal.StructureToPtr(outfits, (nint)ptr, false);
        }

        using var file = File.Create("./test-outfits.tbl");
        using var writer = new BinaryWriter(file);
        writer.Write(buffer);
    }

    [Fact]
    public void OutfitSection_IsModOutfitTest()
    {
        for (int i = 0x7000; i < 0x7000 + 300; i++)
        {
            var outfitId = i - 0x7000;
            if (outfitId == 285)
            {
                Assert.False(VirtualOutfitsSection.IsModOutfit(i));
            }
            else if (outfitId == 286)
            {
                Assert.True(VirtualOutfitsSection.IsModOutfit(i));
            }
        }
    }
}
