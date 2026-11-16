using UnityEngine;
using Unity.Cinemachine;
[RequireComponent(typeof(Collider2D))]
public class CameraRoomTrigger : MonoBehaviour
{
    [SerializeField] private CinemachineCamera roomCamera;
    [SerializeField] private LayerMask playerLayer;
    private void Awake()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (((1 << other.gameObject.layer) & playerLayer) == 0) return;

        if (RoomCameraController.Instance == null)
        {
            Debug.LogWarning("RoomCameraController instance not found.");
            return;
        }

        RoomCameraController.Instance.SwitchCamera(roomCamera);
    }
}