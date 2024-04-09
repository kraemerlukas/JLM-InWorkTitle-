using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;

public class TaskManager : MonoBehaviour
{
    public List<GameObject> taskPrefabs;
    public TextMeshProUGUI taskText;

    private List<string> playerNames = new List<string>();
    private int tasksCompleted = 0;
    private int maxTasks; // Zufällige Anzahl von Aufgaben zwischen 30 und 50
    private bool gameEnded = false;

    private void Start()
    {
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
            SceneManager.LoadScene("Menu"); // Lädt die Szene "Menu"
        }
    }

    private void LoadPlayerData()
    {
        int playerCount = PlayerPrefs.GetInt("PlayerCount", 0);
        Debug.Log("Player count loaded: " + playerCount);
        for (int i = 0; i < playerCount; i++)
        {
            string playerName = PlayerPrefs.GetString("Player" + (i + 1));
            Debug.Log("Player " + (i + 1) + " loaded: " + playerName);
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

        int randomTaskIndex = Random.Range(0, taskPrefabs.Count);
        GameObject randomTaskPrefab = taskPrefabs[randomTaskIndex];

        string taskDescription = randomTaskPrefab.GetComponentInChildren<TextMeshProUGUI>().text;

        while (!HasEnoughPlayers(taskDescription, playerCount))
        {
            randomTaskIndex = Random.Range(0, taskPrefabs.Count);
            randomTaskPrefab = taskPrefabs[randomTaskIndex];
            taskDescription = randomTaskPrefab.GetComponentInChildren<TextMeshProUGUI>().text;
        }

        List<string> selectedPlayers = new List<string>();

        // Ersetze Platzhalter durch zufällig ausgewählte Spieler
        for (int i = 1; i <= 4; i++)
        {
            string placeholder = "{Spieler" + i + "}";
            if (taskDescription.Contains(placeholder) && i <= playerCount)
            {
                string randomPlayer = GetRandomPlayer(selectedPlayers);
                selectedPlayers.Add(randomPlayer);
                taskDescription = taskDescription.Replace(placeholder, randomPlayer);
            }
            else if (taskDescription.Contains("{Allgemein}"))
            {
                taskDescription = taskDescription.Replace("{Allgemein}", "Alle");
            }
        }

        taskText.text = taskDescription;
        tasksCompleted++;
    }

    private bool HasEnoughPlayers(string taskDescription, int playerCount)
    {
        int numPlaceholders = 0;
        for (int i = 1; i <= 4; i++)
        {
            string placeholder = "{Spieler" + i + "}";
            if (taskDescription.Contains(placeholder))
            {
                numPlaceholders++;
            }
        }
        return numPlaceholders <= playerCount;
    }

    private string GetRandomPlayer(List<string> excludePlayers)
    {
        List<string> availablePlayers = new List<string>(playerNames);
        foreach (string player in excludePlayers)
        {
            availablePlayers.Remove(player);
        }

        if (availablePlayers.Count > 0)
        {
            int randomIndex = Random.Range(0, availablePlayers.Count);
            return availablePlayers[randomIndex];
        }
        else
        {
            return "Keine Spieler verfügbar";
        }
    }
}
