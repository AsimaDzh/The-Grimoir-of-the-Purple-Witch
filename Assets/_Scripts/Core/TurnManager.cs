using UnityEngine;

public enum TurnState
{
    PlayerTurn = 0,
    EnemyTurn = 1,
    Busy = 2
}

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance { get; private set; }
    public TurnState CurrentTurn;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(this.gameObject);
    }


    public bool IsPlayerTurn()
    {
        return CurrentTurn == TurnState.PlayerTurn;
    }


    public void StartPlayerTurn()
    {
        CurrentTurn = TurnState.PlayerTurn;
        Debug.Log("Player's Turn Started");
    }


    public void StartEnemyTurn()
    {
        CurrentTurn = TurnState.EnemyTurn;
        Debug.Log("Enemy's Turn Started");
    }
}
