using UnityEngine;

public class InterfaceMenager : MonoBehaviour
{
    public void LeaveGame()
    {
        GameMenager.ReturnToLobby();
    }
}
