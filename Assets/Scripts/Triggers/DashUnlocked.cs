using UnityEngine;

public class DashUnlocked : MonoBehaviour
{
    [SerializeField] PlayerMovement playerMovement;
    private void OnTriggerEnter2D(Collider2D other)
    {
        playerMovement.DashUnlock();
        Destroy(gameObject);
    }
}
