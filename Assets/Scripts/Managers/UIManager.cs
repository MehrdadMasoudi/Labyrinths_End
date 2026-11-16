using System.Collections;
using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("persistent Hints")]
    [SerializeField] private GameObject hintPanel;
    [SerializeField] private TextMeshProUGUI hintText;

    [Header("temporary Toasts (Unlocks/Checkpoints)")]
    [SerializeField] private GameObject toastPanel;
    [SerializeField] private TextMeshProUGUI toastText;
    [SerializeField] private float toastDuration = 2f;

    private Coroutine toastRoutine;

    private void Awake()
    {
        Instance = this;
        if (hintPanel != null) hintPanel.SetActive(false);
        if (toastPanel != null) toastPanel.SetActive(false);
    }

    // --- HINTS (Stay on screen until hidden) ---
    public void ShowHint(string text)
    {
        if (hintPanel != null) hintPanel.SetActive(true);
        if (hintText != null) hintText.text = text;
    }

    public void HideHint()
    {
        if (hintPanel != null) hintPanel.SetActive(false);
    }

    // --- TOASTS (Auto-hide after X seconds) ---
    public void ShowToast(string text)
    {
        if (toastRoutine != null) StopCoroutine(toastRoutine);
        toastRoutine = StartCoroutine(ToastRoutine(text));
    }

    private IEnumerator ToastRoutine(string text)
    {
        if (toastPanel != null) toastPanel.SetActive(true);
        if (toastText != null) toastText.text = text;

        yield return new WaitForSecondsRealtime(toastDuration);

        if (toastPanel != null) toastPanel.SetActive(false);
        toastRoutine = null;
    }
}