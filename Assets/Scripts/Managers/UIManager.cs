using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Platform")]
    [SerializeField] private bool forceMobileInEditor = false;

    [Header("Persistent Hints")]
    [FormerlySerializedAs("hintPanel")][SerializeField] private GameObject pcHintPanel;
    [SerializeField] private GameObject mobileHintPanel;
    [SerializeField] private TextMeshProUGUI pcHintText;
    [SerializeField] private TextMeshProUGUI mobileHintText;

    [Header("Temporary Toasts")]
    [FormerlySerializedAs("toastPanel")][SerializeField] private GameObject pcToastPanel;
    [SerializeField] private GameObject mobileToastPanel;
    [SerializeField] private TextMeshProUGUI pcToastText;
    [SerializeField] private TextMeshProUGUI mobileToastText;
    [SerializeField] private float toastDuration = 2f;

    public bool IsMobile { get; private set; }

    private Coroutine toastRoutine;
    private Component hintOwner;

    private GameObject activeHintPanel, activeToastPanel;
    private TextMeshProUGUI activeHintText, activeToastText;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;

#if UNITY_EDITOR
        IsMobile = Application.isMobilePlatform || forceMobileInEditor;
#else
        IsMobile = Application.isMobilePlatform;
#endif

        activeHintPanel = Pick(IsMobile, mobileHintPanel, pcHintPanel);
        activeToastPanel = Pick(IsMobile, mobileToastPanel, pcToastPanel);
        activeHintText = Pick(IsMobile, mobileHintText, pcHintText);
        activeToastText = Pick(IsMobile, mobileToastText, pcToastText);

        HideAllHints();
        HideAllToasts();
    }

    private void Start()
    {
        if (hintOwner == null) HideAllHints();
        if (toastRoutine == null) HideAllToasts();
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private void OnDisable()
    {
        toastRoutine = null;
    }

    // Platform's object, falling back to the other one if it's not assigned
    private static T Pick<T>(bool mobile, T mobileObj, T pcObj) where T : Object
    {
        if (mobile) return mobileObj != null ? mobileObj : pcObj;
        return pcObj != null ? pcObj : mobileObj;
    }

    private static void Off(Component c) { if (c != null) c.gameObject.SetActive(false); }
    private static void Off(GameObject g) { if (g != null) g.SetActive(false); }

    private void HideAllHints()
    {
        Off(pcHintPanel);
        Off(mobileHintPanel);
        Off(pcHintText);
        Off(mobileHintText);
    }

    private void HideAllToasts()
    {
        Off(pcToastPanel);
        Off(mobileToastPanel);
        Off(pcToastText);
        Off(mobileToastText);
    }

    // ---------- Hints ----------
    public void ShowHint(string text, Component owner = null)
    {
        hintOwner = owner;
        HideAllHints(); // make sure the other platform's panel is off
        if (activeHintPanel != null) activeHintPanel.SetActive(true);
        if (activeHintText != null)
        {
            activeHintText.gameObject.SetActive(true);
            activeHintText.text = text;
        }
    }

    public void HideHint(Component owner = null)
    {
        if (owner != null && hintOwner != owner) return;
        hintOwner = null;
        HideAllHints();
    }

    // ---------- Toasts ----------
    public void ShowToast(string text)
    {
        if (!isActiveAndEnabled)
        {
            Debug.LogWarning("UIManager is on an inactive object. Don't put it on or under the panels.", this);
            return;
        }
        if (toastRoutine != null) StopCoroutine(toastRoutine);
        toastRoutine = StartCoroutine(ToastRoutine(text));
    }

    private IEnumerator ToastRoutine(string text)
    {
        HideAllToasts();
        if (activeToastPanel != null) activeToastPanel.SetActive(true);
        if (activeToastText != null)
        {
            activeToastText.gameObject.SetActive(true);
            activeToastText.text = text;
        }

        yield return new WaitForSecondsRealtime(toastDuration);

        HideAllToasts();
        toastRoutine = null;
    }
}