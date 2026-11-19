using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private GameObject pauseMenuUI;
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    [Header("Touch Controls (mobile only)")]
    [SerializeField] private Button floatingButton;
    [SerializeField] private Image floatingButtonImage; // the button's own Image
    [SerializeField] private TMP_Text floatingLabel;
    [SerializeField] private FloatingJoystick joystick;
    [SerializeField] private Color onColor = new Color(0.2f, 0.75f, 0.3f);  // green
    [SerializeField] private Color offColor = new Color(0.85f, 0.25f, 0.25f); // red

    private bool isPaused = false;

    private static bool IsMobileNow()
    {
        if (UIManager.Instance != null) return UIManager.Instance.IsMobile;
        return Application.isMobilePlatform;
    }

    private void Start()
    {
        Time.timeScale = 1f;
        isPaused = false;

        // The floating option only exists on mobile
        bool mobile = IsMobileNow();
        if (floatingButton != null) floatingButton.gameObject.SetActive(mobile);
        if (mobile) RefreshFloatingButton();

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
        RefreshFloatingButton(); // show the saved state each time the menu opens
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

        // Drop all buttons before the scene dies
        if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);

        SceneManager.LoadScene(mainMenuSceneName);
    }

    // Called by the FloatingButton's On Click
    public void ToggleFloating()
    {
        if (joystick == null) return;
        joystick.SetFloating(!joystick.IsFloating);
        RefreshFloatingButton();
    }

    private void RefreshFloatingButton()
    {
        if (joystick == null) return;

        bool on = joystick.IsFloating;
        if (floatingLabel != null)
            floatingLabel.text = on ? "Floating Joystick: ON" : "Floating Joystick: OFF";
        if (floatingButtonImage != null)
            floatingButtonImage.color = on ? onColor : offColor;
    }
}