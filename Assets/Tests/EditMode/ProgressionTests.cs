using NUnit.Framework;
using UnityEngine;

public class ProgressionTests
{
    [Test]
    public void CheckPoint_SavesAndLoadsCorrectPosition()
    {
        GameObject managerGO = new GameObject("CheckPointManager");
        CheckPointManager manager = managerGO.AddComponent<CheckPointManager>();
        InvokePrivateMethod(manager, "Awake"); // To set the Singleton Instance

        Vector3 targetPosition = new Vector3(10f, 5f, 0f);

        // Saving the checkpoint
        manager.SetCheckPoint(targetPosition);

        // making sure it saved
        Vector3 savedPos = manager.GetCheckPoint();
        Assert.AreEqual(targetPosition, savedPos, "Checkpoint did not save the correct pos");

        Object.DestroyImmediate(managerGO);
    }

    private void InvokePrivateMethod(object obj, string methodName)
    {
        var method = obj.GetType().GetMethod(methodName,
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (method != null) method.Invoke(obj, null);
    }
}