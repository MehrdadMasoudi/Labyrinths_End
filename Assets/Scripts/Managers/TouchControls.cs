using UnityEngine;

public class TouchControlsVisibility : MonoBehaviour
{
    [SerializeField] private GameObject touchControls;   // your touch canvas
    [SerializeField] private bool showInEditor = false;  // tick this to test the touch UI on PC

    private void Awake()
    {
        bool show = Application.isMobilePlatform;
#if UNITY_EDITOR
        show = showInEditor;
#endif
        touchControls.SetActive(show);
    }
}