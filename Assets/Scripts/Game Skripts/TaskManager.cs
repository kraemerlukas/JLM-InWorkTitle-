using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;
using UnityEngine.UIElements;

public enum SpecialTaskType
{
    None,
    Exen,
    Regel,
    Runde,
    Duell,
    Lieber
}

public class TaskManager : MonoBehaviour
{
    public GameObject taskPrefabGroupEasy; // Referenz auf die Prefab-Gruppe f�r leichte Aufgaben
    public GameObject taskPrefabGroupMedium; // Referenz auf die Prefab-Gruppe f�r mittelschwere Aufgaben
    public GameObject taskPrefabGroupHard; // Referenz auf die Prefab-Gruppe f�r schwere Aufgaben
    public GameObject exTaskGroup; // Referenz auf die Prefab-Gruppe f�r "Exen" Aufgaben
    public GameObject RoundTaskGroup; // Referenz auf die Prefab-Gruppe f�r "Exen" Aufgaben
    public GameObject DuelTaskGroup;
    public GameObject RatherTaskGroup;
    public GameObject mainCamera; // Referenz auf die Hauptkamera
    public TextMeshProUGUI taskText;
    public GameObject Ads;
    public TextMeshProUGUI title;
    private List<string> playerNames = new List<string>();
    private List<string> driverNames = new List<string>();
    private int tasksCompleted = 0;
    private bool hasDrivers;
    private int maxTasks; // Zuf�llige Anzahl von Aufgaben zwischen 30 und 50
    private bool gameEnded = false;
    private GameObject selectedTaskPrefabGroup; // Ausgew�hlte Prefab-Gruppe f�r Aufgaben
    private int minDrinks; // Mindestanzahl an Schl�cken
    private int maxDrinks; // H�chstanzahl an Schl�cken
    private Color32 StandartColor;
    private bool isEx;

    private void Start()
    {
        StandartColor = new Color32(6,190,149,0);
        mainCamera.GetComponent<Camera>().backgroundColor = StandartColor;

        Screen.orientation = ScreenOrientation.LandscapeLeft;
        maxTasks = Random.Range(60, 81);
        LoadPlayerData();
        SetSelectedTaskPrefab();
        SetDrinkRange();
        ShowNextTask();

        // Debug-Log f�r die aktuelle Schwierigkeitsstufe
        Debug.Log("Aktuelle Schwierigkeitsstufe: " + PlayerPrefs.GetString("SelectedDifficulty", "Easy"));
    }
    
    private void Update()
    {
        if (tasksCompleted == 15 || tasksCompleted == 35 || tasksCompleted == 60)
        {
            Ads.GetComponent<InterstitialAdExample>().ShowAd();
        }

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
                selectedTaskPrefabGroup = taskPrefabGroupEasy; // Standardm��ig Easy verwenden
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

        // �berpr�fen, ob eine Special-Aufgabe angezeigt werden soll
        if (ShouldShowSpecialTask())
        {
            ShowSpecialTask();
            return;
        }

        mainCamera.GetComponent<Camera>().backgroundColor = StandartColor;
        title.gameObject.SetActive(false);

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
                    // Fahrer sind nicht verf�gbar, �berspringe diese Aufgabe
                    continue;
                }
            }
            else
            {
                // Nicht gen�gend Spieler f�r diese Aufgabe, �berspringe sie
                continue;
            }
        }
    }

    private bool ShouldShowSpecialTask()
    {
        // Zuf�llig entscheiden, ob eine Special-Aufgabe angezeigt werden soll
        int randomNumber = Random.Range(0, 500); // Eine gr��ere Reichweite f�r die Genauigkeit
        Debug.Log(randomNumber);
        if (randomNumber < 15) // Zum Beispiel nur 0,5% Wahrscheinlichkeit f�r "EX"
        {
            isEx = true;          
            return true;

        }
        else if (randomNumber <= 100) 
        {
            isEx = false;

            return true;
        }
        return false;
    }

    private void ShowSpecialTask()
    {
        SpecialTaskType specialType;
        
        if (!isEx)
        {
          specialType = (SpecialTaskType)Random.Range(2, 6); 

        }
        else
        {
            specialType = SpecialTaskType.Exen;
        }
        

        // Je nach Spezialaufgaben-Typ die entsprechende Aufgabe anzeigen
        switch (specialType)
        {
            case SpecialTaskType.Exen:
                title.gameObject.SetActive(true);
                title.text = "EX";
                mainCamera.GetComponent<Camera>().backgroundColor = Color.red;
                ShowExenTask();
                break;
            case SpecialTaskType.Regel:
                mainCamera.GetComponent<Camera>().backgroundColor = StandartColor;               
                ShowNextTask();
                // Implementiere die Anzeige einer "Regel" Aufgabe hier
                break;
            case SpecialTaskType.Runde:
                title.gameObject.SetActive(true);
                title.text = "RUNDE";
                mainCamera.GetComponent<Camera>().backgroundColor = Color.blue;
                ShowGameTask();
                // Implementiere die Anzeige einer "Spiel" Aufgabe hier
                break;
            case SpecialTaskType.Duell:
                title.gameObject.SetActive(true);
                title.text = "Duell";
                mainCamera.GetComponent<Camera>().backgroundColor = Color.magenta;
                ShowDuelTask();
                // Implementiere die Anzeige einer "Spiel" Aufgabe hier
                break;
            case SpecialTaskType.Lieber:
                title.gameObject.SetActive(true);
                title.text = "Entweder..";
                mainCamera.GetComponent<Camera>().backgroundColor = Color.green;
                ShowRatherTask();
                // Implementiere die Anzeige einer "Spiel" Aufgabe hier
                break;
            default:
                mainCamera.GetComponent<Camera>().backgroundColor = StandartColor;

                ShowNextTask();
                break;
        }
    }

    private void ShowExenTask()
    {
        // Eine zuf�llige Aufgabe aus der "ExTaskGroup" ausw�hlen und anzeigen
        Transform[] exTaskTransforms = exTaskGroup.GetComponentsInChildren<Transform>();
        List<GameObject> exTasks = new List<GameObject>();
        foreach (Transform child in exTaskTransforms)
        {
            if (child.gameObject != exTaskGroup)
            {
                exTasks.Add(child.gameObject);
            }
        }

        int randomExTaskIndex = Random.Range(0, exTasks.Count);
        GameObject randomExTaskPrefab = exTasks[randomExTaskIndex];

        string exTaskDescription = randomExTaskPrefab.GetComponentInChildren<TextMeshProUGUI>().text;

        ReplacePlaceholders(ref exTaskDescription, playerNames.Count);

        taskText.text = exTaskDescription;
    }

    private void ShowGameTask()
    {
        // Eine zuf�llige Aufgabe aus der "ExTaskGroup" ausw�hlen und anzeigen
        Transform[] RoundTaskTransforms = RoundTaskGroup.GetComponentsInChildren<Transform>();
        List<GameObject> RoundTasks = new List<GameObject>();
        foreach (Transform child in RoundTaskTransforms)
        {
            if (child.gameObject != exTaskGroup)
            {
                RoundTasks.Add(child.gameObject);
            }
        }

        int randomExTaskIndex = Random.Range(0, RoundTasks.Count);
        GameObject randomGameTaskPrefab = RoundTasks[randomExTaskIndex];

        string RoundTaskDescription = randomGameTaskPrefab.GetComponentInChildren<TextMeshProUGUI>().text;

        ReplacePlaceholders(ref RoundTaskDescription, playerNames.Count);

        taskText.text = RoundTaskDescription;
    }

    private void ShowDuelTask()
    {
        // Eine zuf�llige Aufgabe aus der "ExTaskGroup" ausw�hlen und anzeigen
        Transform[] DuelTaskTransforms = DuelTaskGroup.GetComponentsInChildren<Transform>();
        List<GameObject> DuelTasks = new List<GameObject>();
        foreach (Transform child in DuelTaskTransforms)
        {
            if (child.gameObject != exTaskGroup)
            {
                DuelTasks.Add(child.gameObject);
            }
        }

        int randomExTaskIndex = Random.Range(0, DuelTasks.Count);
        GameObject randomDuelTaskPrefab = DuelTasks[randomExTaskIndex];

        string DuelTaskDescription = randomDuelTaskPrefab.GetComponentInChildren<TextMeshProUGUI>().text;

        ReplacePlaceholders(ref DuelTaskDescription, playerNames.Count);

        taskText.text = DuelTaskDescription;
    }
    private void ShowRatherTask()
    {
        // Eine zuf�llige Aufgabe aus der "ExTaskGroup" ausw�hlen und anzeigen
        Transform[] RatherTaskTransforms = RatherTaskGroup.GetComponentsInChildren<Transform>();
        List<GameObject> RatherTasks = new List<GameObject>();
        foreach (Transform child in RatherTaskTransforms)
        {
            if (child.gameObject != exTaskGroup)
            {
                RatherTasks.Add(child.gameObject);
            }
        }

        int randomExTaskIndex = Random.Range(0, RatherTasks.Count);
        GameObject randomRatherTaskPrefab = RatherTasks[randomExTaskIndex];

        string RatherTaskDescription = randomRatherTaskPrefab.GetComponentInChildren<TextMeshProUGUI>().text;

        ReplacePlaceholders(ref RatherTaskDescription, playerNames.Count);

        taskText.text = RatherTaskDescription;
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
            else if (taskDescription.Contains("{Schl�cke}"))
            {
                int randomDrinks = Random.Range(minDrinks, maxDrinks + 1);
                taskDescription = taskDescription.Replace("{Schl�cke}", randomDrinks.ToString());
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