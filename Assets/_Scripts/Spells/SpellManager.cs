using System.Collections.Generic;
using UnityEngine;


public class SpellManager : MonoBehaviour
{
    [Header("========== Connections ==========")]
    [SerializeField] private PlayerStats playerStats;

    [Header("========== Spells on player ==========")]
    [SerializeField] private SpellsBase[] spellInstances;
    [SerializeField] private bool[] spellAvailableAtStart;
    [SerializeField] private int defaultSpellIndexInAvailable = 0;

    private List<SpellsBase> _availableSpells = new List<SpellsBase>();
    private SpellsBase _currentSpell;

    public SpellsBase CurrentSpell => _currentSpell;
    public PlayerStats PlayerStats => playerStats;


    private void Awake()
    {
        if (playerStats == null)
            playerStats = GetComponent<PlayerStats>();

        BuildAvailableSpellsList();

        if (_availableSpells.Count == 0)
        {
            Debug.LogError("No available spells", this);
            return;
        }

        int startIndex = Mathf.Clamp(
            defaultSpellIndexInAvailable,
            0, _availableSpells.Count - 1);

        EquipSpell(_availableSpells[startIndex]);
    }


    private void OnEnable()
    {
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnAttackPressed += HandleAttackPressed;
        }
    }


    private void OnDisable()
    {
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnAttackPressed -= HandleAttackPressed;
        }
    }


    private void BuildAvailableSpellsList()
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


    private void EquipSpell(SpellsBase spell)
    {
        if (spell == null) return;

        if (spellInstances != null)
        {
            foreach (SpellsBase s in spellInstances)
            {
                if (s != null)
                    s.gameObject.SetActive(s == spell);
            }
        }

        _currentSpell = spell;
        SetupSpell(_currentSpell);
    }


    private void HandleSpellPressed(int index)
    {
        if (!TurnManager.Instance.IsPlayerTurn()) return;

        if (index < 0 || index >= _availableSpells.Count) return;

        SpellsBase spell = _availableSpells[index];

        if (spell == null)
        {
            Debug.LogWarning("SpellManager: player has no current spell");
            return;
        }

        EquipSpell(spell);

        spell.Attack();

        TurnManager.Instance.StartEnemyTurn();
    }


    public void UnlockSpellBySlotIndex(int slotIndex)
    {
        if (spellInstances == null
            || slotIndex < 0
            || slotIndex >= spellInstances.Length) return;

        SpellsBase s = spellInstances[slotIndex];

        if (s != null && !_availableSpells.Contains(s))
            _availableSpells.Add(s);
    }


    private void SetupSpell(SpellsBase spell)
    {
        if (spell == null) return;

        spell.owner = transform;
        spell.transform.localPosition = Vector3.zero;
        spell.transform.localRotation = Quaternion.identity;
    }
}