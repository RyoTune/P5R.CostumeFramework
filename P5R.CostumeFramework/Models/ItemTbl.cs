using System.Runtime.InteropServices;

namespace P5R.CostumeFramework.Models;

public class ItemTbl
{
    private int numCostumeSets = 0;
    private readonly string file;

    public ItemTbl(string file)
    {
        this.file = file;
    }

    public void AddCostumeSet()
    {
        this.numCostumeSets++;
    }

    public void WriteItemTbl(string outputFile)
    {
        using var inFileStream = File.OpenRead(this.file);
        using var outFileStream = File.OpenWrite(outputFile);

        // Copy everything up to end of original outfits
        // to output.
        var buffer = new byte[0x1ce94];
        inFileStream.Read(buffer, 0, buffer.Length);
        outFileStream.Write(buffer);

        // Seek to next section in input.
        inFileStream.Seek(12, SeekOrigin.Current);
        Console.WriteLine(inFileStream.Position);

        // Update outfit size bytes
        //outFileStream.Seek(-9156, SeekOrigin.Current);
        outFileStream.Position = 0x1aad0;
        uint outfitTblSize = (uint)(9152 + (this.numCostumeSets * 10 * 32));
        var outfitSizeBe = outfitTblSize.ToBigEndian();
        outFileStream.Write(BitConverter.GetBytes(outfitSizeBe));
        outFileStream.Seek(9152, SeekOrigin.Current);

        // For each set.
        for (int i = 0; i < this.numCostumeSets; i++)
        {
            // Add new outfit item for each character (10 total).
            for (int j = 0; j < 10; j++)
            {
                var outfitEntry = new OutfitEntry();

                outfitEntry.equippableFlags |= OrderedEquippable[j];

                var entryBufferSize = Marshal.SizeOf<OutfitEntry>();
                var entryBuffer = new byte[entryBufferSize];
                var ptr = Marshal.AllocHGlobal(entryBufferSize);
                Marshal.StructureToPtr(outfitEntry, ptr, false);
                Marshal.Copy(ptr, entryBuffer, 0, entryBufferSize);
                outFileStream.Write(entryBuffer);
                Marshal.FreeHGlobal(ptr);
            }
        }

        // Write padding.
        outFileStream.Write(new byte[16 - ((outfitTblSize + 4) % 16)]);

        while (inFileStream.ReadByte() is var nextByte && nextByte != -1)
        {
            outFileStream.WriteByte((byte)nextByte);
        }
    }

    public static EquippableUsers[] OrderedEquippable = new EquippableUsers[]
    {
        EquippableUsers.Joker,
        EquippableUsers.Ryuji,
        EquippableUsers.Morgana,
        EquippableUsers.Ann,
        EquippableUsers.Yusuke,
        EquippableUsers.Makoto,
        EquippableUsers.Haru,
        EquippableUsers.Futaba,
        EquippableUsers.Akechi,
        EquippableUsers.Sumire,
    };
}

[StructLayout(LayoutKind.Sequential, Size = 32)]
public struct OutfitEntry
{
    public OutfitEntry()
    {
    }

    public uint icon = 4;
    public ushort unknown0 = 0;
    public ushort unknown1 = 0;
    public ushort unknown2 = 0;
    public EquippableUsers equippableFlags = 0;
    public ushort unknown3 = 0;
    public ushort unknown4 = 0;
    public ushort unknown5 = 0;
    public ushort unknown6 = 0;
    public ushort unknown7 = 0;
    public ushort unknown8 = 100;
    public ushort unknown9 = 0;
    public ushort unknown10 = 20;
    public ushort unknown11 = 799;
    public ushort unknown12 = 0;
}

[Flags]
public enum EquippableUsers
    : ushort
{
    Joker = 512,
    Ryuji = 1024,
    Morgana = 2048,
    Ann = 4096,
    Yusuke = 8192,
    Makoto = 16384,
    Haru = 32768,
    Futaba = 1,
    Akechi = 2,
    Sumire = 4,
};