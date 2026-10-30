using UnityEngine;

public class CheckPointManager : MonoBehaviour
{
    public static CheckPointManager Instance { get; private set; }
    public Vector3 currentCheckPoint;

    private void Awake() => Instance = this;
    public void SetCheckPoint(Vector3 position) => currentCheckPoint = position;
    public Vector3 GetCheckPoint() => currentCheckPoint;
}
