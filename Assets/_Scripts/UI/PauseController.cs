using UnityEngine;
using UnityEngine.UI;


public class PauseController : MonoBehaviour
{
    [SerializeField] private GameObject pauseMenuUI;
    [SerializeField] private Button buttonResume;
    [SerializeField] private Button buttonMainMenu;


    private void OnEnable()
    {
        if (EventBus.Instance != null)
        {
            EventBus.Instance.OnGamePaused += ShowPausePanel;
            EventBus.Instance.OnGameResumed += HidePauseMenu;
        }
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnPausePressed += HandlePausePressed;
            InputManager.Instance.OnCancelPressed += HandleCancelPressed;
        }
    }


    private void OnDisable()
    {
        if (EventBus.Instance != null)
        {
            EventBus.Instance.OnGamePaused -= ShowPausePanel;
            EventBus.Instance.OnGameResumed -= HidePauseMenu;
        }
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnPausePressed -= HandlePausePressed;
            InputManager.Instance.OnCancelPressed -= HandleCancelPressed;
        }
    }


    private void Start()
    {
        buttonResume.onClick.AddListener(OnResumeClicked);
        buttonMainMenu.onClick.AddListener(OnMainMenuClicked);
    }


    void ShowPausePanel()
    {
        if (pauseMenuUI != null)
            pauseMenuUI.SetActive(true);
    }

    
    void HidePauseMenu()
    {
        if (pauseMenuUI != null)
            pauseMenuUI.SetActive(false);
    }


    void OnResumeClicked()
    {
        if (pauseMenuUI != null)
            GameManager.Instance.Resume();
    }


    void OnMainMenuClicked()
    {
        if (pauseMenuUI != null)
            GameManager.Instance.GoToMenu();
    }


    private void HandlePausePressed()
    {
        if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.Playing)
            GameManager.Instance.Pause();
    }


    private void HandleCancelPressed()
    {
        if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.Paused)
            GameManager.Instance.Resume();
    }
}
