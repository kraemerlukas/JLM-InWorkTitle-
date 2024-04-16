using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;

public class TaskManager : MonoBehaviour
{
    public GameObject taskPrefabGroupEasy; // Referenz auf die Prefab-Gruppe für leichte Aufgaben
    public GameObject taskPrefabGroupMedium; // Referenz auf die Prefab-Gruppe für mittelschwere Aufgaben
    public GameObject taskPrefabGroupHard; // Referenz auf die Prefab-Gruppe für schwere Aufgaben
    public TextMeshProUGUI taskText;

    private List<string> playerNames = new List<string>();
    private List<string> driverNames = new List<string>();
    private int tasksCompleted = 0;
    private bool hasDrivers;
    private int maxTasks; // Zufällige Anzahl von Aufgaben zwischen 30 und 50
    private bool gameEnded = false;
    private GameObject selectedTaskPrefabGroup; // Ausgewählte Prefab-Gruppe für Aufgaben
    private int minDrinks; // Mindestanzahl an Schlücken
    private int maxDrinks; // Höchstanzahl an Schlücken

    private void Start()
    {
        Screen.orientation = ScreenOrientation.LandscapeLeft;
        maxTasks = Random.Range(30, 51);
        LoadPlayerData();
        SetSelectedTaskPrefab();
        SetDrinkRange();
        ShowNextTask();

        // Debug-Log für die aktuelle Schwierigkeitsstufe
        Debug.Log("Aktuelle Schwierigkeitsstufe: " + PlayerPrefs.GetString("SelectedDifficulty", "Easy"));
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
        for (int i = 0; i < playerCount; i++)
        {
            string playerName = PlayerPrefs.GetString("Player" + (i + 1));
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

        hasDrivers = driverNames.Count > 0; // Setze hasDrivers auf true, wenn Fahrer vorhanden sind
    }

    private void SetSelectedTaskPrefab()
    {
        string selectedDifficulty = PlayerPrefs.GetString("SelectedDifficulty", "Easy");

        switch (selectedDifficulty)
        {
            case "Easy":
                selectedTaskPrefabGroup = taskPrefabGroupEasy;
                break;
            case "Medium":
                selectedTaskPrefabGroup = taskPrefabGroupMedium;
                break;
            case "Hard":
                selectedTaskPrefabGroup = taskPrefabGroupHard;
                break;
            default:
                selectedTaskPrefabGroup = taskPrefabGroupEasy; // Standardmäßig Easy verwenden
                break;
        }
    }

    private void SetDrinkRange()
    {
        string selectedDifficulty = PlayerPrefs.GetString("SelectedDifficulty", "Easy");

        switch (selectedDifficulty)
        {
            case "Easy":
                minDrinks = 1;
                maxDrinks = 5;
                break;
            case "Medium":
                minDrinks = 2;
                maxDrinks = 7;
                break;
            case "Hard":
                minDrinks = 3;
                maxDrinks = 9;
                break;
            default:
                minDrinks = 1;
                maxDrinks = 5;
                break;
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

        Transform[] childTransforms = selectedTaskPrefabGroup.GetComponentsInChildren<Transform>();
        List<GameObject> childObjects = new List<GameObject>();
        foreach (Transform child in childTransforms)
        {
            if (child.gameObject != selectedTaskPrefabGroup)
            {
                childObjects.Add(child.gameObject);
            }
        }

        bool taskDisplayed = false;
        while (!taskDisplayed)
        {
            int randomTaskIndex = Random.Range(0, childObjects.Count);
            GameObject randomTaskPrefab = childObjects[randomTaskIndex];

            string taskDescription = randomTaskPrefab.GetComponentInChildren<TextMeshProUGUI>().text;

            if (HasEnoughPlayers(taskDescription))
            {
                if (HasEnoughDrivers(taskDescription))
                {
                    ReplacePlaceholders(ref taskDescription, playerCount);
                    taskText.text = taskDescription;
                    tasksCompleted++;
                    taskDisplayed = true;
                }
                else
                {
                    // Fahrer sind nicht verfügbar, überspringe diese Aufgabe
                    continue;
                }
            }
            else
            {
                // Nicht genügend Spieler für diese Aufgabe, überspringe sie
                continue;
            }
        }
    }

    private bool HasEnoughPlayers(string taskDescription)
    {
        int requiredPlayerCount = 0;
        for (int i = 1; i <= 4; i++)
        {
            string placeholder = "{Spieler" + i + "}";
            if (taskDescription.Contains(placeholder))
            {
                requiredPlayerCount++;
            }
        }

        return requiredPlayerCount <= playerNames.Count;
    }

    private bool HasEnoughDrivers(string taskDescription)
    {
        int requiredDriverCount = 0;
        for (int i = 1; i <= 4; i++)
        {
            string placeholder = "{Fahrer" + i + "}";
            if (taskDescription.Contains(placeholder))
            {
                requiredDriverCount++;
            }
        }

        return requiredDriverCount <= driverNames.Count;
    }

    private void ReplacePlaceholders(ref string taskDescription, int playerCount)
    {
        HashSet<string> usedNames = new HashSet<string>();

        for (int i = 1; i <= 4; i++)
        {
            string placeholder = "{Spieler" + i + "}";
            if (taskDescription.Contains(placeholder) && i <= playerCount)
            {
                string randomPlayer = GetRandomPlayer();
                while (usedNames.Contains(randomPlayer))
                {
                    randomPlayer = GetRandomPlayer();
                }
                usedNames.Add(randomPlayer);
                taskDescription = taskDescription.Replace(placeholder, randomPlayer);
            }
            else if (taskDescription.Contains("{Fahrer" + i + "}"))
            {
                if (hasDrivers)
                {
                    string randomDriver = GetRandomDriver();
                    while (usedNames.Contains(randomDriver))
                    {
                        randomDriver = GetRandomDriver();
                    }
                    usedNames.Add(randomDriver);
                    taskDescription = taskDescription.Replace("{Fahrer" + i + "}", randomDriver);
                }
                else
                {
                    ShowNextTask();
                    return;
                }
            }
            else if (taskDescription.Contains("{Allgemein}"))
            {
                taskDescription = taskDescription.Replace("{Allgemein}", "Alle");
            }
            else if (taskDescription.Contains("{Schlücke}"))
            {
                int randomDrinks = Random.Range(minDrinks, maxDrinks + 1);
                taskDescription = taskDescription.Replace("{Schlücke}", randomDrinks.ToString());
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