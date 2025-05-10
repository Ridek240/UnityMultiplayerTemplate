using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class ElementScript : MonoBehaviour
{
    public  TextMeshProUGUI ServerName;
    public  TextMeshProUGUI OwnerName;
    public TextMeshProUGUI MaxPlayers;
    public Lobby Lobby;
    public Image PasswordIcon;
    public Image StartedIcon;

    public bool HasPassword
    {
        set
        {
            PasswordIcon.gameObject.SetActive(value);
        }
    }
    public bool HasStarted
    {

        set { StartedIcon.gameObject.SetActive(value); }
    }


    public void CreateElement(string name, string maxplayers,string OwnerName, bool HasPassword, bool HasStarted, Lobby lobby)
    {
        ServerName.text = name;
        MaxPlayers.text = maxplayers;
        this.HasPassword =  HasPassword;
        this.HasStarted = HasStarted;
        this.OwnerName.text = OwnerName;
        Lobby = lobby;
    }

    public void onClick()
    {
        LobbyMenager.Instance.JoinLobbyByLobby(Lobby);
    }
}
