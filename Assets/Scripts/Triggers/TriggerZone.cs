using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class TriggerZone : MonoBehaviour
{
    [SerializeField] private LayerMask playerLayer;

    [Header("Messages")]
    [TextArea][SerializeField] private string pcMessage;
    [TextArea][SerializeField] private string mobileMessage;

    [Header("Type of UI")]
    [SerializeField] private bool isToast = true; // true = Toast, false = Hint

    private int collidersInside;

    private void Awake()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsPlayer(other)) return;

        collidersInside++;
        if (collidersInside != 1) return;

        var ui = UIManager.Instance;
        if (ui == null) return;

        string message = GetMessage(ui.IsMobile);

        if (isToast) ui.ShowToast(message);
        else ui.ShowHint(message, this);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!IsPlayer(other)) return;

        collidersInside = Mathf.Max(0, collidersInside - 1);
        if (collidersInside == 0) HideIfHint();
    }

    private void OnDisable()
    {
        if (collidersInside > 0)
        {
            collidersInside = 0;
            HideIfHint();
        }
    }

    private void HideIfHint()
    {
        if (!isToast && UIManager.Instance != null)
            UIManager.Instance.HideHint(this);
    }

    private string GetMessage(bool mobile)
    {
        string message = mobile ? mobileMessage : pcMessage;
        return string.IsNullOrEmpty(message) ? pcMessage : message;
    }

    private bool IsPlayer(Collider2D other)
    {
        return (playerLayer.value & (1 << other.gameObject.layer)) != 0;
    }
}