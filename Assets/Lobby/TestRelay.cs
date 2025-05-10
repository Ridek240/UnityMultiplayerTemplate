using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

public class TestRelay : MonoBehaviour
{
    private void Start()
    {
        
    }

    public static async Task<string> CreateRelay()
    {
        try
        {
            Allocation allocation =  await RelayService.Instance.CreateAllocationAsync(LobbyMenager.Instance.ActualLobby.MaxPlayers-1);
            string JoinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            
            Debug.Log($"{JoinCode} {allocation.Region}");

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(allocation.ToRelayServerData("dtls"));

            NetworkManager.Singleton.StartHost();
            return JoinCode;
        }
        catch(RelayServiceException e)
        {
            Debug.LogError(e.Message);
            return "0";
        }

    }

    public async static void JoinRelay(string joinCode)
    {
        try
        {
            JoinAllocation allocation =  await RelayService.Instance.JoinAllocationAsync(joinCode);


            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(allocation.ToRelayServerData("dtls"));


            NetworkManager.Singleton.StartClient();
        }
        catch(RelayServiceException e)
        {
            Debug.LogError(e.Message);
        }
    }
}
