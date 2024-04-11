using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class PlayerController : MonoBehaviour
{
    private VerticalLayoutGroup team1List;
    private VerticalLayoutGroup team2List;
    private TeamManager teamManager;

    private bool isOnTeam1;
    private string playerName;

    public void Initialize(VerticalLayoutGroup team1, VerticalLayoutGroup team2, TeamManager manager, string name)
    {
        team1List = team1;
        team2List = team2;
        teamManager = manager;
        playerName = name;

        // Überprüfe, in welcher LayoutGroup sich der Spieler befindet
        isOnTeam1 = transform.parent == team1List.transform;
    }

    public void SwitchTeam()
    {
        // Verschiebe den Spieler von einer LayoutGroup zur anderen
        Transform newParent = isOnTeam1 ? team2List.transform : team1List.transform;
        transform.SetParent(newParent);

        // Aktualisiere den Status des Spielers
        isOnTeam1 = !isOnTeam1;

        // Aktualisiere PlayerPrefs mit dem neuen Team des Spielers
        if (isOnTeam1)
        {
            teamManager.RemovePlayerFromTeam2(playerName);
            teamManager.AddPlayerToTeam1(playerName);
        }
        else
        {
            teamManager.RemovePlayerFromTeam1(playerName);
            teamManager.AddPlayerToTeam2(playerName);
        }

        // Debug-Ausgabe der aktualisierten Teams
        teamManager.DebugTeams();
    }
}