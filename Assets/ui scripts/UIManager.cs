using UnityEngine;
using UnityEngine.Events;

public class UIManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject mainMenuPanel;
    public GameObject waitingPanel;
    public GameObject inGameHUDPanel; 

    [Header("Events")]
    public UnityEvent OnGameStarted;

    // Show the main menu on application start.
    void Start()
    {
        ShowMainMenu();
    }

    // Activates the main menu panel and hides others.
    public void ShowMainMenu()
    {
        mainMenuPanel.SetActive(true);
        waitingPanel.SetActive(false);
        inGameHUDPanel.SetActive(false);
    }

    // Activates the waiting panel and hides others.
    public void ShowWaitingPanel()
    {
        mainMenuPanel.SetActive(false);
        waitingPanel.SetActive(true);
        inGameHUDPanel.SetActive(false);
    }

    // Activates the in-game HUD panel and fires the OnGameStarted event.
    public void ShowInGameHUD()
    {
        mainMenuPanel.SetActive(false);
        waitingPanel.SetActive(false);
        inGameHUDPanel.SetActive(true);

        OnGameStarted?.Invoke();
    }
}