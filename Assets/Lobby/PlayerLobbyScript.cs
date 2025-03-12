using System;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class PlayerLobbyScript : MonoBehaviour
{
    public Transform PlayerList;
    public Transform PlayerListPrefab;

    [SerializeField] TextMeshProUGUI LobbyName;
    [SerializeField] TextMeshProUGUI Code;

    private void Start()
    {
        LobbyInterface.UpdatePlayerList += UpdatePlayerScreen;
    }

    private void OnEnable()
    {
        UpdatePlayerScreen();
    }

    private void UpdatePlayerScreen()
    {
        if (LobbyInterface.ActualLobby != null)
        {
            LobbyInterface.ClearChildren(PlayerList);

            LobbyName.text = LobbyInterface.ActualLobby.Name;
            Code.text = LobbyInterface.ActualLobby.LobbyCode;

            foreach (var player in LobbyInterface.ActualLobby.Players)
            {
                ElementScript element = Instantiate(PlayerListPrefab, PlayerList).GetComponent<ElementScript>();
                element.CreateElement(player.Id, player.Data["PlayerName"].Value, null);
            }
        }
    }

    public void CopyToClipboard()
    {
        GUIUtility.systemCopyBuffer = LobbyInterface.ActualLobby.LobbyCode;
    }

    public async void LeaveLobby()
    {

        await LobbyService.Instance.RemovePlayerAsync(LobbyInterface.ActualLobby.Id, AuthenticationService.Instance.PlayerId);
        LobbyInterface.Instance.OpenLobbyList();
        LobbyInterface.ActualLobby = null;
        
    }

    public async void StartGame()
    {
        if(LobbyInterface.IsLobbyOwner)
        {
            try
            {
                Debug.Log("StartGame");

                string RelayCode = await TestRelay.CreateRelay();
                await LobbyService.Instance.UpdateLobbyAsync(LobbyInterface.ActualLobby.Id, new
                    UpdateLobbyOptions
                {
                    Data = new Dictionary<string, DataObject>
                {
                    {"RelayCode", new DataObject(DataObject.VisibilityOptions.Member, RelayCode) }
                }
                });
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }
    }
}
