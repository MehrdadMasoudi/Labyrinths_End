using System.Collections;
using UnityEngine;

public class HitStopManager : MonoBehaviour
{
    public static HitStopManager Instance { get; private set; }
    private Coroutine routine;

    private void Awake() => Instance = this;

    public void Freeze(float duration)
    {
        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(FreezeRoutine(duration));
    }

    private IEnumerator FreezeRoutine(float duration)
    {
        float prev = Time.timeScale;
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = prev;
        routine = null;
    }
}