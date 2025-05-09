using System.Collections;
using System.Collections.Generic;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class TestLobby : MonoBehaviour
{

    [SerializeField] Lobby HostLobby { get; set; }
    [SerializeField] Lobby JoinedLobby { get; set; }
    [SerializeField] private float RefreshTime;
    [SerializeField] private float RefreshTimeReamining = 15;
    [SerializeField] private string LobbyCode;
    [SerializeField] private string PlayerName;
    string lobbyName = "test";
    int maxPlayers = 4;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    async void Start()
    {
        await UnityServices.InitializeAsync();

        AuthenticationService.Instance.SignedIn += () =>
        {
            Debug.Log($"signed in {AuthenticationService.Instance.PlayerId}");
        };

        await AuthenticationService.Instance.SignInAnonymouslyAsync();
        //await AuthenticationService.Instance.SignInWithSteamAsync();
        

    }

    [ContextMenu("Create lobby")]
    public async void CreateLobby()
    {
        try
        {

            CreateLobbyOptions options = new CreateLobbyOptions { 
                IsPrivate = false,
                Player = new Player
                {
                    Data = new Dictionary<string, PlayerDataObject> { {"PlayerName" , new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, PlayerName) } }
                },
                Data = new Dictionary<string, DataObject>
                {
                    {"Gamemode", new DataObject(DataObject.VisibilityOptions.Public, "KillingSphere", DataObject.IndexOptions.S1 ) }
                }
            };
            HostLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, options);
            JoinedLobby = HostLobby;
            Debug.Log($"Joined Lobby: {HostLobby.Name} {HostLobby.MaxPlayers} {HostLobby.Id} {HostLobby.LobbyCode} ");
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e.Message);
        }
    }


    public async void ListLobbies()
    {
        try
        {
            QueryLobbiesOptions options = new QueryLobbiesOptions
            {
                Count = 25,
                Filters = new List<QueryFilter>
                {
                    new QueryFilter(QueryFilter.FieldOptions.AvailableSlots, "0", QueryFilter.OpOptions.GT),
                    new QueryFilter(QueryFilter.FieldOptions.S1, "KillingSphere", QueryFilter.OpOptions.EQ)
                },
                Order = new List<QueryOrder>
                {
                    new QueryOrder(false,QueryOrder.FieldOptions.Created)
                }


            }
            ;

            QueryResponse responce = await LobbyService.Instance.QueryLobbiesAsync(options);
            Debug.Log($"Lobbies found {responce.Results.Count}");

            foreach (Lobby lobby in responce.Results)
            {
                Debug.Log($"{lobby.Name} {lobby.MaxPlayers}");
            }
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e.Message);
        }

    }
    public void FixedUpdate()
    {
        SendHeartBeat();
        UpdateLobbyData();

    }

    private async void SendHeartBeat()
    {
        RefreshTimeReamining -= Time.deltaTime;
        if (RefreshTimeReamining < 0)
        {
            RefreshTimeReamining = RefreshTime;
            if (HostLobby != null)
            {
                PrintPlayers(HostLobby);
                await LobbyService.Instance.SendHeartbeatPingAsync(HostLobby.Id);
            }
        }
    }

    public async void JoinLobby()
    {
        try
        {
            JoinLobbyByCodeOptions options = new JoinLobbyByCodeOptions
            {
                Player = new Player
                {
                    Data = new Dictionary<string, PlayerDataObject> { { "PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, PlayerName) } }
                }
            };

            Debug.Log($"Joined lobby with code {LobbyCode}");
            Lobby joinedLobby = await LobbyService.Instance.JoinLobbyByCodeAsync(LobbyCode, options);
            JoinedLobby = joinedLobby;
            PrintPlayers(joinedLobby);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e.Message);
        }
    }

    public async void QuickJoinLobby()
    {
        try
        {
            await LobbyService.Instance.QuickJoinLobbyAsync();

        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e.Message);
        }
    }

    private void PrintPlayers(Lobby lobby)
    {

        Debug.Log($"{lobby.Name} {lobby.Data["Gamemode"].Value}");
        foreach(Player player in lobby.Players)
        {
            Debug.Log($"{player.Id}  {player.Data["PlayerName"].Value}");
        }
    }

    public async void UpdateLobby()
    {

        HostLobby = await LobbyService.Instance.UpdateLobbyAsync(HostLobby.Id, new UpdateLobbyOptions { Name = "Systematyzm" });
        JoinedLobby = HostLobby;
    }
    float RefreshTimeReamining2;
    float RefreshTime2=1;
    private async void UpdateLobbyData()
    {
        if (JoinedLobby != null)
        {
            RefreshTimeReamining2 -= Time.deltaTime;
        if (RefreshTimeReamining2 < 0)
        {
            RefreshTimeReamining2 = RefreshTime2;

                JoinedLobby = await LobbyService.Instance.GetLobbyAsync(JoinedLobby.Id);
                PrintPlayers(JoinedLobby);
            }
        }
    }

    public async void LeaveLobby()
    {
        await LobbyService.Instance.RemovePlayerAsync(JoinedLobby.Id, AuthenticationService.Instance.PlayerId);
    }
}
