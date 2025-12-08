using UnityEngine;

public class UIFlowManager : MonoBehaviour
{
    [Header("Paneles UI")]
    [SerializeField] private GameObject panelStartButtons;
    [SerializeField] private GameObject panelGameUI;
    [SerializeField] private GameObject panelEndMatch;

    private void Start()
    {
        // Mostrar solo el panel de entrada al inicio
        panelStartButtons.SetActive(true);
        panelGameUI.SetActive(false);
        panelEndMatch.SetActive(false);
    }

    // Llamado desde los botones START HOST / START CLIENT
    public void OnStartGame()
    {
        panelStartButtons.SetActive(false);
        panelGameUI.SetActive(true);
    }

    // Llamado desde UIManager al terminar la partida
    public void ShowEndMatchPanel()
    {
        panelEndMatch.SetActive(true);
    }

    public void HideEndMatchPanel()
    {
        panelEndMatch.SetActive(false);
    }
}
