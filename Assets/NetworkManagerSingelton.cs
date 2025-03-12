using Unity.Netcode;
using UnityEngine;

public class NetworkManagerSingelton : NetworkManager
{
    private static NetworkManagerSingelton instance;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject); // Usuwa nowy NetworkManager
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }


}
