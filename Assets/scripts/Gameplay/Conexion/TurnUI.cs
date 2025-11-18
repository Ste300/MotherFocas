using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Fusion;

public class TurnUI : MonoBehaviour
{
    public NetworkTurnManager turnMgr;
    public TextMeshProUGUI tmpText;
    public Text legacyText;

    PlayerNetworkInfo _localInfo;

    // Updates the turn text every frame (LateUpdate for UI).
    void LateUpdate()
    {
        if (turnMgr == null) return;

        string msg; 

        if (turnMgr.IsGameFinished)
        {
            msg = "Game Over!";
            if (turnMgr.WinnerSlot != -1)
            {
                msg = $"Player {turnMgr.WinnerSlot + 1} Wins!";
            }
        }
        else
        {
            if (_localInfo == null)
            {
                foreach (var inf in FindObjectsOfType<PlayerNetworkInfo>())
                {
                    var no = inf.GetComponent<Fusion.NetworkObject>();
                    if (no != null && no.HasInputAuthority) { _localInfo = inf; break; }
                }
            }

            float t = Mathf.Ceil(turnMgr.GetTimeLeftSeconds());
            int slot = turnMgr.ActiveSlot;

            string who = (_localInfo != null && turnMgr.IsMyTurn(_localInfo.Slot))
                ? "Your Turn"
                : $"Player {slot + 1}'s Turn";

            msg = $"{who}  •  {t:0}s";
        }

        if (tmpText) tmpText.text = msg;
        if (legacyText) legacyText.text = msg;
    }
}