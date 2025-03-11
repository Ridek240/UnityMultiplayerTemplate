using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TestLobby))]
public class MyScriptEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector(); // Rysuje domyœlny inspektor

        TestLobby myScript = (TestLobby)target;
        if (GUILayout.Button("CreateLobby"))
        {
            myScript.CreateLobby();
        }
        if (GUILayout.Button("CheckLobbies"))
        {
            myScript.ListLobbies();
        }
        if (GUILayout.Button("Join Lobby"))
        {
            myScript.JoinLobby();
        }
        if (GUILayout.Button("Quick join"))
        {
            myScript.QuickJoinLobby();
        }
        if (GUILayout.Button("Update Lobby"))
        {
            myScript.UpdateLobby();
        }
        if (GUILayout.Button("Leave Lobby"))
        {
            myScript.LeaveLobby();
        }
    }
}