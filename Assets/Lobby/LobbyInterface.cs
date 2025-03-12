using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using Unity.Services.Lobbies;
using UnityEngine;
using NUnit.Framework;
using System.Threading.Tasks;
using TMPro;
using Unity.Multiplayer.Playmode;
using UnityEngine.UI;
using System;
using Unity.Services.Authentication;

public class LobbyInterface : MonoBehaviour
{
    private static LobbyInterface instance;
    public static LobbyLayout layout = LobbyLayout.LobbyList;


    private void OnEnable()
    {
        switch(layout)
        {
            default:
            case LobbyLayout.LobbyList:
                OpenLobbyList();
                break;
            case LobbyLayout.LobbyPlayer:
                OpenLobbyLobby();
                break;
        }
        
    }

    private void Start()
    {
        if (instance == null)
        {
            instance = this;
        }
    }


    public static LobbyInterface Instance { get => instance; set => instance = value; }

    [SerializeField] Transform PlayerMenu;
    [SerializeField] Transform LobbyMenu;
    [SerializeField] Transform LobbyListMenu;
    [SerializeField] Transform LobbyCreateMenu;
    [SerializeField] Transform LobbyLobbyMenu;


    public static void ClearChildren(Transform parent)
    {
        if (parent.childCount == 0) return;
        foreach (Transform child in parent)
        {
            Destroy(child.gameObject);
        }
    }



    private void FixedUpdate()
    {
        if(LobbyMenager.CurrentPlayer == null)
        {
            LobbyMenu.gameObject.SetActive(false);
            PlayerMenu.gameObject.SetActive(true);
        }
        else
        {
            LobbyMenu.gameObject.SetActive(true);
            PlayerMenu.gameObject.SetActive(false);
        }
        
    }

    private void Update()
    {
        SendHeartBeat();
    }


    [SerializeField] private float RefreshTime = 15;
    [SerializeField] private float RefreshTimeReamining = 15;

    private async void SendHeartBeat()
    {
        if (LobbyMenager.ActualLobby != null && LobbyMenager.IsLobbyOwner)
        {
            RefreshTimeReamining -= Time.deltaTime;
            if (RefreshTimeReamining < 0)
            {
                RefreshTimeReamining = RefreshTime;
                await LobbyService.Instance.SendHeartbeatPingAsync(LobbyMenager.ActualLobby.Id);
            }
        }
    }

    public void OpenLobbyCreate()
    {
        LobbyCreateMenu.gameObject.SetActive(true);
        LobbyListMenu.gameObject.SetActive(false);
        LobbyLobbyMenu.gameObject.SetActive(false);
        layout = LobbyLayout.LobbyCreate;
    }

    public void OpenLobbyLobby()
    {
        LobbyCreateMenu.gameObject.SetActive(false);
        LobbyListMenu.gameObject.SetActive(false);
        LobbyLobbyMenu.gameObject.SetActive(true);
        layout = LobbyLayout.LobbyPlayer;
    }

    public void OpenLobbyList()
    {
        LobbyCreateMenu.gameObject.SetActive(false);
        LobbyListMenu.gameObject.SetActive(true);
        LobbyLobbyMenu.gameObject.SetActive(false);
        layout = LobbyLayout.LobbyList;
    }

    public enum LobbyLayout
    {
        LobbyList,
        LobbyPlayer,
        LobbyCreate
    }
}
