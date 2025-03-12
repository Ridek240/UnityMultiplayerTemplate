using System.Collections.Generic;
using TMPro;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class CreateLobbyScript : MonoBehaviour
{

    [SerializeField] Toggle Isprivate;
    [SerializeField] Slider MaxPlayers;
    [SerializeField] TMP_InputField LobbyName;
    [SerializeField] TextMeshProUGUI Number;


    public async void CreateLobby()
    {
        try
        {
            CreateLobbyOptions options = new CreateLobbyOptions
            {
                Player = LobbyMenager.CurrentPlayer,
                Data = new Dictionary<string, DataObject>
                {
                    {"RelayCode", new DataObject(DataObject.VisibilityOptions.Member, "0") },
                    {"GameStarted", new DataObject(DataObject.VisibilityOptions.Public, "0") },
                    
                }
                
            };

            options.IsPrivate = Isprivate.isOn;
            LobbyMenager.ActualLobby = await LobbyService.Instance.CreateLobbyAsync(LobbyName.text, (int)MaxPlayers.value, options);
            //LobbyMenager.Instance.OpenLobbyLobby();
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e.Message);
        }
    }

    public void Click()
    {
        CreateLobby();
    }

    public void OnSliderValueChanged()
    {
        Number.text = MaxPlayers.value.ToString();
    }
}
