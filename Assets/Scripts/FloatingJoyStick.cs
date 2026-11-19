using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.OnScreen;

public class FloatingJoystick : OnScreenControl, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [SerializeField] RectTransform touchArea;   // JoystickArea itself
    [SerializeField] RectTransform background;  // the D-Pad circle
    [SerializeField] RectTransform handle;      // StickHandle
    [SerializeField] float radius = 150f;
    [SerializeField] bool floating = true;      // default, until the player chooses

    [InputControl(layout = "Vector2")]
    [SerializeField] string m_ControlPath;

    protected override string controlPathInternal
    {
        get => m_ControlPath;
        set => m_ControlPath = value;
    }

    int activePointer = int.MinValue;
    Vector2 restPosition;

    public bool IsFloating => floating;

    void Awake()
    {
        if (touchArea == null) touchArea = (RectTransform)transform;
        restPosition = background.anchoredPosition;
        floating = PlayerPrefs.GetInt("FloatingStick", floating ? 1 : 0) == 1;
    }

    public void SetFloating(bool value)
    {
        floating = value;
        PlayerPrefs.SetInt("FloatingStick", value ? 1 : 0);
        if (activePointer == int.MinValue)
            background.anchoredPosition = restPosition;
    }

    public void OnPointerDown(PointerEventData e)
    {
        if (activePointer != int.MinValue) return; // already tracking a finger
        activePointer = e.pointerId;

        if (floating)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                touchArea, e.position, e.pressEventCamera, out var p);
            background.anchoredPosition = p;
        }
        OnDrag(e);
    }

    public void OnDrag(PointerEventData e)
    {
        if (e.pointerId != activePointer) return;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            background, e.position, e.pressEventCamera, out var local);

        Vector2 v = Vector2.ClampMagnitude(local, radius);
        handle.anchoredPosition = v;
        SendValueToControl(v / radius);
    }

    public void OnPointerUp(PointerEventData e)
    {
        if (e.pointerId != activePointer) return;
        activePointer = int.MinValue;

        handle.anchoredPosition = Vector2.zero;
        background.anchoredPosition = restPosition;
        SendValueToControl(Vector2.zero);
    }
}