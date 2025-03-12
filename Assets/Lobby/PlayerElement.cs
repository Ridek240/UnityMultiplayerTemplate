using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class PlayerElement : MonoBehaviour
{
    public TextMeshProUGUI OwnerName;
    public void CreateElement(string name)
    {
        OwnerName.text = name;
    }
}
