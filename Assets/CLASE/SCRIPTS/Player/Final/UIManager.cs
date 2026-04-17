using Fusion;
using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("Paneles UI")]
    [SerializeField] private GameObject panelStartButtons; // Solo referencia, NO se activa aquí
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

        // El UIManager del gameplay NO controla el menú
        // El menú se activa/desactiva desde tu lógica de escenas

        if (panelGameUI != null)
            panelGameUI.SetActive(true);   // HUD siempre visible

        if (panelEndMatch != null)
            panelEndMatch.SetActive(false); // EndMatch oculto al inicio
    }

    private void Start()
    {
        runner = FindObjectOfType<NetworkRunner>();
    }

    private void Update()
    {
        if (runner == null || GameManager.Instance == null) return;
        if (GameManager.Instance.Object == null || !GameManager.Instance.Object.IsValid) return;

        // Timer
        if (timerText != null)
            timerText.text = $"Tiempo: {GameManager.Instance.Timer}s";

        // Scoreboard
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

    public void ShowEndMatch(int winnerIndex, int maxScore)
    {
        if (panelEndMatch != null)
            panelEndMatch.SetActive(true);

        if (winnerText == null) return;

        if (winnerIndex == -1)
            winnerText.text = "Ningún jugador ganó puntos";
        else
            winnerText.text = $"Ganó el jugador {winnerIndex} con {maxScore} puntos";
    }

    public void OnExitButton()
    {
        Debug.Log("EXIT PRESIONADO");
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    public void OnRetryButton()
    {
        Debug.Log("RETRY PRESIONADO");

        if (panelEndMatch != null)
            panelEndMatch.SetActive(false);

        // Reiniciar partida
        if (GameManager.Instance != null)
            GameManager.Instance.RpcStartMatch();
    }
}
