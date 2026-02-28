using Fusion;
using TMPro;
using TMPro.SpriteAssetUtilities;
using UnityEngine;

public class LobbyManager : MonoBehaviour
{
    [SerializeField] private Transform viewportContent;
    [SerializeField] private GameObject lobbyPrefab;
    [SerializeField] private GameObject WarningMessage;

    [Header("Custom Session")]
    [SerializeField] private TMP_InputField sessionNameInput;
    [SerializeField] private TMP_Text maxPlayerCountTxt;
    private int maxPlayerCount =0;
    private void OnEnable()
    { 
        PhotonManager._PhotonManager.onSessionListUpdated += DestroyCanvasContent;
        PhotonManager._PhotonManager.onSessionListUpdated += UpdateSessionCanvas;
        maxPlayerCountTxt.text = maxPlayerCount.ToString();
       
    }

    private void OnDisable()
    {
        PhotonManager._PhotonManager.onSessionListUpdated -= DestroyCanvasContent;
        PhotonManager._PhotonManager.onSessionListUpdated -= UpdateSessionCanvas;

      
    }

    public void DestroyCanvasContent()
    {

        Debug.Log("Destroy Canvas");
        WarningMessage.SetActive(PhotonManager._PhotonManager.availableSessions.Count <=0);

        for (int i = 0; i <viewportContent.childCount; i++)
        {

            Destroy(viewportContent.GetChild(i).gameObject);

        }
    }

    public void UpdateSessionCanvas()
    {

        Debug.Log("Creando sesiones: " + PhotonManager._PhotonManager.availableSessions.Count);
        foreach (SessionInfo session in PhotonManager._PhotonManager.availableSessions)
        {
            GameObject sessionInstance = Instantiate(lobbyPrefab, viewportContent);
            sessionInstance.GetComponent<SessionEntry>().SetInfo(session);
        }
    }

    public void UpdatePlayerCount(int number)
    {
        maxPlayerCount += number;

        maxPlayerCount = maxPlayerCount > 10? 1 : maxPlayerCount <= 0? 10: maxPlayerCount;
        maxPlayerCountTxt.text = maxPlayerCount.ToString();
    }

    public void ResetCustomLobby()
    {
        maxPlayerCount = 1;
        maxPlayerCountTxt.text = maxPlayerCount.ToString();
        sessionNameInput.text = string.Empty;

    }

    // 🔥 MÉTODO NUEVO PARA EL BOTÓN "CREATE LOBBY"
    public void CreateLobbyButton()
    {
        string sessionName = sessionNameInput.text;

        if (string.IsNullOrEmpty(sessionName))
        {
            Debug.LogWarning("El nombre de la sesión está vacío.");
            return;
        }

        PhotonManager._PhotonManager.CreateCustomLobby(sessionName, maxPlayerCount);
    }

}
