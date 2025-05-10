using System.Collections.Generic;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class LobbyList : MonoBehaviour
{

    public Transform LobbyListRect;
    public Transform LobbyElementPrefab;
    [SerializeField] TMP_InputField CodeField;
    private void Awake()
    {
        LobbyMenager.Instance.OnLobbyStateUpdate += OnLobbyStateUpdate;
    }

    private void OnLobbyStateUpdate()
    {
        if (LobbyMenager.Instance.LobbyState == LobbyState.SearchingLobby)
        {
            gameObject.SetActive(true);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    public async void UpdateLobbyList()
    {

        if (LobbyMenager.Instance.IsLoggedIn)
        {
            List<Lobby> lobbies = await LobbyMenager.Instance.GetListOfLobbies();


            LobbyMenager.ClearChildren(LobbyListRect);

            foreach (Lobby lob in lobbies)
            {
                ElementScript element = Instantiate(LobbyElementPrefab, LobbyListRect).GetComponent<ElementScript>();
                string playername = "";
                foreach (var player in lob.Players)
                {
                    if (player.Id == lob.HostId)
                    {
                        Debug.Log($"Znaleziono hosta: {player.Id}");
                        playername = player.Data["PlayerName"].Value;
                    }
                }


                element.CreateElement(lob.Name, $"{lob.Players.Count}/{lob.MaxPlayers}", playername, lob.HasPassword, lob.Data["GameStarted"].Value == "1" ? true : false, lob);
            }
        }
    }

    public void OnJoinLobby()
    {
        LobbyMenager.Instance.JoinLobbyByCode(CodeField.text);
    }
    public void OnCreateLobby()
    {
        LobbyMenager.Instance.LobbyState = LobbyState.CreatingLobby;
    }
}
