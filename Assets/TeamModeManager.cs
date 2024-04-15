using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;

public class TeamTaskManager : MonoBehaviour
{
    public GameObject taskPrefabGroup; // Referenz auf die Prefab-Gruppe
    public TextMeshProUGUI taskText;

    private List<string> playerNames = new List<string>();
    private List<string> team1Players = new List<string>();
    private List<string> team2Players = new List<string>();
    private string team1Name;
    private string team2Name;
    private int tasksCompleted = 0;
    private int maxTasks; // Zufällige Anzahl von Aufgaben zwischen 30 und 50
    private bool gameEnded = false;
    private bool lastTeamWasTeam1 = false; // Variable, um das zuletzt gewählte Team zu verfolgen

    private void Start()
    {
        Screen.orientation = ScreenOrientation.LandscapeLeft;
        maxTasks = Random.Range(30, 51);
        LoadPlayerData();
        ShowNextTask();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && !gameEnded)
        {
            ShowNextTask();
        }

        if (gameEnded && Input.GetMouseButtonDown(0))
        {
            EndRound();
            Screen.orientation = ScreenOrientation.Portrait;
            SceneManager.LoadScene("Menu"); // Lädt die Szene "Menu"
        }
    }

    private void EndRound()
    {
        int roundsPlayed = PlayerPrefs.GetInt("RoundsPlayed", 0);
        roundsPlayed++;
        PlayerPrefs.SetInt("RoundsPlayed", roundsPlayed);
        PlayerPrefs.Save();
    }

    private void LoadPlayerData()
    {
        team1Name = PlayerPrefs.GetString("Team1Name", "Team 1");
        team2Name = PlayerPrefs.GetString("Team2Name", "Team 2");

        int team1Count = PlayerPrefs.GetInt("Team1Count", 0);
        for (int i = 0; i < team1Count; i++)
        {
            string playerName = PlayerPrefs.GetString("Team1Player" + i);
            team1Players.Add(playerName);
            playerNames.Add(playerName);
        }

        int team2Count = PlayerPrefs.GetInt("Team2Count", 0);
        for (int i = 0; i < team2Count; i++)
        {
            string playerName = PlayerPrefs.GetString("Team2Player" + i);
            team2Players.Add(playerName);
            playerNames.Add(playerName);
        }
    }

    private void ShowNextTask()
    {
        int playerCount = playerNames.Count;

        if (tasksCompleted >= maxTasks)
        {
            taskText.text = "Runde Beendet!";
            gameEnded = true;
            return;
        }

        // Zugriff auf die Kindobjekte der Prefab-Gruppe
        Transform[] childTransforms = taskPrefabGroup.GetComponentsInChildren<Transform>();
        List<GameObject> childObjects = new List<GameObject>();
        foreach (Transform child in childTransforms)
        {
            if (child.gameObject != taskPrefabGroup)
            {
                childObjects.Add(child.gameObject);
            }
        }

        // Zufällige Auswahl einer Aufgabe aus den Kindobjekten der Prefab-Gruppe
        int randomTaskIndex = Random.Range(0, childObjects.Count);
        GameObject randomTaskPrefab = childObjects[randomTaskIndex];

        string taskDescription = randomTaskPrefab.GetComponentInChildren<TextMeshProUGUI>().text;

        // Zufällige Auswahl eines Teams für die Platzhalter
        string selectedTeamName = GetRandomTeam();
        string otherTeamName = selectedTeamName == team1Name ? team2Name : team1Name;
        lastTeamWasTeam1 = selectedTeamName == team1Name; // Aktualisiere das zuletzt gewählte Team

        // Ersetze Platzhalter durch zufällig ausgewählte Spieler und Teams
        ReplaceTeamPlaceholders(ref taskDescription, selectedTeamName, otherTeamName);

        taskText.text = taskDescription;
        tasksCompleted++;
    }

    private string GetRandomTeam()
    {
        return Random.Range(0, 2) == 0 ? team1Name : team2Name;
    }

    private void ReplaceTeamPlaceholders(ref string taskDescription, string selectedTeamName, string otherTeamName)
    {
        // Ersetze {Team1} oder {Team2} durch den ausgewählten Teamnamen
        taskDescription = taskDescription.Replace("{Team1}", selectedTeamName);
        taskDescription = taskDescription.Replace("{Team2}", otherTeamName);

        // Ersetze {Team1:Spieler} durch einen zufälligen Spieler aus dem ausgewählten Team
        if (taskDescription.Contains("{Team1:Spieler}"))
        {
            string randomPlayer = GetRandomPlayer(selectedTeamName == team1Name ? team1Players : team2Players);
            taskDescription = taskDescription.Replace("{Team1:Spieler}", randomPlayer);
        }

        // Ersetze {Team2:Spieler} durch einen zufälligen Spieler aus dem anderen Team
        if (taskDescription.Contains("{Team2:Spieler}"))
        {
            string randomPlayer = GetRandomPlayer(selectedTeamName == team2Name ? team1Players : team2Players);
            taskDescription = taskDescription.Replace("{Team2:Spieler}", randomPlayer);
        }
    }

    private string GetRandomPlayer(List<string> teamPlayers)
    {
        if (teamPlayers.Count > 0)
        {
            int randomIndex = Random.Range(0, teamPlayers.Count);
            return teamPlayers[randomIndex];
        }
        else
        {
            return "Kein Spieler verfügbar";
        }
    }
}