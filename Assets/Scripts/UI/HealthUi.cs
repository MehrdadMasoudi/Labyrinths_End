using UnityEngine;
using UnityEngine.UI;

public class HealthUI : MonoBehaviour
{
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private Image heartPrefab;
    [SerializeField] private Sprite fullSprite;
    [SerializeField] private Sprite emptySprite;

    private Image[] hearts;

    void Start()
    {
        hearts = new Image[playerHealth.MaxHealth];
        for (int i = 0; i < hearts.Length; i++)
            hearts[i] = Instantiate(heartPrefab, transform);

        playerHealth.OnHealthChanged += Refresh;
        Refresh(playerHealth.currentHealth, playerHealth.MaxHealth);
    }

    void OnDestroy()
    {
        if (playerHealth != null) playerHealth.OnHealthChanged -= Refresh;
    }

    private void Refresh(int current, int max)
    {
        for (int i = 0; i < hearts.Length; i++)
            hearts[i].sprite = i < current ? fullSprite : emptySprite;
    }
}