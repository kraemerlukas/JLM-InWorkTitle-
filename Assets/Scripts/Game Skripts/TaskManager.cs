using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;

public class TaskManager : MonoBehaviour
{
    public GameObject taskPrefabGroup; // Referenz auf die Prefab-Gruppe
    public TextMeshProUGUI taskText;

    private List<string> playerNames = new List<string>();
    private List<string> driverNames = new List<string>();
    private int tasksCompleted = 0;
    private int maxTasks; // Zuf�llige Anzahl von Aufgaben zwischen 30 und 50
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

            SceneManager.LoadScene("Menu"); // L�dt die Szene "Menu"
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

    private int skippedTasks = 0; // Z�hlvariable f�r �bersprungene Aufgaben

    private void ShowNextTask()
    {
        int playerCount = playerNames.Count;
        int driverCount = driverNames.Count;

        if (tasksCompleted - skippedTasks >= maxTasks)
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

        // Zuf�llige Auswahl einer Aufgabe aus den Kindobjekten der Prefab-Gruppe
        int randomTaskIndex = Random.Range(0, childObjects.Count);
        GameObject randomTaskPrefab = childObjects[randomTaskIndex];

        string taskDescription = randomTaskPrefab.GetComponentInChildren<TextMeshProUGUI>().text;

        // �berpr�fung, ob gen�gend Spieler und Fahrer f�r die Aufgabe vorhanden sind
        if (!HasEnoughPlayersAndDriversForTask(taskDescription, playerCount, driverCount))
        {
            skippedTasks++; // Erh�he die Anzahl der �bersprungenen Aufgaben
            ShowNextTask(); // Aufgabe �berspringen und neue anzeigen
            return;
        }

        // Ersetze Platzhalter durch zuf�llig ausgew�hlte Spieler und Fahrer
        ReplacePlaceholders(ref taskDescription, playerCount);

        taskText.text = taskDescription;
        tasksCompleted++;
    }

    private bool HasEnoughPlayersAndDriversForTask(string taskDescription, int playerCount, int driverCount)
    {
        int requiredPlayers = 0;
        int requiredDrivers = 0;

        for (int i = 1; i <= 4; i++)
        {
            string playerPlaceholder = "{Spieler" + i + "}";
            string driverPlaceholder = "{Fahrer" + i + "}";

            if (taskDescription.Contains(playerPlaceholder))
            {
                requiredPlayers++;
            }
            else if (taskDescription.Contains(driverPlaceholder))
            {
                requiredDrivers++;
            }
        }

        return requiredPlayers <= playerCount && requiredDrivers <= driverCount;
    }

    private void ReplacePlaceholders(ref string taskDescription, int playerCount)
    {
        HashSet<string> usedNames = new HashSet<string>(); // Um doppelte Namen zu verhindern

        for (int i = 1; i <= 4; i++)
        {
            string placeholder = "{Spieler" + i + "}";
            if (taskDescription.Contains(placeholder) && i <= playerCount)
            {
                string randomPlayer = GetRandomPlayer();
                while (usedNames.Contains(randomPlayer)) // �berpr�fe, ob der Name bereits verwendet wurde
                {
                    randomPlayer = GetRandomPlayer(); // Wenn ja, w�hle einen neuen zuf�lligen Spieler aus
                }
                usedNames.Add(randomPlayer); // F�ge den Namen zur Liste der verwendeten Namen hinzu
                taskDescription = taskDescription.Replace(placeholder, randomPlayer);
            }
            else if (taskDescription.Contains("{Fahrer" + i + "}"))
            {
                if (hasDrivers)
                {
                    string randomDriver = GetRandomDriver();
                    while (usedNames.Contains(randomDriver)) // �berpr�fe, ob der Name bereits verwendet wurde
                    {
                        randomDriver = GetRandomDriver(); // Wenn ja, w�hle einen neuen zuf�lligen Fahrer aus
                    }
                    usedNames.Add(randomDriver); // F�ge den Namen zur Liste der verwendeten Namen hinzu
                    taskDescription = taskDescription.Replace("{Fahrer" + i + "}", randomDriver);
                }
                else
                {
                    // �berspringe die Aufgabe mit {Fahrer}
                    ShowNextTask();
                    return;
                }
            }
            else if (taskDescription.Contains("{Allgemein}"))
            {
                taskDescription = taskDescription.Replace("{Allgemein}", "Alle");
            }
        }
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
            return "Keine Spieler verf�gbar";
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
            return "Kein Fahrer verf�gbar";
        }
    }
}
