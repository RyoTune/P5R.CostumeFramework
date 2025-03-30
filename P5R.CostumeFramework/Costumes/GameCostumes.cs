using P5R.CostumeFramework.Models;
using System.Collections;

namespace P5R.CostumeFramework.Costumes;

internal class GameCostumes : IReadOnlyList<Costume>
{
    private readonly List<Costume> costumes = new();

    public GameCostumes()
    {
        var characters = Enum.GetValues<Character>();
        var akechiDarkSuit = new Costume(Character.Akechi, 0x7000 + 13)
        {
            IsEnabled = true,
            Name = "Dark Suit",
        };

        this.costumes.Add(akechiDarkSuit);

        for (int currentSet = 0; currentSet < VirtualOutfitsSection.GAME_OUTFIT_SETS + VirtualOutfitsSection.MOD_OUTFIT_SETS; currentSet++)
        {
            foreach (var character in characters)
            {
                var itemId = 0x7010 + (currentSet * 10) + (int)character - 1;
                var costume = new Costume(character, itemId);

                if (currentSet < VirtualOutfitsSection.GAME_OUTFIT_SETS)
                {
                    costume.IsEnabled = true;
                    var costumeSet = (CostumeSet)VirtualOutfitsSection.GetOutfitSetId(itemId);

                    // Disable costumes to match game default.
                    if (costume.Character == Character.Morgana)
                    {
                        if (costumeSet == CostumeSet.Winter_Uniform
                            || costumeSet == CostumeSet.Summer_Clothes
                            || costumeSet == CostumeSet.Winter_Clothes
                            || costumeSet == CostumeSet.Summer_Uniform
                            || costumeSet == CostumeSet.Tracksuit
                            || costumeSet == CostumeSet.Swimsuit)
                        {
                            costume.IsEnabled = false;
                        }
                    }

                    if (costume.Character == Character.Akechi)
                    {
                        if (costumeSet == CostumeSet.Summer_Clothes
                            || costumeSet == CostumeSet.Swimsuit)
                        {
                            costume.IsEnabled = false;
                        }
                    }

                    if (costume.Character == Character.Futaba)
                    {
                        if (costumeSet == CostumeSet.Summer_Uniform
                            || costumeSet == CostumeSet.Winter_Uniform)
                        {
                            costume.IsEnabled = false;
                        }
                    }

                    if (costume.Character == Character.Sumire
                        && costumeSet == CostumeSet.Starlight_Outfit)
                    {
                        costume.IsEnabled = false;
                    }

                    if (costumeSet == CostumeSet.Loungewear
                        && costume.Character != Character.Joker)
                    {
                        costume.IsEnabled = false;
                    }

                    if (costumeSet == CostumeSet.Yumizuki_High
                        || costumeSet == CostumeSet.Moonlight_Outfit)
                    {
                        costume.IsEnabled = false;
                    }

                    SetGameCostumeName(costume, costumeSet);
                }

                this.costumes.Add(costume);
            }
        }
    }

    private static void SetGameCostumeName(Costume costume, CostumeSet costumeSet)
    {
        switch (costumeSet)
        {
            case CostumeSet.Default:
                costume.Name = GetDefaultCostumeName(costume);
                break;
            case CostumeSet.Summer_Uniform:
            case CostumeSet.Winter_Uniform:
            case CostumeSet.Summer_Clothes:
            case CostumeSet.Winter_Clothes:
            case CostumeSet.Tracksuit:
            case CostumeSet.Swimsuit:
            case CostumeSet.Loungewear:
            case CostumeSet.Gekkoukan_High:
            case CostumeSet.Yasogami_High:
            case CostumeSet.Karukozaka_High:
            case CostumeSet.Christmas_Outfit:
            case CostumeSet.Dancewear:
            case CostumeSet.Seven_Sisters_High:
            case CostumeSet.Shadow_Ops_Uniform:
            case CostumeSet.Samurai_Garb:
            case CostumeSet.Starlight_Outfit:
            case CostumeSet.Moonlight_Outfit:
            case CostumeSet.Ultramarine_Outfit:
            case CostumeSet.Featherman_Suit:
            case CostumeSet.Demonica_Head:
            case CostumeSet.Demonica_Suit:
            case CostumeSet.New_Cinema_Outfit:
                costume.Name = costumeSet.ToString().Replace('_', ' ');
                break;
            case CostumeSet.St_Hermelin_High:
                costume.Name = "St. Hermelin High";
                break;
            case CostumeSet.Catherine:
                costume.Name = GetCatherineName(costume);
                break;
            case CostumeSet.Butler_Suit:
                costume.Name = GetButlerName(costume);
                break;
            case CostumeSet.Yumizuki_High:
                costume.Name = GetYumizukName(costume);
                break;
            default:
                throw new Exception($"Unknown game costume set {costumeSet}.");
        }

        if (costume.Character == Character.Morgana
            && GetMorganaUniqueName(costumeSet) is string morganaName)
        {
            costume.Name = morganaName;
        }

        if (costume.Character == Character.Akechi
            && GetAkechiUniqueName(costumeSet) is string akechiName)
        {
            costume.Name = akechiName;
        }
    }

    private static string? GetAkechiUniqueName(CostumeSet costumeSet)
    {
        return costumeSet switch
        {
            CostumeSet.Seven_Sisters_High => "Kasugayama High",
            CostumeSet.Karukozaka_High => "Hazama's Uniform",
            _ => null,
        };
    }

    private static string? GetMorganaUniqueName(CostumeSet costumeSet)
    {
        return costumeSet switch
        {
            CostumeSet.Gekkoukan_High => "Aigis Costume",
            CostumeSet.Yasogami_High => "Teddie Costume",
            CostumeSet.St_Hermelin_High => "Trish Costume v1",
            CostumeSet.Seven_Sisters_High => "Trish Costume v2",
            CostumeSet.Samurai_Garb => "Burroughs Costume",
            CostumeSet.Ultramarine_Outfit => "Long Nose Outfit",
            _ => null,
        };
    }

    private static string GetYumizukName(Costume costume)
    {
        return costume.Character switch
        {
            Character.Joker => "Yumizuki High",
            Character.Ryuji => "Yumizuki High",
            Character.Morgana => "Gouto Costume",
            Character.Ann => "Ouran High",
            Character.Yusuke => "Yumizuki High",
            Character.Makoto => "Ouran High",
            Character.Haru => "Ouran High",
            Character.Futaba => "Ouran High",
            Character.Akechi => "Imperial Uniform",
            Character.Sumire => "Ouran High",
            _ => throw new Exception(),
        };
    }

    private static string GetButlerName(Costume costume)
    {
        return costume.Character switch
        {
            Character.Joker
            or Character.Ryuji
            or Character.Yusuke
            or Character.Akechi => "Butler Suit",
            _ => "Maid Uniform",
        };
    }

    private static string GetCatherineName(Costume costume)
    {
        return costume.Character switch
        {
            Character.Joker => "Vincent's Outfit",
            Character.Ryuji => "Orlando's Fashion",
            Character.Morgana => "Stray Sheep Suit",
            Character.Ann => "Catherine's Cami",
            Character.Yusuke => "Johnny's Coat",
            Character.Makoto => "Katherine's Outfit",
            Character.Haru => "Erica's Uniform",
            Character.Futaba => "Toby's Overalls",
            Character.Akechi => "Boss's Suit",
            Character.Sumire => "Rin's One Piece",
            _ => throw new Exception(),
        };
    }

    private static string GetDefaultCostumeName(Costume costume)
    {
        return costume.Character switch
        {
            Character.Joker => "Phantom Suit",
            Character.Ryuji => "Pirate Armor",
            Character.Morgana => "Morgana Classic",
            Character.Ann => "Red Latex Suit",
            Character.Yusuke => "Outlaw's Attire",
            Character.Makoto => "Metal Rider",
            Character.Haru => "Musketeer Suit",
            Character.Futaba => "Cyber Gear",
            Character.Akechi => "Prince Suit",
            Character.Sumire => "Black Leotard",
            _ => throw new Exception()
        };
    }

    public int Count => this.costumes.Count;

    public Costume this[int index] => this.costumes[index];

    public IEnumerator<Costume> GetEnumerator() => this.costumes.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => this.costumes.GetEnumerator();
}
