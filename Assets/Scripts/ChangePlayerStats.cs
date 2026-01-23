using UnityEngine;

public class ChangePlayerStats : MonoBehaviour
{
    private void Start()
    {
        PlayerStats.playerHealth = 300f;
        PlayerStats.playerMaxHealth = 300f;
        PlayerStats.playerMana = 150f;
        PlayerStats.spellCards = new string[] { "Fireball", "Ice Shard", "Lightning Bolt" };
        PlayerStats.inventory = new string[] { "Health Potion", "Mana Potion", "Sword" };
    }

    // LevelUp method to increase player stats
    // Inventory
    // SpellCards
    // On damage taken
}
