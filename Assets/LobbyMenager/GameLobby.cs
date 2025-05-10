using System;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class GameLobby : MonoBehaviour
{

    public Transform PlayerList;
    public Transform PlayerListPrefab;

    [SerializeField] TextMeshProUGUI LobbyName;
    [SerializeField] TextMeshProUGUI Code;

    private void Awake()
    {
        LobbyMenager.Instance.OnLobbyUpdate += UpdatePlayerScreen;
        LobbyMenager.Instance.OnLobbyStateUpdate += OnLobbyStateUpdate;
    }

    private void OnLobbyStateUpdate()
    {
        if (LobbyMenager.Instance.LobbyState == LobbyState.InLobby)
        {
            gameObject.SetActive(true);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
    private void UpdatePlayerScreen()
    {
        if (LobbyMenager.Instance.ActualLobby != null)
        {
            LobbyMenager.ClearChildren(PlayerList);

            LobbyName.text = LobbyMenager.Instance.ActualLobby.Name;
            Code.text = LobbyMenager.Instance.ActualLobby.LobbyCode;

            foreach (var player in LobbyMenager.Instance.ActualLobby.Players)
            {
                PlayerElement element = Instantiate(PlayerListPrefab, PlayerList).GetComponent<PlayerElement>();
                element.CreateElement(player.Data["PlayerName"].Value);
            }
        }
    }
    public void CopyToClipboard()
    {
        GUIUtility.systemCopyBuffer = LobbyMenager.Instance.ActualLobby.LobbyCode;
    }

    public void LeaveLobby()
    {
        LobbyMenager.Instance.LeaveLobbySelf();
    }

    public async void StartGame()
    {
        if (LobbyMenager.Instance.IsLobbyOwner)
        {
            try
            {
                Debug.Log("StartGame");

                string RelayCode = await TestRelay.CreateRelay();
                await LobbyService.Instance.UpdateLobbyAsync(LobbyMenager.Instance.ActualLobby.Id, new
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
        LobbyMenager.Instance.OnLobbyUpdate -= UpdatePlayerScreen;
    }

}
