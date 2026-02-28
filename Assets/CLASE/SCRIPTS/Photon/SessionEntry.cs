using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Fusion;

public class SessionEntry : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI sessionName;
    [SerializeField] private TextMeshProUGUI playerCount;
    [SerializeField] private Button joinButton;

    public void SetInfo(SessionInfo sessionInfo)
    {
        sessionName.text = sessionInfo.Name;
        playerCount.text = sessionInfo.PlayerCount.ToString() + "/" + sessionInfo.MaxPlayers.ToString();

        if (sessionInfo.PlayerCount >= sessionInfo.MaxPlayers )
        
        {

            joinButton.interactable = false;
        }
    }

    public void JoinSession() 
    {
        PhotonManager._PhotonManager.JoinSession(sessionName.text);
    }

}

