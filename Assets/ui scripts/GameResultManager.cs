using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Fusion;
using System.Linq;

[DisallowMultipleComponent]
public class GameResultManager : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject panelVictory;
    public GameObject panelDefeat;
    public GameObject panelDraw;

    private NetworkTurnManager _networkTurnManager;
    private PlayerNetworkInfo _myInfo;
    bool gameEnded = false;

    // Hides all result panels on start.
    void Start()
    {
        HideAllPanels();
    }

    // Polls the network state to check if the game has ended.
    void Update()
    {
        if (gameEnded) return;

        if (_networkTurnManager == null)
        {
            _networkTurnManager = FindObjectOfType<NetworkTurnManager>();
            if (_networkTurnManager == null) return;
        }

        if (_myInfo == null)
        {
            var bridge = FindObjectsOfType<PlayerNetworkBridge>()
                .FirstOrDefault(p => p.HasInputAuthority);
            
            if (bridge != null)
            {
                _myInfo = bridge.GetComponent<PlayerNetworkInfo>();
            }
            if (_myInfo == null) return;
        }

        if (_networkTurnManager.IsGameFinished)
        {
            gameEnded = true;
            ShowResult(_networkTurnManager.WinnerSlot, _myInfo.Slot);
        }
    }

    // Shows the correct panel (Victory, Defeat, Draw) based on the result.
    void ShowResult(int winnerSlot, int mySlot)
    {
        HideAllPanels();

        if (winnerSlot == -1)
        {
            if (panelDraw != null) panelDraw.SetActive(true);
            else if (panelDefeat != null) panelDefeat.SetActive(true);
        }
        else if (winnerSlot == mySlot)
        {
            if (panelVictory != null) panelVictory.SetActive(true);
        }
        else
        {
            if (panelDefeat != null) panelDefeat.SetActive(true);
        }
    }

    // Helper function to deactivate all result panels.
    void HideAllPanels()
    {
        if (panelVictory != null) panelVictory.SetActive(false);
        if (panelDefeat != null) panelDefeat.SetActive(false);
        if (panelDraw != null) panelDraw.SetActive(false);
    }
}