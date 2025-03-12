using System;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class LobbyMenager
{
    public static Lobby _ActualLobby;
    public static Lobby ActualLobby
    {
        get
        {
            return _ActualLobby;
        }
        set
        {
            SetNewLobby(value);
        }
    }

    public static event Action UpdatePlayerList;
    public static Player CurrentPlayer;
    public static bool IsLoggedIn
    {
        get { return CurrentPlayer != null; }
    }
    public static bool IsLobbyOwner
    {
        get
        {
            return (ActualLobby.HostId == AuthenticationService.Instance.PlayerId);
        }
    }

    private static async void SetNewLobby(Lobby value)
    {
        _ActualLobby = value;
        if (value != null)
        {
            LobbyEventCallbacks callbacks = new LobbyEventCallbacks();
            callbacks.LobbyChanged += Callbacks_LobbyChanged;
            await LobbyService.Instance.SubscribeToLobbyEventsAsync(value.Id, callbacks);

            Debug.Log($"Joined Lobby: {_ActualLobby.Name} {_ActualLobby.MaxPlayers} {_ActualLobby.Id} {_ActualLobby.LobbyCode} ");
            UpdatePlayerList?.Invoke();
            LobbyInterface.Instance.OpenLobbyLobby();


        }
        else
        {
            LobbyInterface.Instance.OpenLobbyList();
        }

    }

    private static async void Callbacks_LobbyChanged(ILobbyChanges obj)
    {
        _ActualLobby = await LobbyService.Instance.GetLobbyAsync(_ActualLobby.Id);
        UpdatePlayerList?.Invoke();
        if (!IsLobbyOwner)
        {
            if (ActualLobby.Data["RelayCode"].Value != "0")
                TestRelay.JoinRelay(ActualLobby.Data["RelayCode"].Value);
        }

    }
}
