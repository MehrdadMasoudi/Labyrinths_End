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
    private bool isFacingRight;

    private void Awake()
    {
        player = playerTransform.GetComponent<PlayerMovement>();
        isFacingRight = player.IsFacingRight;
    }

    private void Update()
    {
        // Follow the player's position
        transform.position = playerTransform.position;
    }

    public void CallTurn()
    {
        if (turnCoroutine != null)
            StopCoroutine(turnCoroutine);

        turnCoroutine = StartCoroutine(FlipYRotation());
    }

    private IEnumerator FlipYRotation()
    {
        float startRotation = transform.localEulerAngles.y;
        float endRotationAmount = DetermineEndRotation();
        float elapsedTime = 0f;

        while (elapsedTime < flipRotationTime)
        {
            elapsedTime += Time.deltaTime;

            float yRotation = Mathf.Lerp(
                startRotation,
                endRotationAmount,
                elapsedTime / flipRotationTime
            );

            transform.localRotation = Quaternion.Euler(0f, yRotation, 0f);

            yield return null;
        }

        transform.localRotation =
            Quaternion.Euler(0f, endRotationAmount, 0f);

        turnCoroutine = null;
    }

    private float DetermineEndRotation()
    {
        isFacingRight = !player.IsFacingRight;

        return isFacingRight ? 180f : 0f;
    }
}