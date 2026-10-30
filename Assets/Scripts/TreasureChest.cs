using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TreasureChest : MonoBehaviour
{
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private EnemyHealth[] requiredEnemies;   // must all be dead; leave empty for no requirement
    [SerializeField] private GameObject winPanel;             // UI panel, disabled by default
    [SerializeField] private float restartDelay = 3f;
    private bool triggered;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (triggered) return;
        if (((1 << other.gameObject.layer) & playerLayer) == 0) return;

        foreach (var e in requiredEnemies)
            if (e != null && !e.IsDead) return;

        triggered = true;
        StartCoroutine(EndGame());
    }

    private IEnumerator EndGame()
    {
        var pm = FindAnyObjectByType<PlayerMovement>();
        var pa = FindAnyObjectByType<PlayerAttack>();
        var ph = FindAnyObjectByType<PlayerHealth>();

        pm.enabled = false;
        pa.enabled = false;
        ph.IsInvincible = true;
        pm.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;

        if (winPanel != null) winPanel.SetActive(true);
        yield return new WaitForSecondsRealtime(restartDelay);

        Time.timeScale = 1f;   // in case hit-stop was active
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}