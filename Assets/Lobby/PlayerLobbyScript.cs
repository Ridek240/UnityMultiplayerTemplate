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
        LobbyMenager.UpdatePlayerList += UpdatePlayerScreen;
    }

    private void OnEnable()
    {
        UpdatePlayerScreen();
    }

    private void UpdatePlayerScreen()
    {
        if (LobbyMenager.ActualLobby != null)
        {
            LobbyInterface.ClearChildren(PlayerList);

            LobbyName.text = LobbyMenager.ActualLobby.Name;
            Code.text = LobbyMenager.ActualLobby.LobbyCode;

            foreach (var player in LobbyMenager.ActualLobby.Players)
            {
                PlayerElement element = Instantiate(PlayerListPrefab, PlayerList).GetComponent<PlayerElement>();
                element.CreateElement(player.Data["PlayerName"].Value);
            }
        }
    }

    public void CopyToClipboard()
    {
        GUIUtility.systemCopyBuffer = LobbyMenager.ActualLobby.LobbyCode;
    }

    public async void LeaveLobby()
    {

        await LobbyService.Instance.RemovePlayerAsync(LobbyMenager.ActualLobby.Id, AuthenticationService.Instance.PlayerId);
        LobbyInterface.Instance.OpenLobbyList();
        LobbyMenager.ActualLobby = null;
        
    }

    public async void StartGame()
    {
        if(LobbyMenager.IsLobbyOwner)
        {
            try
            {
                Debug.Log("StartGame");

                string RelayCode = await TestRelay.CreateRelay();
                await LobbyService.Instance.UpdateLobbyAsync(LobbyMenager.ActualLobby.Id, new
                    UpdateLobbyOptions
                {
                    Data = new Dictionary<string, DataObject>
                {
                    {"RelayCode", new DataObject(DataObject.VisibilityOptions.Member, RelayCode) },
                    {"GameStarted", new DataObject(DataObject.VisibilityOptions.Public, "1") }
                }
                });


                GameMenager.Instance.StartGame();
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }
    }

    private void OnDestroy()
    {
        LobbyMenager.UpdatePlayerList -= UpdatePlayerScreen;
    }
}
