using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;
using UnityEngine.UIElements;
using Unity.VisualScripting;

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

    public GameObject taskPrefabGroupNormal; // Referenz auf die Prefab-Gruppe für leichte Aufgaben
    public GameObject taskPrefabGroupHardcore; // Referenz auf die Prefab-Gruppe für mittelschwere Aufgaben
    public GameObject taskPrefabGroupBar; // Referenz auf die Prefab-Gruppe für schwere Aufgaben


    public GameObject exTaskGroup; // Referenz auf die Prefab-Gruppe für "Exen" Aufgaben
    public GameObject RoundTaskGroup; // Referenz auf die Prefab-Gruppe für "Exen" Aufgaben
    public GameObject DuelTaskGroup;
    public GameObject rulePTaskGroup;
    public GameObject RatherTaskGroup;


    public GameObject mainCamera; // Referenz auf die Hauptkamera
    public TextMeshProUGUI taskText;
    public TextMeshProUGUI title;
    public GameObject Ads;
 

    private List<string> playerNames = new List<string>();
    private List<string> driverNames = new List<string>();
    private int tasksCompleted = 0;
    private bool hasDrivers;
    private int maxTasks; // Zufällige Anzahl von Aufgaben zwischen 30 und 50
    private bool gameEnded = false;

    private GameObject selectedTaskPrefabGroup; // Ausgewählte Prefab-Gruppe für Aufgaben

    private int minDrinks; // Mindestanzahl an Schlücken
    private int maxDrinks; // Höchstanzahl an Schlücken

    private Color32 StandartColor;

    private bool isEx;
    private bool ruleActive = false; // Gibt an, ob eine Regel aktiv ist
    public string currentRulePlayer;
    private int taskCounter = 0; // Zähler für die Anzahl der Regel-Aufgaben

    private bool isTest = true;

    private void Start()
    {
        StandartColor = new Color32(6, 190, 149, 0);
        mainCamera.GetComponent<Camera>().backgroundColor = StandartColor;

        Screen.orientation = ScreenOrientation.LandscapeLeft;
        maxTasks = Random.Range(80, 111);
        LoadPlayerData();
        SetSelectedTaskPrefab();
        SetDrinkRange();
        ShowNextTask();

        // Debug-Log für die aktuelle Schwierigkeitsstufe
        Debug.Log("Aktuelle Schwierigkeitsstufe: " + PlayerPrefs.GetString("SelectedDifficulty", "Easy"));
    }

    private void Update()
    {
        if (isTest)
        {
            if (tasksCompleted == 20 || tasksCompleted == 35 || tasksCompleted == 60 || tasksCompleted == 70)
            {
                //Ads.GetComponent<InterstitialAdExample>().ShowAd();
            }
        }

        if (Input.GetMouseButtonDown(0) && !gameEnded)
        {
            //Ads.GetComponent<InterstitialAdExample>().WasTrue = false;
            ShowNextTask();

            if (taskCounter >= 10 && taskCounter <= 13 && ruleActive)
            {
                taskCounter = 0;
                ShowRuleEnd();
            }

            if(ruleActive)
            {
                taskCounter++;
            }
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
        string selectedDifficulty = PlayerPrefs.GetString("SelectedMode", "Normal");
        Debug.Log(PlayerPrefs.GetString("SelectedMode", "Normal"));
        switch (selectedDifficulty)
        {
            case "Normal":
                selectedTaskPrefabGroup = taskPrefabGroupNormal;
                break;
            case "Harcore":
                selectedTaskPrefabGroup = taskPrefabGroupHardcore;
                break;
            case "Bar":
                selectedTaskPrefabGroup = taskPrefabGroupBar;
                break;
            default:
                selectedTaskPrefabGroup = taskPrefabGroupNormal; // Standardmäßig Easy verwenden
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
        Debug.Log(tasksCompleted);
        int playerCount = playerNames.Count;

        if (tasksCompleted >= maxTasks)
        {
            taskText.text = "Runde Beendet!";
            gameEnded = true;
            return;
        }

        // Überprüfen, ob eine Special-Aufgabe angezeigt werden soll
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

    private bool ShouldShowSpecialTask()
    {
        // Zufällig entscheiden, ob eine Special-Aufgabe angezeigt werden soll
        int randomNumber = Random.Range(0, 500); // Eine größere Reichweite für die Genauigkeit
        if (randomNumber < 15) // Zum Beispiel nur 0,5% Wahrscheinlichkeit für "EX"
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

                if(!ruleActive)
                {
                    title.gameObject.SetActive(true);
                    title.text = "REGEL";
                    mainCamera.GetComponent<Camera>().backgroundColor = Color.yellow;
                    ShowRuleTask();
                    break;
                }
                 else
                {
                    ShowNextTask();
                    break;
                }
             


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

 
    private void ShowRuleTask()
        {
            // Eine zufällige Aufgabe aus der "rulePTaskGroup" auswählen und anzeigen
            Transform[] ruleTaskTransforms = rulePTaskGroup.GetComponentsInChildren<Transform>();
            List<GameObject> ruleTasks = new List<GameObject>();
            foreach (Transform child in ruleTaskTransforms)
            {
                if (child.gameObject != rulePTaskGroup)
                {
                    ruleTasks.Add(child.gameObject);
                }
            }

            int randomRuleTaskIndex = Random.Range(0, ruleTasks.Count);
            GameObject randomRuleTaskPrefab = ruleTasks[randomRuleTaskIndex];

            string ruleTaskDescription = randomRuleTaskPrefab.GetComponentInChildren<TextMeshProUGUI>().text;
        currentRulePlayer = GetRandomPlayer();
        // Direkt den ausgewählten Spieler in die Regel einsetzen
        ruleTaskDescription = ruleTaskDescription.Replace("{Spieler1}", currentRulePlayer);

            taskText.text = ruleTaskDescription;

            ruleActive = true; // Aktiviere das Flag, dass eine Regel aktiv ist

            tasksCompleted++;
        }

    private void ShowRuleEnd()
    {
        title.gameObject.SetActive(true);
        title.text = "REGELENDE";
        mainCamera.GetComponent<Camera>().backgroundColor = Color.yellow;

        // Text für das Ende der Regel-Aufgaben anzeigen
        string ruleEndText = "{Spieler1}, deine Regel ist vorbei!";
        ruleEndText = ruleEndText.Replace("{Spieler1}", currentRulePlayer);
        taskText.text = ruleEndText;

        ruleActive = false; // Deaktiviere das Flag, dass eine Regel aktiv ist
    }


    private void ShowExenTask()
    {
        // Eine zufällige Aufgabe aus der "ExTaskGroup" auswählen und anzeigen
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
        isEx = false;
        tasksCompleted++;
    }

    private void ShowGameTask()
    {
        // Eine zufällige Aufgabe aus der "ExTaskGroup" auswählen und anzeigen
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
        tasksCompleted++;
    }

    private void ShowDuelTask()
    {
        // Eine zufällige Aufgabe aus der "ExTaskGroup" auswählen und anzeigen
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
        tasksCompleted++;
    }
    private void ShowRatherTask()
    {
        // Eine zufällige Aufgabe aus der "ExTaskGroup" auswählen und anzeigen
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
        tasksCompleted++;
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
        if (!taskDescription.Contains("{Fahrer"))
        {
            // Die Aufgabe enthält keine Fahrer-Platzhalter, daher wird davon ausgegangen, dass genug Fahrer vorhanden sind
            return true;
        }

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