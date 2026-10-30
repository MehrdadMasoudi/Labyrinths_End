using System.Collections;
using UnityEngine;

public class PlayerFade : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private float fadeDuration = 3f;
    // Update is called once per frame
    public IEnumerator FadeOut()
    {
        float elapsed = 0f;
        Color color = spriteRenderer.color;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            color.a = 1f - (elapsed / fadeDuration);
            spriteRenderer.color = color;
            yield return null;
        }
        color.a = 0f;
        spriteRenderer.color = color;
    }
    public IEnumerator FadeIn()
    {
        float elapsed = 0f;
        Color color = spriteRenderer.color;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            color.a = elapsed / fadeDuration;
            spriteRenderer.color = color;
            yield return null;
        }
        color.a = 1f;
        spriteRenderer.color = color;
    }
}
