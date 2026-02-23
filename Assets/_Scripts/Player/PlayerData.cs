using UnityEngine;

// This is a ScriptableObject that can be used to store player-related data
[CreateAssetMenu(
    fileName = "PlayerData",
    menuName = "GameData/PlayerData",
    order = 0)]

public class PlayerData : ScriptableObject
{
    [Header("========== Player Stats ==========")]
    [Min(1f)] public float maxHealth = 100f;
    [Min(0f)] public float maxMana = 40f;

    [Header("========== Movement ===========")]
    [Min(0f)] public float moveSpeed = 5f;
}
