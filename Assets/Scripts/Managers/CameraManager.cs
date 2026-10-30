using UnityEngine;

using Unity.Cinemachine;
using System.Collections;
public class CameraManager : MonoBehaviour
{
    public static CameraManager instance;
    [SerializeField] private CinemachineCamera[] allCameras;

    [Header("Controls for lerping the Y Damping during player jump/fall")]
    [SerializeField] private float _fallPanAmount = 0.25f;
    [SerializeField] private float _fallYPanTime = 0.35f;
    public float _fallSpeedYDampingChangeThreshold = -15f;

    public bool IsLerpingYDamping { get; private set; }
    public bool LerpedFromPlayerFalling { get; set; }

    private Coroutine _lerpYPanCoroutine;

    private CinemachineCamera currentCamera;
    private CinemachinePositionComposer _framingTransposer;
    [SerializeField] float _normYPanAmount = 2f;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }

        for (int i = 0; i < allCameras.Length; i++)
        {
            if (allCameras[i].enabled)
            {
                // set the current active camera
                currentCamera = allCameras[i];

                // set the framing transposer
                _framingTransposer = currentCamera.GetComponent<CinemachinePositionComposer>();
            }
        }
        _normYPanAmount = _framingTransposer.Damping.y;
    }

    #region Lerp the Y Damping
    public void LerpYDamping(bool isPlayerFalling)
    {
        _lerpYPanCoroutine = StartCoroutine(LerpYAction(isPlayerFalling));
    }

    private IEnumerator LerpYAction(bool isPlayerFalling)
    {
        IsLerpingYDamping = true;

        // grab the starting damping amount
        float startDampAmount = _framingTransposer.Damping.y;
        float endDampAmount = 0f;

        // determine the end damping amount
        if (isPlayerFalling)
        {
            endDampAmount = _fallPanAmount;
            LerpedFromPlayerFalling = true;
        }
        else
        {
            endDampAmount = _normYPanAmount; // Note: _normYPanAmount is referenced but not declared in the visible code
        }

        // lerp the pan amount
        float elapsedTime = 0f;
        while (elapsedTime < _fallYPanTime)
        {
            elapsedTime += Time.deltaTime;

            float lerpedPanAmount = Mathf.Lerp(startDampAmount, endDampAmount, (elapsedTime / _fallYPanTime));
            Vector3 damping = _framingTransposer.Damping;
            damping.y = lerpedPanAmount;
            _framingTransposer.Damping = damping;

            yield return null;
        }

        IsLerpingYDamping = false;
    }
    #endregion
}