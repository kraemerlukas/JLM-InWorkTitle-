using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using TMPro;

public enum TeamName
{
    Alpha,
    Bravo,
    Charlie,
    Delta,
    Echo
}

public class TeamManager : MonoBehaviour
{
    public GameObject playerPrefab; // Das vorgefertigte Prefab für die Spieler
    public TextMeshProUGUI team1Title;
    public TextMeshProUGUI team2Title;
    private List<string> playerNames = new List<string>();
    private List<string> team1 = new List<string>();
    private List<string> team2 = new List<string>();
    public VerticalLayoutGroup team1List;
    public VerticalLayoutGroup team2List;

    private void Start()
    {

    }

    public void InitializeTeams()
    {
        LoadPlayerNames(); // Zuerst Spieler laden, um die vorherigen zu löschen
        ClearTeams(); // Dann Teams löschen
        AssignPlayersToTeams();
        AssignTeamNames();
        SetTeamTitles(); // Titel der Teams setzen
        DisplayTeams(); // Anzeigen der Teams nach der Initialisierung
        Debug.Log("Team 1 (" + team1Name + "): " + string.Join(", ", team1));
        Debug.Log("Team 2 (" + team2Name + "): " + string.Join(", ", team2));
    }

    private string team1Name;
    private string team2Name;

    private void AssignTeamNames()
    {
        List<TeamName> availableTeamNames = new List<TeamName>((TeamName[])Enum.GetValues(typeof(TeamName)));
        Shuffle(availableTeamNames);

        team1Name = availableTeamNames[0].ToString();
        team2Name = availableTeamNames[1].ToString();
    }

    private void LoadPlayerNames()
    {
        playerNames.Clear(); // Vorherige Spieler löschen
        int playerCount = PlayerPrefs.GetInt("PlayerCount", 0);
        for (int i = 1; i <= playerCount; i++)
        {
            string playerName = PlayerPrefs.GetString("Player" + i);
            playerNames.Add(playerName);
        }
    }

    private void DisplayTeams()
    {
        // Team 1 Spieler hinzufügen
        foreach (string player in team1)
        {
            GameObject playerObj = Instantiate(playerPrefab, team1List.transform);
            playerObj.GetComponentInChildren<TextMeshProUGUI>().text = player;
            PlayerController playerController = playerObj.GetComponent<PlayerController>();
            playerController.Initialize(team1List, team2List, this, player);
        }

        // Team 2 Spieler hinzufügen
        foreach (string player in team2)
        {
            GameObject playerObj = Instantiate(playerPrefab, team2List.transform);
            playerObj.GetComponentInChildren<TextMeshProUGUI>().text = player;
            PlayerController playerController = playerObj.GetComponent<PlayerController>();
            playerController.Initialize(team1List, team2List, this, player);
        }
    }

    private void AssignPlayersToTeams()
    {
        Shuffle(playerNames); // Shuffle player names for fair distribution

        foreach (string playerName in playerNames)
        {
            if (team1.Count < team2.Count)
            {
                team1.Add(playerName);
            }
            else
            {
                team2.Add(playerName);
            }
        }
    }

    // Titel der Teams setzen
    private void SetTeamTitles()
    {
        team1Title.text = team1Name;
        team2Title.text = team2Name;
    }

    // Löscht vorhandene Teams
    public void ClearTeams()
    {
        team1.Clear();
        team2.Clear();
        // Auch die Spieler-GameObjects in den Teams löschen
        foreach (Transform child in team1List.transform)
        {
            Destroy(child.gameObject);
        }
        foreach (Transform child in team2List.transform)
        {
            Destroy(child.gameObject);
        }
    }

    // Spieler aus Team 1 entfernen
    public void RemovePlayerFromTeam1(string playerName)
    {
        team1.Remove(playerName);
        SaveTeamsToPlayerPrefs();
    }

    // Spieler aus Team 2 entfernen
    public void RemovePlayerFromTeam2(string playerName)
    {
        team2.Remove(playerName);
        SaveTeamsToPlayerPrefs();
    }

    // Spieler zu Team 1 hinzufügen
    public void AddPlayerToTeam1(string playerName)
    {
        team1.Add(playerName);
        SaveTeamsToPlayerPrefs();
    }

    // Spieler zu Team 2 hinzufügen
    public void AddPlayerToTeam2(string playerName)
    {
        team2.Add(playerName);
        SaveTeamsToPlayerPrefs();
    }

    // Teams und Spieler in PlayerPrefs speichern
    private void SaveTeamsToPlayerPrefs()
    {
        PlayerPrefs.SetString("Team1Name", team1Name);
        PlayerPrefs.SetString("Team2Name", team2Name);

        PlayerPrefs.SetInt("Team1Count", team1.Count);
        for (int i = 0; i < team1.Count; i++)
        {
            PlayerPrefs.SetString("Team1Player" + i, team1[i]);
        }

        PlayerPrefs.SetInt("Team2Count", team2.Count);
        for (int i = 0; i < team2.Count; i++)
        {
            PlayerPrefs.SetString("Team2Player" + i, team2[i]);
        }

        PlayerPrefs.Save();
    }

    // Fisher-Yates shuffle algorithm to shuffle the list
    private void Shuffle<T>(List<T> list)
    {
        System.Random rnd = new System.Random();
        for (int i = 0; i < list.Count; i++)
        {
            int randomIndex = rnd.Next(i, list.Count);
            T temp = list[i];
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }

    // Debug-Ausgabe der Teams
    public void DebugTeams()
    {
        Debug.Log("Team 1 (" + team1Name + "): " + string.Join(", ", team1));
        Debug.Log("Team 2 (" + team2Name + "): " + string.Join(", ", team2));
    }
}