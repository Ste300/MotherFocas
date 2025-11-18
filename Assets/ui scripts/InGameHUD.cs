using UnityEngine;
using UnityEngine.UI;
using Fusion;

public class InGameHUD : MonoBehaviour
{
    [Header("References")]
    public TeamManager teamManager;
    public NetworkTurnManager networkTurnManager;

    public Text roundText; 
    public Text turnText; 

    [Header("Options")]
    public float refreshInterval = 0.2f;

    int lastRound = -1;
    int lastSlot = -1;
    float timer = 0f;

    // Finds references and forces an initial UI refresh.
    void Start()
    {
        if (teamManager == null)
            teamManager = FindObjectOfType<TeamManager>();
        
        if (networkTurnManager == null)
        {
            networkTurnManager = FindObjectOfType<NetworkTurnManager>();
            if (networkTurnManager == null)
                Debug.LogWarning("[InGameHUD] NetworkTurnManager not found.");
        }
        ForceRefresh();
    }

    // Updates the UI on a timer.
    void Update()
    {
        timer += Time.deltaTime;
        if (timer < refreshInterval) return;
        timer = 0f;
        RefreshIfNeeded();
    }

    // Checks and updates Round and Turn text if they have changed.
    void RefreshIfNeeded()
    {
        if (teamManager != null)
        {
            int r = teamManager.CurrentRound;
            if (r != lastRound)
            {
                if (roundText != null) roundText.text = $"Round: {r}";
                lastRound = r;
            }
        }

        if (networkTurnManager != null)
        {
            int s = networkTurnManager.ActiveSlot;
            if (s != lastSlot)
            {
                if (turnText != null)
                {
                    turnText.text = $"Turn: Player {s + 1}";
                }
                lastSlot = s;
            }
        }
    }

    // Forces the UI to refresh on the next frame.
    public void ForceRefresh()
    {
        lastRound = -1;
        lastSlot = -1;
        RefreshIfNeeded();
    }
}