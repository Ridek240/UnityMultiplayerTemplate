using Unity.Services.Lobbies.Models;
using UnityEngine;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Core;
using System.Collections.Generic;
using System;

public class MPPlayer : MonoBehaviour
{
    
    [SerializeField] TMP_InputField PlayerName;



    public async void CreatePlayer()
    {
        await UnityServices.InitializeAsync();

        AuthenticationService.Instance.SignedIn += () =>
        {
            Debug.Log($"signed in {AuthenticationService.Instance.PlayerId}");
        };

        await AuthenticationService.Instance.SignInAnonymouslyAsync();

        LobbyMenager.CurrentPlayer = new Player
        {
            Data = new Dictionary<string, PlayerDataObject> { { "PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, PlayerName.text) } }
        };

        Debug.Log($"Created player {LobbyMenager.CurrentPlayer.Data["PlayerName"].Value} id: {LobbyMenager.CurrentPlayer.Id}");
        //onComplete?.Invoke();
    }


}
