using System;
using Unity.Services.Lobbies.Models;
using Unity.VisualScripting;
using UnityEngine;

public class DiscordConnection : MonoBehaviour
{
    Discord.Discord discord;

    public static DiscordConnection instance;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(instance == null)
        {
            instance = this;
            DontDestroyOnLoad(instance);

        }
        else
        {
            Destroy(this);
        }
        try
        {
            discord = new Discord.Discord(1369010254985171036, (ulong)Discord.CreateFlags.NoRequireDiscord);
        }
        catch(Exception e)
        {
            discord = null;
            Debug.Log(e);
        }
        if (discord == null) return;

        discord.GetActivityManager().OnActivityJoin += (secret) =>
        {
            Debug.Log("Otrzymano ¿¹danie do³¹czenia z tokenem: " + secret);
            LobbyMenager.Instance.JoinLobbyByCode(secret);

        };

        ChangeActivity("Main Menu");
    }


    private void OnDisable()
    {
        discord.Dispose();
    }

    public void ChangeActivity(string SceneName)
    {
        if (discord == null) return;
        var activityMenager = discord.GetActivityManager();
        var activity = new Discord.Activity();

        activity.State = "In Main Menu";
        activity.Details = SceneName;
        //activity.Timestamps.Start = 1;
        activity.Assets.LargeImage = "gorem";



        activityMenager.UpdateActivity(activity, (res) => { Debug.Log("Activity Updated"); 
        });
    }


    public void ChangeActivity(Lobby lobby)
    {
        if (discord == null) return;
        var activityMenager = discord.GetActivityManager();
        var activity = new Discord.Activity();

        activity.State = "In Lobby";
        var detail = LobbyMenager.Instance.IsLobbyOwner ? "Creating Lobby" : "In Lobby";
        activity.Details = $"{detail} : {lobby.Name}";
        activity.Secrets.Join = lobby.LobbyCode;
        activity.Party.Id = "1234";
        activity.Party.Size.CurrentSize = lobby.Players.Count; 
        activity.Party.Size.MaxSize = lobby.MaxPlayers; 
        activity.Party.Privacy = lobby.IsPrivate ? Discord.ActivityPartyPrivacy.Private : Discord.ActivityPartyPrivacy.Public;
        activityMenager.UpdateActivity(activity, (res) =>
        {
            Debug.Log(res.ToString());
        });


        }
    // Update is called once per frame
    void Update()
    {
        discord.RunCallbacks();
    }
}
