using System;
using UnityEngine;


public class PlayerStats : MonoBehaviour
{
    [Header("========== Player Data ===========")]
    public PlayerData playerData;

    [Header("========== Current Stats (Runtime) ==========")]
    [SerializeField] private float currentHealth;
    [SerializeField] private float currentMana;

    public float CurrentHealth => currentHealth;
    public float CurrentMana => currentMana;

    public event Action<float, float> OnHealthChanged;
    public event Action<float, float> OnManaChanged;
    public event Action OnDeath;


    private void Awake()
    {
        InitializeFromData();
    }


    public void InitializeFromData()
    {
        if (playerData == null)
        {
            Debug.LogError("PlayerStats: PlayerData is null!", this);
            return;
        }
        // Taking stats from PlayerData and applying them to current values, with clamping for safety.
        currentHealth = Mathf.Clamp(playerData.maxHealth, 1f, float.MaxValue);
        currentMana = Mathf.Clamp(playerData.maxMana, 0f, float.MaxValue);

        // Notifying listeners about the initial values
        OnHealthChanged?.Invoke(currentHealth, playerData.maxHealth);
        OnManaChanged?.Invoke(currentMana, playerData.maxMana);
    }


    public void ApplyLevelUpBonuses(float healthBonus, float manaBonus)
    {
        playerData.maxHealth += healthBonus;
        playerData.maxMana += manaBonus;

        currentHealth = playerData.maxHealth;
        currentMana = Mathf.Clamp(currentMana, 0f, playerData.maxMana);

        OnHealthChanged?.Invoke(currentHealth, playerData.maxHealth);
        OnManaChanged?.Invoke(currentMana, playerData.maxMana);
    }


    public void TakeDamage(float amount)
    {
        // Not sensible to take damage with non-positive value or to damage a dead player.
        if (amount <= 0f || currentHealth <= 0f) return;

        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0f, playerData.maxHealth);

        OnHealthChanged?.Invoke(currentHealth, playerData.maxHealth);

        if (currentHealth <= 0f) OnDeath?.Invoke();
    }


    public void Heal(float amount)
    {
        // Not sensible to heal with non-positive value or to heal a dead player.
        if (amount <= 0f || currentHealth <= 0f) return;

        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0f, playerData.maxHealth);

        OnHealthChanged?.Invoke(currentHealth, playerData.maxHealth);
    }


    public void AddMana(float amount)
    {
        // If the amount is zero or negative, there's no point in trying to add mana.
        if (Mathf.Approximately(amount, 0f)) return;

        currentMana += amount;
        currentMana = Mathf.Clamp(currentMana, 0f, playerData.maxMana);

        OnManaChanged?.Invoke(currentMana, playerData.maxMana);
    }
}