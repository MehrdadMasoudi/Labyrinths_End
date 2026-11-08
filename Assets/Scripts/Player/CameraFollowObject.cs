using System.Collections;
using UnityEngine;

public class CameraFollowObject : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform playerTransform;
    [Header("Flip Rotation Stats")]
    [SerializeField] private float flipRotationTime = 0.5f;

    private Coroutine turnCoroutine;
    private PlayerMovement player;
    private bool lastFacingRight;

    private void Awake()
    {
        player = playerTransform.GetComponent<PlayerMovement>();
        lastFacingRight = player.IsFacingRight;
    }

    private void Update()
    {
        transform.position = playerTransform.position;

        // start a turn only when the facing direction actually changes
        bool facingRight = player.IsFacingRight;
        if (facingRight != lastFacingRight)
        {
            lastFacingRight = facingRight;
            if (turnCoroutine != null) StopCoroutine(turnCoroutine);
            turnCoroutine = StartCoroutine(FlipYRotation());
        }
    }

    private IEnumerator FlipYRotation()
    {
        float startRotation = transform.localEulerAngles.y;
        float endRotation = lastFacingRight ? 0f : 180f;
        float elapsed = 0f;

        while (elapsed < flipRotationTime)
        {
            elapsed += Time.deltaTime;
            float y = Mathf.Lerp(startRotation, endRotation, elapsed / flipRotationTime);
            transform.localRotation = Quaternion.Euler(0f, y, 0f);
            yield return null;
        }

        transform.localRotation = Quaternion.Euler(0f, endRotation, 0f);
        turnCoroutine = null;
    }
}