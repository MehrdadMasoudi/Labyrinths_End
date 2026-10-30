using UnityEngine;
public class BladeSpinner : MonoBehaviour
{
    [Header("Rotation")][SerializeField] private float rotationSpeed = 720f;
    private void Update()
    {
        transform.Rotate(
            0f,
            0f,
            rotationSpeed * Time.deltaTime
        );
    }
}