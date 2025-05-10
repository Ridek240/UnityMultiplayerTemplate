using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyCreation : MonoBehaviour
{
    [SerializeField] Toggle Isprivate;
    [SerializeField] Slider MaxPlayers;
    [SerializeField] TMP_InputField LobbyName;
    [SerializeField] TextMeshProUGUI Number;


    private void Awake()
    {
        LobbyMenager.Instance.OnLobbyStateUpdate += OnLobbyStateUpdate;
    }

    private void OnLobbyStateUpdate()
    {
        if(LobbyMenager.Instance.LobbyState == LobbyState.CreatingLobby)
        {
            gameObject.SetActive(true);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    public void CreateLobby()
    {
        LobbyMenager.Instance.CreateLobby(LobbyName.text, (int)MaxPlayers.value, Isprivate.isOn);
    }

    public void OnSliderValueChanged(Single value)
    {
        Number.text = value.ToString();
    }
}
