// BoltInteraction.cs
using UnityEngine;

public class BoltInteraction : MonoBehaviour
{
    // Event to notify manager of wrench interactions
    public System.Action<bool> onWrenchAction; 

    public void ReportWrenchAction(bool success)
    {
        onWrenchAction?.Invoke(success);
    }
}