using UnityEngine;
using Unity.Cinemachine;
public class RoomCameraController : MonoBehaviour
{
    public static RoomCameraController Instance { get; private set; }
    [Header("Starting Camera")]
    [SerializeField] private CinemachineCamera startingCamera;

    [Header("Camera Priorities")]
    [SerializeField] private int activePriority = 20;
    [SerializeField] private int inactivePriority = 0;

    public CinemachineCamera CurrentCamera { get; private set; }

    private void Awake()
    {

        Instance = this;

        CurrentCamera = startingCamera;
        CurrentCamera.Priority = activePriority;
    }

    public void SwitchCamera(CinemachineCamera newCamera)
    {
        if (newCamera == null || newCamera == CurrentCamera)
            return;

        if (CurrentCamera != null)
            CurrentCamera.Priority = inactivePriority;

        CurrentCamera = newCamera;
        CurrentCamera.Priority = activePriority;

        // Refresh CameraManager's cached Position Composer
        if (CameraManager.instance != null)
        {
            CameraManager.instance.RefreshPositionComposer(CurrentCamera);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}
