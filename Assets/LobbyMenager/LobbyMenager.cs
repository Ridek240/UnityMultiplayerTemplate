using UnityEngine;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using System;
using Unity.Services.Authentication;
using Unity.Services.Relay.Models;
using Unity.Services.Relay;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using System.Threading.Tasks;
using System.Collections.Generic;

public class LobbyMenager : MonoBehaviour
{
    public static LobbyMenager Instance;

    private void Awake()
    {
        if(Instance==null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }


    //Fields
    public bool GameStarted;
    public Player CurrentPlayer;
    public bool IsLoggedIn
    {
        get { return CurrentPlayer != null; }
    }
    private Lobby _ActualLobby;
    public Lobby ActualLobby
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
    private LobbyState _LobbyState;

    public LobbyState LobbyState
    {
        get { return _LobbyState; }
        set
        {
            _LobbyState = value;
            OnLobbyStateUpdate?.Invoke();
        }
    }
    public bool IsLobbyOwner
    {
        get
        {
            return (ActualLobby.HostId == AuthenticationService.Instance.PlayerId);
        }
    }

    //Events
    public event Action OnLobbyUpdate;
    public event Action OnLobbyStateUpdate;

    private async void SetNewLobby(Lobby value)
    {
        _ActualLobby = value;
        if (value != null)
        {
            LobbyEventCallbacks callbacks = new LobbyEventCallbacks();
            callbacks.LobbyChanged += LobbyChanged;
            //callbacks.PlayerJoined += PlayerJoined;
            await LobbyService.Instance.SubscribeToLobbyEventsAsync(value.Id, callbacks);

            Debug.Log($"Joined Lobby: {_ActualLobby.Name} {_ActualLobby.MaxPlayers} {_ActualLobby.Id} {_ActualLobby.LobbyCode} ");
            OnLobbyUpdate?.Invoke();

            DiscordConnection.instance.ChangeActivity(_ActualLobby);
            LobbyChanged(null);
            LobbyState = LobbyState.InLobby;

        }
        else
        {
            LobbyState = LobbyState.SearchingLobby;
        }

    }

    private void Start()
    {
        LobbyState = LobbyState.None;
    }
    private void Update()
    {
        SendHeartBeat();
    }
    [SerializeField] private float RefreshTime = 15;
    [SerializeField] private float RefreshTimeReamining = 15;
    private async void SendHeartBeat()
    {
        if (ActualLobby != null && IsLobbyOwner)
        {
            RefreshTimeReamining -= Time.deltaTime;
            if (RefreshTimeReamining < 0)
            {
                RefreshTimeReamining = RefreshTime;
                await LobbyService.Instance.SendHeartbeatPingAsync(ActualLobby.Id);
            }
        }
    }
    private async void LobbyChanged(ILobbyChanges obj)
    {
        _ActualLobby = await LobbyService.Instance.GetLobbyAsync(_ActualLobby.Id);
        OnLobbyUpdate?.Invoke();
        if (!IsLobbyOwner)
        {
            if (ActualLobby.Data["RelayCode"].Value != "0")
                TestRelay.JoinRelay(ActualLobby.Data["RelayCode"].Value);
        }
    }

    public async void CreateLobby(string LobbyName, int MaxPlayers, bool IsPrivate)
    {
        try
        {
            CreateLobbyOptions options = new CreateLobbyOptions
            {
                Player = CurrentPlayer,
                Data = new Dictionary<string, DataObject>
                {
                    {"RelayCode", new DataObject(DataObject.VisibilityOptions.Member, "0") },
                    {"GameStarted", new DataObject(DataObject.VisibilityOptions.Public, "0") },
                }
            };

            options.IsPrivate = IsPrivate;
            ActualLobby = await LobbyService.Instance.CreateLobbyAsync(LobbyName, MaxPlayers, options);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e.Message);
        }
    }
    #region Joining/Leaving
    public async void JoinLobbyByLobby(Lobby lobby)
    {
        try
        {
            JoinLobbyByIdOptions options = new JoinLobbyByIdOptions
            {
                Player = CurrentPlayer
            };

            Lobby joinedLobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobby.Id, options);
            ActualLobby = joinedLobby;
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e.Message);
        }
    }

    public async void JoinLobbyByCode(string code)
    {
        try
        {
            JoinLobbyByCodeOptions options = new JoinLobbyByCodeOptions
            {
                Player = CurrentPlayer
            };

            Lobby joinedLobby = await LobbyService.Instance.JoinLobbyByCodeAsync(code, options);
            ActualLobby = joinedLobby;

        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e.Message);
        }
    }
    public async void LeaveLobby(string PlayerID)
    {
        try
        {
            await LobbyService.Instance.RemovePlayerAsync(ActualLobby.Id, PlayerID);
            //LobbyInterface.Instance.OpenLobbyList();
            //ActualLobby = null;
        }
        catch(Exception e)
        {
            Debug.LogError(e.ToString());
        }
    }
    public async void LeaveLobbySelf()
    {
        LeaveLobby(AuthenticationService.Instance.PlayerId);
        ActualLobby = null;
    }
    #endregion
    public async Task<List<Lobby>> GetListOfLobbies(int Max_Lobbies = 5)
    {
        try
        {
            QueryLobbiesOptions options = new QueryLobbiesOptions
            {
                Count = Max_Lobbies,
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

    #region RelayConnection
    public async void JoinRelay(string joinCode)
    {
        try
        {
            JoinAllocation allocation = await RelayService.Instance.JoinAllocationAsync(joinCode);


            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(allocation.ToRelayServerData("dtls"));


            NetworkManager.Singleton.StartClient();
        }
        catch (RelayServiceException e)
        {
            Debug.LogError(e.Message);
        }
    }

    public async Task<string> CreateRelay()
    {
        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(ActualLobby.MaxPlayers - 1);
            string JoinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            Debug.Log($"{JoinCode} {allocation.Region}");

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(allocation.ToRelayServerData("dtls"));

            NetworkManager.Singleton.StartHost();
            return JoinCode;
        }
        catch (RelayServiceException e)
        {
            Debug.LogError(e.Message);
            return "0";
        }

    }
    #endregion

    public static void ClearChildren(Transform parent)
    {
        if (parent.childCount == 0) return;
        foreach (Transform child in parent)
        {
            Destroy(child.gameObject);
        }
    }
}

public enum LobbyState
{
    None,
    CreatingLobby,
    InLobby,
    SearchingLobby
}

