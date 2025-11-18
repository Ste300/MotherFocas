using UnityEngine;

/// Bridges network state to the UI. Hides the waiting panel
/// when the NetworkTurnManager signals GameStarted.
public class MatchUIBridge : MonoBehaviour
{
    public NetworkTurnManager turnMgr;
    public GameObject waitPanel;
    public GameObject hudPanel;
    bool done;

    // Polls the NetworkTurnManager until the game starts.
    void Update()
    {
        if (done || turnMgr == null) return;
        
        if (turnMgr.GameStarted)
        {
            if (waitPanel) waitPanel.SetActive(false);
            if (hudPanel)  hudPanel.SetActive(true);
            done = true;
        }
    }
}