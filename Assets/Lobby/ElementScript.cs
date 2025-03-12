using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class ElementScript : MonoBehaviour
{
    public  TextMeshProUGUI ServerName;
    public TextMeshProUGUI MaxPlayers;
    public Lobby Lobby;


    public void CreateElement(string name, string maxplayers, Lobby lobby)
    {
        ServerName.text = name;
        MaxPlayers.text = maxplayers;

        Lobby = lobby;
    }

    public void onClick()
    {
        LobbyListScript.JoinByLobby(Lobby);
    }
}
