using UnityEngine;

public abstract class SpellsBase : MonoBehaviour
{
    [Header("========== General Spells Data ==========")]
    public SpellsData spellsData;

    [Header("========== Spell Stats ==========")]
    public Transform owner;
    protected float nextAttackTime = 0f;

    public float Damage => spellsData != null ? spellsData.damage : 0f;
    public float Range => spellsData != null ? spellsData.range : 0f;
    public float AttackSpeed => spellsData != null ? spellsData.attackSpeed : 0f;


    public virtual bool CanAttack()
    {
        if (spellsData == null)
        {
            Debug.LogWarning("SpellsData is not assigned.");
            return false;
        }

        return Time.time >= nextAttackTime;
    }


    protected void StartAttackCooldown()
    {
        float cooldown = AttackSpeed > 0f ? (1f / AttackSpeed) : 0.5f;
        nextAttackTime = Time.time + cooldown;
    }


    public abstract void Attack();
}
