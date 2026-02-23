using UnityEngine;


[CreateAssetMenu(
    fileName = "SpellsData",
    menuName = "GameData/SpellsData",
    order = 1)]

public class SpellsData : ScriptableObject
{
    public enum SpellType
    {
        Single = 0,
        Multiple = 1,
        Buff = 2,
        Heal = 3,
        Defensive = 4,
        Poisonous = 5
    }
}
