using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Lobbies.Models;
using Unity.Services.Lobbies;
using UnityEngine;
using TMPro;

public class LobbyListScript : MonoBehaviour
{

    public Transform LobbyList;
    public Transform LobbyElementPrefab;
    [SerializeField] TMP_InputField CodeField;

    private void OnEnable()
    {
        UpdateLobbyList();
    }

    public async void UpdateLobbyList()
    {

        if (LobbyMenager.IsLoggedIn)
        {
            List<Lobby> lobbies = await GetListOfLobbies();


            LobbyInterface.ClearChildren(LobbyList);

            foreach (Lobby lob in lobbies)
            {
                ElementScript element = Instantiate(LobbyElementPrefab, LobbyList).GetComponent<ElementScript>();
                string playername = "";
                foreach (var player in lob.Players)
                {
                    if (player.Id == lob.HostId)
                    {
                        Debug.Log($"Znaleziono hosta: {player.Id}");
                        playername= player.Data["PlayerName"].Value;
                    }
                }


                element.CreateElement(lob.Name, $"{lob.Players.Count}/{lob.MaxPlayers}", playername, lob.HasPassword, lob.Data["GameStarted"].Value == "1" ? true : false, lob);
            }
        }
    }

    public async Task<List<Lobby>> GetListOfLobbies()
    {
        try
        {
            QueryLobbiesOptions options = new QueryLobbiesOptions
            {
                Count = 5,
                Filters = new List<QueryFilter>
                {
                    new QueryFilter(QueryFilter.FieldOptions.AvailableSlots, "0", QueryFilter.OpOptions.GT)
                },
                Order = new List<QueryOrder>
                {
                    new QueryOrder(false,QueryOrder.FieldOptions.Created)
                }
            };
            QueryResponse responce = await LobbyService.Instance.QueryLobbiesAsync(options);
            return responce.Results;
        }
        catch (LobbyServiceException e)
        {

            Debug.Log(e.Message);
            return new List<Lobby>();
        }
    }

    public static async void JoinByLobby(Lobby lobby)
    {
        try
        {
            JoinLobbyByIdOptions options = new JoinLobbyByIdOptions
            {
                Player = LobbyMenager.CurrentPlayer
            };

            Lobby joinedLobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobby.Id, options);
            LobbyMenager.ActualLobby = joinedLobby;
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e.Message);
        }
    }

    public async void JoinLobbyByCode()
    {
        try
        {
            JoinLobbyByCodeOptions options = new JoinLobbyByCodeOptions
            {
                Player = LobbyMenager.CurrentPlayer
            };

            Lobby joinedLobby = await LobbyService.Instance.JoinLobbyByCodeAsync(CodeField.text, options);
            LobbyMenager.ActualLobby = joinedLobby;

        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e.Message);
        }
    }
}
