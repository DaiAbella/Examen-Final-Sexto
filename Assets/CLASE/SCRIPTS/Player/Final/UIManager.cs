using Fusion;
using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("Referencias UI (TMP)")]
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private GameObject endMatchPanel;
    [SerializeField] private TMP_Text winnerText;

    private NetworkRunner runner;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void Start()
    {
        runner = FindObjectOfType<NetworkRunner>();
    }

    private void Update()
    {
        if (GameManager.Instance == null || runner == null) return;

        // Verifica que el NetworkObject del GameManager esté inicializado por Fusion
        var gm = GameManager.Instance;
        if (gm.Object == null || !gm.Object.IsValid) return;

        // ⏱ Actualizar Timer
        if (timerText != null)
        {
            int timeLeft = gm.Timer;
            timerText.text = $"Tiempo: {timeLeft}s";
        }

        // 🏁 Actualizar marcador
        if (scoreText != null)
        {
            string scoresDisplay = "";
            // runner.ActivePlayers puede estar vacío al inicio; iterar de forma segura
            foreach (var player in runner.ActivePlayers)
            {
                int index = player.RawEncoded;
                int score = gm.GetScore(player);
                scoresDisplay += $"Jugador {index}: {score}\n";
            }

            scoreText.text = scoresDisplay;
        }
    }

    // 🏆 Mostrar pantalla final
    public void ShowEndMatch(int winnerIndex, int maxScore)
    {
        if (endMatchPanel != null) endMatchPanel.SetActive(true);
        if (winnerText != null) winnerText.text = $"🏆 Ganador: Jugador {winnerIndex} con {maxScore} puntos";
    }

    // Botón de salir
    public void OnExitButton()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    // Botón de intentar de nuevo
    public void OnRetryButton()
    {
        if (endMatchPanel != null) endMatchPanel.SetActive(false);

        if (GameManager.Instance != null && runner != null)
        {
            var gm = GameManager.Instance;
            if (gm.Object != null && gm.Object.IsValid)
            {
                gm.RegisterRetryVote(runner.LocalPlayer);
            }
        }
    }
}
