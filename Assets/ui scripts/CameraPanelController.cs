using UnityEngine;

public class CameraSwitcher : MonoBehaviour
{
    [Header("Cameras")]
    public Camera camMenu;
    public Camera camGameplay;

    [Header("UI Panels (Must match UIManager)")]
    public GameObject mainMenuPanel;
    public GameObject inGameHUDPanel;

    // Starts with the menu camera active.
    void Start()
    {
        camMenu.gameObject.SetActive(true);
        camGameplay.gameObject.SetActive(false);
    }

    // Switches the active camera based on which UI panel is visible.
    void Update()
    {
        if (mainMenuPanel.activeSelf)
        {
            camMenu.gameObject.SetActive(true);
            camGameplay.gameObject.SetActive(false);
        }
        else if (inGameHUDPanel.activeSelf)
        {
            camMenu.gameObject.SetActive(false);
            camGameplay.gameObject.SetActive(true);
        }
    }
}