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


    private void Start()
    {
        InitializeInputSystem();
    }


    private void Update()
    {
        // Continuously update movement input
        MoveInput = moveAction != null ? moveAction.ReadValue<Vector2>() : Vector2.zero;
    }


    private void InitializeInputSystem()
    {
        if (inputActions == null)
        {
            Debug.LogError("InputActionAsset is not assigned in InputManager.");
            return;
        }

        playerActionMap = inputActions.FindActionMap("Player");
        uiActionMap = inputActions.FindActionMap("UI");

        if (playerActionMap == null || uiActionMap == null)
        {
            Debug.LogError("Action Maps not found in InputActionAsset.");
            return;
        }
        moveAction = playerActionMap.FindAction("Move");
        attackAction = playerActionMap.FindAction("Attack");
        interactAction = playerActionMap.FindAction("Interact");
        pauseAction = playerActionMap.FindAction("Pause");
        cancelAction = uiActionMap.FindAction("Cancel");

        if (attackAction != null)
            attackAction.performed += OnAttackPerformed;
        if (interactAction != null)
            interactAction.performed += OnInteractPerformed;
        if (pauseAction != null)
            pauseAction.performed += OnPausePerformed;
        if (cancelAction != null)
            cancelAction.performed += OnCancelPerformed;

        EnablePlayerInput();
    }


    private void OnEnable()
    {
        if (inputActions != null) inputActions.Enable();

        if (EventBus.Instance != null)
        {
            EventBus.Instance.OnGamePaused += HandleGamePaused;
            EventBus.Instance.OnGameResumed += HandleGameResumed;
        }
    }


    private void OnDisable()
    {
        if (inputActions != null) inputActions.Disable();

        if (EventBus.Instance != null)
        {
            EventBus.Instance.OnGamePaused -= HandleGamePaused;
            EventBus.Instance.OnGameResumed -= HandleGameResumed;
        }
    }


    private void OnDestroy()
    {
        if (attackAction != null)
            attackAction.performed -= OnAttackPerformed;
        if (interactAction != null)
            interactAction.performed -= OnInteractPerformed;
        if (pauseAction != null)
            pauseAction.performed -= OnPausePerformed;
        if (cancelAction != null)
            cancelAction.performed -= OnCancelPerformed;
    }


    public void EnablePlayerInput()
    {
        if (playerActionMap != null)
            playerActionMap.Enable();
        if (uiActionMap != null)
            uiActionMap.Disable();
    }


    public void EnableUIInput()
    {
        if (playerActionMap != null)
            playerActionMap.Disable();
        if (uiActionMap != null)
            uiActionMap.Enable();
    }


    private void OnAttackPerformed(InputAction.CallbackContext context)
    {
        AttackPressed = true;
        OnAttackPressed?.Invoke();
    }


    private void OnInteractPerformed(InputAction.CallbackContext context)
    {
        InteractPressed = true;
        OnInteractPressed?.Invoke();
    }


    private void OnPausePerformed(InputAction.CallbackContext context)
    {
        OnPausePressed?.Invoke();
    }


    private void OnCancelPerformed(InputAction.CallbackContext context)
    {
        OnCancelPressed?.Invoke();
    }


    private void HandleGamePaused()
    {
        if (playerActionMap != null)
            playerActionMap.Disable();

        Debug.Log("Player input disabled, game paused");
    }


    private void HandleGameResumed()
    {
        if (playerActionMap != null)
            playerActionMap.Enable();
    }


    public Vector2 GetMoveInput()
    {
        return MoveInput;
    }


    public bool IsAttackPressed()
    {
        return AttackPressed;
    }


    public bool IsInteractPressed()
    {
        return InteractPressed;
    }
}
