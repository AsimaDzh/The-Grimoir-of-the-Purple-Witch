using System.Collections.Generic;
using UnityEngine;


public class SpellManager : MonoBehaviour
{
    [Header("========== Connections ==========")]
    [SerializeField] private PlayerStats playerStats;

    [Header("========== Weapons on player ==========")]
    [SerializeField] private SpellsBase[] spellInstances;
    [SerializeField] private bool[] spellAvailableAtStart;
    [SerializeField] private int defaultSpellIndexInAvailable = 0;

    private List<SpellsBase> _availableSpells = new List<SpellsBase>();
    private int _currentAvailableIndex;
    private SpellsBase _currentSpell;

    public SpellsBase CurrentWeapon => _currentSpell;
    public PlayerStats PlayerStats => playerStats;


    private void Awake()
    {
        if (playerStats == null)
            playerStats = GetComponent<PlayerStats>();

        BuildAvailableWeaponsList();

        if (_availableSpells.Count == 0)
        {
            Debug.LogError("No available spells", this);
            return;
        }

        int startIndex = Mathf.Clamp(
            defaultSpellIndexInAvailable,
            0, _availableSpells.Count - 1);
        _currentAvailableIndex = startIndex;
        EquipByEnableDisable(_availableSpells[startIndex]);
    }


    private void OnEnable()
    {
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnAttackPressed += HandleAttackPressed;
            InputManager.Instance.OnSpellNextSelected += SwitchToNextWeapon;
            InputManager.Instance.OnSpellPrevSelected += SwitchToPrevWeapon;
        }
    }


    private void OnDisable()
    {
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnAttackPressed -= HandleAttackPressed;
            InputManager.Instance.OnSpellNextSelected -= SwitchToNextWeapon;
            InputManager.Instance.OnSpellPrevSelected -= SwitchToPrevWeapon;
        }
    }


    private void BuildAvailableWeaponsList()
    {
        _availableSpells.Clear();
        if (spellInstances == null) return;

        for (int i = 0; i < spellInstances.Length; i++)
        {
            if (spellInstances[i] == null) continue;

            bool available =
                spellAvailableAtStart == null
                || i >= spellAvailableAtStart.Length
                || spellAvailableAtStart[i];

            if (available)
                _availableSpells.Add(spellInstances[i]);
        }
    }


    private void EquipByEnableDisable(SpellsBase spell)
    {
        if (spell == null) return;

        if (spellInstances != null)
        {
            foreach (SpellsBase w in spellInstances)
            {
                if (w != null)
                    w.gameObject.SetActive(w == spell);
            }
        }

        _currentSpell = spell;
        SetupWeapon(_currentSpell);
    }

    private void SwitchToNextWeapon()
    {
        if (_availableSpells.Count == 0) return;
        _currentAvailableIndex = (_currentAvailableIndex + 1) % _availableSpells.Count;
        EquipByEnableDisable(_availableSpells[_currentAvailableIndex]);
    }


    private void SwitchToPrevWeapon()
    {
        if (_availableSpells.Count == 0) return;
        _currentAvailableIndex = (_currentAvailableIndex - 1 + _availableSpells.Count) % _availableSpells.Count;
        EquipByEnableDisable(_availableSpells[_currentAvailableIndex]);
    }


    private void HandleAttackPressed()
    {
        if (!TurnManager.Instance.IsPlayerTurn()) return;

        if (_currentSpell == null)
        {
            Debug.LogWarning("SpellManager: player has no current spell");
            return;
        }

        _currentSpell.Attack();
        TurnManager.Instance.StartEnemyTurn();
    }


    public void UnlockWeaponBySlotIndex(int slotIndex)
    {
        if (spellInstances == null
            || slotIndex < 0
            || slotIndex >= spellInstances.Length) return;
        SpellsBase w = spellInstances[slotIndex];
        if (w != null && !_availableSpells.Contains(w))
            _availableSpells.Add(w);
    }


    private void SetupWeapon(SpellsBase spell)
    {
        if (spell == null) return;
        spell.owner = transform;
        spell.transform.localPosition = Vector3.zero;
        spell.transform.localRotation = Quaternion.identity;
    }
}