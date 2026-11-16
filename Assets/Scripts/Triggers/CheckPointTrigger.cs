using UnityEngine;

public class CheckPointTrigger : MonoBehaviour
{
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private ParticleSystem checkpointParticles;
    private bool isActivated = false;

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (((1 << collision.gameObject.layer) & playerLayer) == 0) return;

        if (isActivated) return;
        isActivated = true;

        CheckPointManager.Instance.SetCheckPoint(transform.position);

        // Play the effect
        if (checkpointParticles != null)
            checkpointParticles.Play();

        // Show the UI Toast
        // UIManager.Instance?.ShowToast("Checkpoint Reached!");
    }
}