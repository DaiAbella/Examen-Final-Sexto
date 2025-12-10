using Fusion;
using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("Paneles UI")]
    [SerializeField] private GameObject panelStartButtons;
    [SerializeField] private GameObject panelGameUI;
    [SerializeField] private GameObject panelEndMatch;

    [Header("Referencias UI (TMP)")]
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text winnerText;

    private NetworkRunner runner;

    private void Awake()
    {
        Instance = this;

        // Estado inicial
        ShowStartPanel(true);
        ShowGameCanvas(false);
        ShowEndPanel(false);

        Debug.Log("[UIManager] Awake. Paneles inicializados.");
    }

    private void Start()
    {
        runner = FindObjectOfType<NetworkRunner>();
        if (runner == null) Debug.LogWarning("[UIManager] No se encontró NetworkRunner.");
    }

    private void Update()
    {
        if (GameManager.Instance == null || runner == null) return;

        // ⏱ Actualizar Timer
        if (timerText != null)
        {
            timerText.text = $"Tiempo: {GameManager.Instance.Timer}s";
        }

        // 🏁 Actualizar marcador
        if (scoreText != null)
        {
            string scoresDisplay = "";
            foreach (var player in runner.ActivePlayers)
            {
                int index = player.RawEncoded;
                int score = GameManager.Instance.GetScore(player);
                scoresDisplay += $"Jugador {index}: {score}\n";
            }

            scoreText.text = scoresDisplay;
        }
    }

    // 🔹 Activar/desactivar panel de inicio
    public void ShowStartPanel(bool active)
    {
        if (panelStartButtons != null) panelStartButtons.SetActive(active);
        Debug.Log($"[UIManager] Panel_StartButtons activo: {active}");
    }

    // 🔹 Activar/desactivar panel de juego
    public void ShowGameCanvas(bool active)
    {
        if (panelGameUI != null) panelGameUI.SetActive(active);
        Debug.Log($"[UIManager] Panel_GameUI activo: {active}");
    }

    // 🔹 Activar/desactivar panel de fin de partida
    public void ShowEndPanel(bool active)
    {
        if (panelEndMatch != null) panelEndMatch.SetActive(active);
        Debug.Log($"[UIManager] Panel_EndMatch activo: {active}");
    }

    // 🏆 Mostrar texto de ganador
    public void ShowEndMatch(int winnerIndex, int maxScore)
    {
        if (winnerText != null)
        {
            winnerText.text = $"🏆 Ganador: Jugador {winnerIndex} con {maxScore} puntos";
            Debug.Log($"[UIManager] Texto de ganador actualizado.");
        }
    }

    // 🔘 Botón de salir
    public void OnExitButton()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    // 🔁 Botón de intentar de nuevo
    public void OnRetryButton()
    {
        ShowEndPanel(false);

        if (GameManager.Instance != null && runner != null)
        {
            GameManager.Instance.RegisterRetryVote(runner.LocalPlayer);
        }
    }
}
