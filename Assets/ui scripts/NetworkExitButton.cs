using UnityEngine;
using Fusion;

public class NetworkExitButton : MonoBehaviour
{
    // Call this from a UI Button (in Victory/Defeat/Pause panels).
    public void ShutdownNetwork()
    {
        Debug.Log("[NetworkExitButton] Requesting NetworkRunner shutdown...");

        var runner = FindObjectOfType<NetworkRunner>();
        
        if (runner != null)
        {
            runner.Shutdown();
        }
        else
        {
            var ui = FindObjectOfType<UIManager>();
            if (ui != null) ui.ShowMainMenu();
        }
    }
}