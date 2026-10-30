using UnityEngine;
using Unity.Cinemachine;
[RequireComponent(typeof(Collider2D))]
public class CameraRoomTrigger : MonoBehaviour
{
    [SerializeField] private CinemachineCamera roomCamera;
    private void Awake()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (RoomCameraController.Instance == null)
        {
            Debug.LogWarning("RoomCameraController instance not found.");
            return;
        }

        RoomCameraController.Instance.SwitchCamera(roomCamera);
    }
}