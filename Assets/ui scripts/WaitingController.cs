using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class WaitingController : MonoBehaviour
{
    [Header("Refs")]
    public UIManager uiManager;

    [Header("UI")]
    public Text countdownText;          // Text en el panel de espera para mostrar cuenta regresiva
    public GameObject simulateButton;   // botón para simular conexión (activar/desactivar)

    [Header("Simulación local (sin red)")]
    [Tooltip("Si true, al entrar a la pantalla de espera comienza el conteo regresivo")]
    public bool autoStartForTesting = true;
    [Tooltip("Segundos de cuenta regresiva antes de auto-start")]
    public float autoStartDelay = 5f;

    private Coroutine countdownCoroutine;

    void OnEnable()
    {
        // Al activarse el panel, lanzar el conteo si está activo
        if (autoStartForTesting)
        {
            StartCountdown(autoStartDelay);
        }
        else
        {
            UpdateCountdownText(-1); // ocultar o mostrar indicador de espera
        }

        if (simulateButton != null)
            simulateButton.SetActive(true);
    }

    void OnDisable()
    {
        StopCountdown();
    }

    // Llamar desde el botón "Simular conectado"
    public void SimulateRemoteConnectButton()
    {
        Debug.Log("[WaitingController] Simulación: jugador remoto conectado (botón).");
        OnRemotePlayerConnected();
    }

    // Hook que debe llamar tu sistema de red real cuando alguien se conecta
    public void OnRemotePlayerConnected()
    {
        Debug.Log("[WaitingController] Remote player connected -> iniciar partida.");
        StopCountdown();
        if (uiManager != null)
            uiManager.ShowDentroDelJuego();
        else
            Debug.LogWarning("WaitingController: uiManager no asignado.");
    }

    // -------------------------
    // Countdown helpers
    // -------------------------
    public void StartCountdown(float seconds)
    {
        StopCountdown();
        countdownCoroutine = StartCoroutine(CountdownCoroutine(seconds));
    }

    public void StopCountdown()
    {
        if (countdownCoroutine != null)
        {
            StopCoroutine(countdownCoroutine);
            countdownCoroutine = null;
        }
        UpdateCountdownText(-1);
    }

    IEnumerator CountdownCoroutine(float seconds)
    {
        float remaining = seconds;
        while (remaining > 0f)
        {
            UpdateCountdownText(Mathf.CeilToInt(remaining));
            yield return new WaitForSeconds(1f);
            remaining -= 1f;
        }

        // tiempo cumplido -> simular conexión automática
        UpdateCountdownText(0);
        yield return new WaitForSeconds(0.25f);
        OnRemotePlayerConnected();
    }

    void UpdateCountdownText(int secondsLeft)
    {
        if (countdownText == null) return;

        if (secondsLeft < 0)
        {
            countdownText.gameObject.SetActive(false);
        }
        else if (secondsLeft == 0)
        {
            countdownText.gameObject.SetActive(true);
            countdownText.text = "Conectando...";
        }
        else
        {
            countdownText.gameObject.SetActive(true);
            countdownText.text = "Esperando jugador... " + secondsLeft + "s";
        }
    }
}
