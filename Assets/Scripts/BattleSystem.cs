using UnityEngine;

public class BattleSystem : MonoBehaviour
{
    private enum BattleState
    {
        Idle,
        InBattle
    }

    [SerializeField] private ColliderTrigger battleTrigger;
    
    private BattleState _state;


    private void Awake()
    {
        _state = BattleState.Idle;
    }


    void Start()
    {
        battleTrigger.OnPlayerEnterTrigger += BattleTrigger_OnPlayerEnterTrigger;
    }


    private void BattleTrigger_OnPlayerEnterTrigger(object sender, System.EventArgs e)
    {
        if (_state == BattleState.Idle)
        {
            StartBattle();
            battleTrigger.OnPlayerEnterTrigger -= BattleTrigger_OnPlayerEnterTrigger;
        } 
    }


    void StartBattle()
    {
        Debug.Log("Battle Started!");
        _state = BattleState.InBattle;
    }
}
