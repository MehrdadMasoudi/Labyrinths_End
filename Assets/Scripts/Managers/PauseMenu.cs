using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems; // Required to clear the button focus

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private GameObject pauseMenuUI;
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private bool isPaused = false;

    private void Start()
    {
        // Ensure time is running when scene loads
        Time.timeScale = 1f;
        isPaused = false;
        if (pauseMenuUI != null) pauseMenuUI.SetActive(false);
    }

    // This is called by the PlayerInput component on your Player
    public void OnPause(InputAction.CallbackContext context)
    {
        if (!context.performed) return;

        if (isPaused) ResumeGame();
        else PauseGame();
    }

    public void PauseGame()
    {
        if (pauseMenuUI != null) pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;
    }

    public void ResumeGame()
    {
        if (pauseMenuUI != null) pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;

        // Clear focus so inputs go back to the Player
        if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void QuitToMainMenu()
    {
        Time.timeScale = 1f;

        // CRITICAL FIX: Tell the EventSystem to drop all buttons before the scene dies!
        if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);

        SceneManager.LoadScene(mainMenuSceneName);
    }
}