using P5R.CostumeFramework.Configuration;
using P5R.CostumeFramework.Models;

namespace P5R.CostumeFramework.Characters;

public class CharacterAssetsSettings : Dictionary<Character, CharacterAssets>
{
    public CharacterAssetsSettings(Config config)
    {
        this[Character.Joker] = config.Joker_Assets;
        this[Character.Ryuji] = config.Ryuji_Assets;
        this[Character.Morgana] = config.Morgana_Assets;
        this[Character.Ann] = config.Ann_Assets;
        this[Character.Yusuke] = config.Yusuke_Assets;
        this[Character.Makoto] = config.Makoto_Assets;
        this[Character.Haru] = config.Haru_Assets;
        this[Character.Futaba] = config.Futaba_Assets;
        this[Character.Akechi] = config.Akechi_Assets;
        this[Character.Sumire] = config.Sumire_Assets;
    }
}
