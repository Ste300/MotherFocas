using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.IO;

public class PausePanelManager : MonoBehaviour
{
    [Header("Referencias UI")]
    public GameObject pausePanel; // Panel que aparecerá al pausar (root del panel)
    public Button pauseButton;    // Imagen o botón de pausa (abre/cierra)
    public Button exitButton;     // Botón "Salir" dentro del panel

    // opcional: si el panel tiene CanvasGroup puedes usarlo para bloquear raycasts
    public CanvasGroup pauseCanvasGroup;

    private bool isPanelVisible = false;

    void Start()
    {
        // Asegurarnos que no haya duplicados de listeners (si reasignaste script)
        if (pauseButton != null)
        {
            pauseButton.onClick.RemoveListener(TogglePausePanel);
            pauseButton.onClick.AddListener(TogglePausePanel);
        }

        if (exitButton != null)
        {
            exitButton.onClick.RemoveListener(ExitToMainMenu);
            exitButton.onClick.AddListener(ExitToMainMenu);
        }

        // ocultar panel al inicio (si está asignado)
        if (pausePanel != null)
            pausePanel.SetActive(false);

        // si hay CanvasGroup, dejar blocksRaycasts false al inicio
        if (pauseCanvasGroup != null)
            pauseCanvasGroup.blocksRaycasts = false;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            TogglePausePanel();
    }

    // Cambia visibilidad del panel. Si se oculta, se asegura que no bloquee clics.
    public void TogglePausePanel()
    {
        if (pausePanel == null)
        {
            Debug.LogWarning("[PausePanelManager] pausePanel no asignado en el Inspector.");
            return;
        }

        isPanelVisible = !isPanelVisible;
        pausePanel.SetActive(isPanelVisible);

        // Si tienes un CanvasGroup, usar blocksRaycasts para bloquear solo cuando visible
        if (pauseCanvasGroup != null)
            pauseCanvasGroup.blocksRaycasts = isPanelVisible;
    }

    // Cierra explicitamente
    public void ClosePanel()
    {
        if (pausePanel == null) return;
        isPanelVisible = false;
        pausePanel.SetActive(false);
        if (pauseCanvasGroup != null) pauseCanvasGroup.blocksRaycasts = false;
    }

    // Salir al menú principal — asegura que el panel quede desactivado antes de cambiar UI/escena
    public void ExitToMainMenu()
    {
        Debug.Log("🏁 ExitToMainMenu llamado.");

        // 1) ocultar panel de pausa y desactivar su bloqueo de raycast inmediatamente
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
            isPanelVisible = false;
        }
        if (pauseCanvasGroup != null)
            pauseCanvasGroup.blocksRaycasts = false;

        // 2) resetear estado del TeamManager si existe
        TeamManager tm = FindObjectOfType<TeamManager>();
        if (tm != null)
        {
            tm.ResetToInitialState();
            tm.PrepareNewRound();
        }

        // 3) Intentar volver al menú via UIManager (preferido)
        UIManager ui = FindObjectOfType<UIManager>();
        if (ui != null)
        {
            ui.ShowMenuPrincipal();
            Debug.Log("[PausePanelManager] Volviendo al menú principal vía UIManager.");
            return;
        }

        // 4) Si no hay UIManager, intentar cargar la escena 'MenuPrincipal' desde Build Settings
        string targetSceneName = "MenuPrincipal";
        if (IsSceneInBuildSettings(targetSceneName))
        {
            Debug.Log($"[PausePanelManager] Cargando escena '{targetSceneName}' desde Build Settings.");
            SceneManager.LoadScene(targetSceneName);
            return;
        }

        Debug.LogError($"[PausePanelManager] No se encontró UIManager ni la escena '{targetSceneName}' en Build Settings.\n" +
            "Si usas paneles en la misma escena: crea un UIManager y define ShowMenuPrincipal().\n" +
            "Si quieres cargar otra escena: agrega 'MenuPrincipal' a File→Build Settings.");
    }

    bool IsSceneInBuildSettings(string sceneName)
    {
        int count = SceneManager.sceneCountInBuildSettings;
        for (int i = 0; i < count; i++)
        {
            string path = SceneUtility.GetScenePathByBuildIndex(i);
            if (string.IsNullOrEmpty(path)) continue;
            string name = Path.GetFileNameWithoutExtension(path);
            if (string.Equals(name, sceneName, System.StringComparison.OrdinalIgnoreCase))
                return true;
        }
        return false;
    }
}
