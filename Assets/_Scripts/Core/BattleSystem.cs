using UnityEngine;
using UnityEngine.SceneManagement;

enum BattleState
{
    Idle = 0,
    InBattle = 1
}

public class BattleSystem : MonoBehaviour
{

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
        SceneManager.LoadScene(SceneNames.BattleScene);
    }
}
