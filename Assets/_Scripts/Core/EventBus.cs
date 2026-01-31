using System;
using UnityEngine;

public class EventBus : MonoBehaviour
{
    public static EventBus Instance { get; private set; }
    public event Action OnGamePaused;
    public event Action OnGameResumed;


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


    public void RaiseGamePaused()
    {
        OnGamePaused?.Invoke();
    }


    public void RaiseGameResumed()
    {
        OnGameResumed?.Invoke();
    }
}

