using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    public InputActionAsset inputActions;

    // Action Maps
    private InputActionMap playerActionMap;
    private InputActionMap uiActionMap;

    // Player Actions
    private InputAction moveAction;
    private InputAction attackAction;
    private InputAction interactAction;
    private InputAction pauseAction;
    private InputAction cancelAction;

    public Vector2 MoveInput { get; private set; }
    public bool AttackPressed { get; private set; }
    public bool InteractPressed { get; private set; }


    public Action OnAttackPressed;
    public Action OnInteractPressed;
    public Action OnPausePressed;
    public Action OnCancelPressed;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
