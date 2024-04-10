using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;

public class TaskManager : MonoBehaviour
{
    public List<GameObject> taskPrefabs;
    public TextMeshProUGUI taskText;

    private List<string> playerNames = new List<string>();
    private List<string> driverNames = new List<string>();
    private int tasksCompleted = 0;
    private int maxTasks; // Zufällige Anzahl von Aufgaben zwischen 30 und 50
    private bool gameEnded = false;
    private bool hasDrivers = true;

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
        int playerCount = PlayerPrefs.GetInt("PlayerCount", 0);
        Debug.Log("Player count loaded: " + playerCount);
        for (int i = 0; i < playerCount; i++)
        {
            string playerName = PlayerPrefs.GetString("Player" + (i + 1));
            Debug.Log("Player " + (i + 1) + " loaded: " + playerName);

            bool isDriver = PlayerPrefs.GetInt(playerName + "_IsDriver", 0) == 1;
            if (isDriver)
            {
                driverNames.Add(playerName);
            }
            else
            {
                playerNames.Add(playerName);
            }
        }

        if (driverNames.Count == 0)
        {
            hasDrivers = false;
        }
    }

    private void RemoveDriversFromPlayerCount()
    {
        foreach (string driverName in driverNames)
        {
            playerNames.Remove(driverName);
        }

        // Aktualisieren des PlayerCounts nach dem Entfernen der Fahrer
        PlayerPrefs.SetInt("PlayerCount", playerNames.Count);
        for (int i = 0; i < playerNames.Count; i++)
        {
            PlayerPrefs.SetString("Player" + (i + 1), playerNames[i]);
        }
        PlayerPrefs.Save();
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

        // Ersetze Platzhalter durch zufällig ausgewählte Spieler und Fahrer
        HashSet<string> usedNames = new HashSet<string>(); // Um doppelte Namen zu verhindern

        for (int i = 1; i <= 4; i++)
        {
            string placeholder = "{Spieler" + i + "}";
            if (taskDescription.Contains(placeholder) && i <= playerCount)
            {
                string randomPlayer = GetRandomPlayer();
                while (usedNames.Contains(randomPlayer)) // Überprüfe, ob der Name bereits verwendet wurde
                {
                    randomPlayer = GetRandomPlayer(); // Wenn ja, wähle einen neuen zufälligen Spieler aus
                }
                usedNames.Add(randomPlayer); // Füge den Namen zur Liste der verwendeten Namen hinzu
                taskDescription = taskDescription.Replace(placeholder, randomPlayer);
            }
            else if (taskDescription.Contains("{Fahrer" + i + "}"))
            {
                if (hasDrivers)
                {
                    string randomDriver = GetRandomDriver();
                    while (usedNames.Contains(randomDriver)) // Überprüfe, ob der Name bereits verwendet wurde
                    {
                        randomDriver = GetRandomDriver(); // Wenn ja, wähle einen neuen zufälligen Fahrer aus
                    }
                    usedNames.Add(randomDriver); // Füge den Namen zur Liste der verwendeten Namen hinzu
                    taskDescription = taskDescription.Replace("{Fahrer" + i + "}", randomDriver);
                }
                else
                {
                    // Überspringe die Aufgabe mit {Fahrer}
                    ShowNextTask();
                    return;
                }
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

    private string GetRandomPlayer()
    {
        if (playerNames.Count > 0)
        {
            int randomIndex = Random.Range(0, playerNames.Count);
            return playerNames[randomIndex];
        }
        else
        {
            return "Keine Spieler verfügbar";
        }
    }

    private string GetRandomDriver()
    {
        if (driverNames.Count > 0)
        {
            int randomIndex = Random.Range(0, driverNames.Count);
            return driverNames[randomIndex];
        }
        else
        {
            return "Kein Fahrer verfügbar";
        }
    }
}