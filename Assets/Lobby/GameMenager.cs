using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Netcode;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameMenager : NetworkBehaviour
{
    public GameObject PlayerPrefab;
    public static GameMenager Instance;
    public MapList gameSceneName;

    private void Start()
    {
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        NetworkManager.Singleton.OnClientConnectedCallback += PlayerJoined;
        NetworkManager.Singleton.OnClientDisconnectCallback += PlayerLeft;
    }
    public void StartGame()
    {
        NetworkManager.Singleton.SceneManager.LoadScene(gameSceneName.ToString(), LoadSceneMode.Single);
        NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += OnGameSceneLoaded;
    }

    private void PlayerLeft(ulong PlayerId)
    {
        Debug.Log($"Player {PlayerId} left the game.");

        if (IsServer)
        {
            if (NetworkManager.Singleton.ConnectedClients.TryGetValue(PlayerId, out var client))
            {
                NetworkObject playerObject = client.PlayerObject;

                if (playerObject != null)
                {
                    playerObject.Despawn(true); // Usuwa obiekt na wszystkich klientach
                    Destroy(playerObject.gameObject); // Usuwa go lokalnie
                }
            }
        }

        if (IsClient)
        {
            if (PlayerId == NetworkManager.Singleton.LocalClientId)
            {
                ReturnToLobby();
            }
        }
        //if (IsClient)
        //{
        //    if(PlayerId == NetworkManager.Singleton.LocalClientId)
        //    {

        //    }
        //}
    }

    private void PlayerJoined(ulong PlayerId)
    {
        Debug.Log($"Spawn player {PlayerId}");
        if(IsServer)
        {
            var player = Instantiate(PlayerPrefab);
            player.GetComponent<NetworkObject>().SpawnAsPlayerObject(PlayerId, true);
        }



    }

    private void OnGameSceneLoaded(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        Debug.Log("Scene Loaded");
        if (IsHost && sceneName == gameSceneName.ToString())
        {
            foreach (var id in clientsCompleted)
            {
                Debug.Log($"Spawn player {id}");
                var player = Instantiate(PlayerPrefab);
                player.GetComponent<NetworkObject>().SpawnAsPlayerObject(id, true);
            }
        }
    }

    public static void ReturnToLobby()
    {
        NetworkManager.Singleton.Shutdown();
        SceneManager.LoadScene(MapList.LobbyScene.ToString());
        //LobbyInterface.layout = LobbyInterface.LobbyLayout.LobbyPlayer;
        
    }

}

public enum MapList
{
    None,
    LobbyScene,
    TestScene
}
