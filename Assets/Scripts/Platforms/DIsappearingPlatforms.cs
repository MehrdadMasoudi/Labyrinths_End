using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class DisappearingPlatform : MonoBehaviour
{
    [SerializeField] private float visibleDuration = 2f;
    [SerializeField] private float invisibleDuration = 2f;
    [SerializeField] private float fadeDuration = 0.5f;
    [SerializeField] private float maxStartOffset = 2f;
    [SerializeField] private bool syncWithGroup = false;
    [SerializeField] private int groupId = 0;
    [SerializeField] private float durationJitter = 0.2f;
    private SpriteRenderer spriteRenderer;
    private Collider2D platformCollider;

    private float myVisible;
    private float myInvisible;
    private float myFade;

    private static readonly Dictionary<int, float> groupOffsets = new Dictionary<int, float>();

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        platformCollider = GetComponent<Collider2D>();
    }

    private void Start()
    {
        float j = 1f + Random.Range(-durationJitter, durationJitter);
        myVisible = visibleDuration * j;
        myInvisible = invisibleDuration * j;
        myFade = fadeDuration * j;

        StartCoroutine(CycleLoop());
    }

    private IEnumerator CycleLoop()
    {
        float offset = syncWithGroup ? GetGroupOffset(groupId) : Random.Range(0f, maxStartOffset);
        yield return new WaitForSeconds(offset);

        while (true)
        {
            yield return new WaitForSeconds(myVisible - myFade);
            yield return FadeTo(0f);
            platformCollider.enabled = false;

            yield return new WaitForSeconds(myInvisible);

            platformCollider.enabled = true;
            yield return FadeTo(1f);
        }
    }

    private float GetGroupOffset(int id)
    {
        if (!groupOffsets.ContainsKey(id))
            groupOffsets[id] = Random.Range(0f, maxStartOffset);
        return groupOffsets[id];
    }

    private IEnumerator FadeTo(float targetAlpha)
    {
        float startAlpha = spriteRenderer.color.a;
        float elapsed = 0f;
        Color color = spriteRenderer.color;

        while (elapsed < myFade)
        {
            elapsed += Time.deltaTime;
            color.a = Mathf.Lerp(startAlpha, targetAlpha, elapsed / myFade);
            spriteRenderer.color = color;
            yield return null;
        }

        color.a = targetAlpha;
        spriteRenderer.color = color;
    }
}