using System.Collections;
using UnityEngine;

public class HitStopManager : MonoBehaviour
{
    public static HitStopManager Instance { get; private set; }

    private Coroutine routine;
    private bool isHitStopping;
    private bool isPaused;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void Freeze(float duration)
    {
        if (duration <= 0f) return;

        isHitStopping = true;

        if (routine != null)
            StopCoroutine(routine);

        ApplyTimeScale();
        routine = StartCoroutine(FreezeRoutine(duration));
    }

    public void SetPaused(bool paused)
    {
        isPaused = paused;
        ApplyTimeScale();
    }

    private IEnumerator FreezeRoutine(float duration)
    {
        yield return new WaitForSecondsRealtime(duration);

        isHitStopping = false;
        routine = null;
        ApplyTimeScale();
    }

    private void ApplyTimeScale()
    {
        // A hit-stop ending must never resume a game that is still paused.
        Time.timeScale = (isPaused || isHitStopping) ? 0f : 1f;
    }

    private void OnDestroy()
    {
        if (Instance != this) return;

        Instance = null;
        Time.timeScale = 1f;
    }
}
