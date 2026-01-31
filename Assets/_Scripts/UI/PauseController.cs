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
    }


    private void OnDisable()
    {
        if (EventBus.Instance != null)
        {
            EventBus.Instance.OnGamePaused -= ShowPausePanel;
            EventBus.Instance.OnGameResumed -= HidePauseMenu;
        }
    }


    private void Start()
    {
        buttonResume.onClick.AddListener(OnResumeClicked);
        buttonMainMenu.onClick.AddListener(OnMainMenuClicked);
    }


    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePaused();
        }
    }


    void TogglePaused()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogWarning("GameManager instance is null. Cannot toggle pause state.");
            return;
        }

        if (GameManager.Instance.CurrentState == GameState.Playing) 
            GameManager.Instance.Pause();
        else if (GameManager.Instance.CurrentState == GameState.Paused)
            GameManager.Instance.Resume();
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
}
