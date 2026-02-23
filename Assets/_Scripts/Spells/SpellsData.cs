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

    [Header("========== General Spells Data ==========")]
    public string spellName = "New Spell";
    public SpellType spellType = SpellType.Single;
    public Sprite spellIcon;

    [Header("========== Spell Stats ==========")]
    [Min(0f)] public float damage = 10f;
    [Min(0.1f)] public float attackSpeed = 1f;
    [Min(0f)] public float range = 2f;
    [Min(0f)] public float knockbackForce = 0f;
    [Min(1f)] public float manaCost = 5f;
    [Min(0.1f)] public float cooldownTime = 1f;

    [Header("========== Visual & Audio Effects ==========")]
    public AudioClip castSound;
    public ParticleSystem castEffect;
    public GameObject projectilePrefab;
}
