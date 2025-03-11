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

public class LobbyMenager : MonoBehaviour
{
    private static LobbyMenager instance;
    public Transform LobbyList;
    public Transform LobbyElementPrefab;
    public Transform PlayerList;
    public Transform PlayerListPrefab;
    public static Lobby ActualLobby { 
        get 
        { 
            return _ActualLobby; 
        }
        set 
        {
            SetNewLobby(value);
        }
    }


    private void Start()
    {
        if (instance == null)
        {
            instance = this;
        }
    }
    public static LobbyMenager Instance { get => instance; set => instance = value; }

    private static async void SetNewLobby(Lobby value)
    {

        LobbyEventCallbacks callbacks = new LobbyEventCallbacks();
        callbacks.LobbyChanged += Callbacks_LobbyChanged;
        //eventCallbacks.LobbyEventConnectionStateChanged zmiana po³¹czenia
        await LobbyService.Instance.SubscribeToLobbyEventsAsync(value.Id,callbacks);
        _ActualLobby = value;
        Debug.Log($"Created Lobby: {_ActualLobby.Name} {_ActualLobby.MaxPlayers} {_ActualLobby.Id} {_ActualLobby.LobbyCode} ");
        Instance.UpdatePlayerScreen();
    }

    private static async void Callbacks_LobbyChanged(ILobbyChanges obj)
    {
        _ActualLobby = await LobbyService.Instance.GetLobbyAsync(_ActualLobby.Id);
        Instance.UpdatePlayerScreen();

    }

    private void UpdatePlayerScreen()
    {
        ClearChildren(Instance.PlayerList);

        foreach (var player in _ActualLobby.Players)
        {
            ElementScript element = Instantiate(PlayerListPrefab, PlayerList).GetComponent<ElementScript>();
            element.CreateElement(player.Id, player.Data["PlayerName"].Value, null);
        }
    }

    public static Lobby _ActualLobby;
    [SerializeField] TMP_InputField CodeField;
    [SerializeField] Toggle Isprivate;
    [SerializeField] Slider MaxPlayers;
    [SerializeField] TMP_InputField LobbyName;
    [SerializeField] TextMeshProUGUI Number;


    [SerializeField] Transform PlayerMenu;
    [SerializeField] Transform LobbyMenu;
    [SerializeField] Transform LobbyListMenu;
    [SerializeField] Transform LobbyCreateMenu;
    [SerializeField] Transform LobbyLobbyMenu;


    public async void UpdateLobbyList()
    {
        List<Lobby> lobbies  = await GetListOfLobbies();

        ClearChildren(LobbyList);

        foreach (Lobby lob in lobbies)
        {
            ElementScript element = Instantiate(LobbyElementPrefab, LobbyList).GetComponent<ElementScript>();
            element.CreateElement(lob.Name, $"{lob.Players.Count}/{lob.MaxPlayers}", lob);
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

    //public async void Dummy()
    //{
    //    try
    //    {

    //    }
    //    catch (LobbyServiceException e)
    //    {
    //        Debug.Log(e.Message);
    //    }
    //}

    public void ClearChildren(Transform parent)
    {
        foreach (Transform child in parent)
        {
            Destroy(child.gameObject);
        }
    }

    public async void JoinLobbyByCode()
    {
        try
        {
            JoinLobbyByCodeOptions options = new JoinLobbyByCodeOptions
            {
                Player = MPPlayer.CurrentPlayer
            };

            Lobby joinedLobby = await LobbyService.Instance.JoinLobbyByCodeAsync(CodeField.text, options);
            ActualLobby = joinedLobby;

        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e.Message);
        }
    }

    public async void CreateLobby()
    {
        try
        {
            CreateLobbyOptions options = new CreateLobbyOptions
            {
                Player = MPPlayer.CurrentPlayer
            };
            
            options.IsPrivate = Isprivate.isOn;
            ActualLobby = await LobbyService.Instance.CreateLobbyAsync(LobbyName.text, (int)MaxPlayers.value, options);
            OpenLobbyLobby();
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e.Message);
        }
    }

    public static async void JoinByLobby(Lobby lobby)
    {
        try
        {
            JoinLobbyByIdOptions options = new JoinLobbyByIdOptions
            {
                Player = MPPlayer.CurrentPlayer
            };

            Lobby joinedLobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobby.Id, options);
            ActualLobby = joinedLobby;
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e.Message);
        }
    }

    private void FixedUpdate()
    {
        if(MPPlayer.CurrentPlayer == null)
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
        RefreshTimeReamining -= Time.deltaTime;
        if (RefreshTimeReamining < 0)
        {
            RefreshTimeReamining = RefreshTime;
            if (ActualLobby != null)
            {
                await LobbyService.Instance.SendHeartbeatPingAsync(ActualLobby.Id);
            }
        }
    }


    public void OnSliderValueChanged()
    {
        Number.text = MaxPlayers.value.ToString();
    }


    public void OpenLobbyCreate()
    {
        LobbyCreateMenu.gameObject.SetActive(true);
        LobbyListMenu.gameObject.SetActive(false);
        LobbyLobbyMenu.gameObject.SetActive(false);
    }

    public void OpenLobbyLobby()
    {
        LobbyCreateMenu.gameObject.SetActive(false);
        LobbyListMenu.gameObject.SetActive(false);
        LobbyLobbyMenu.gameObject.SetActive(true);
    }


}
