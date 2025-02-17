using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class TaskManager : MonoBehaviour
{
    public TextMeshProUGUI taskText;
    public TextMeshProUGUI title;
    public Camera mainCamera;

    private List<string> normalTasks = new List<string>();
    private List<string> exTasks = new List<string>();
    private List<string> duellTasks = new List<string>();
    private List<string> regelTasks = new List<string>();
    private List<string> rundeTasks = new List<string>();
    private List<string> lieberTasks = new List<string>();

    private Color defaultColor;
    private int tasksCompleted = 0;
    private bool isEx = false;
    private int maxTasks;
    private bool gameEnded = false;

    private int minDrinks;
    private int maxDrinks;

    private List<string> playerNames = new List<string>();
    private List<string> driverNames = new List<string>();
    private List<string> nonDriverPlayers = new List<string>();
    private HashSet<string> usedPlayers = new HashSet<string>();

    private bool ruleActive = false;
    private int ruleCountdown = 0;
    private string currentRulePlayer = "";

    private enum SpecialTaskType { Exen, Regel, Runde, Duell, Lieber }

    private void Start()
    {
        defaultColor = mainCamera.backgroundColor;
        LoadPlayers();
        LoadAllTasks();
        SetDrinkRange();
        maxTasks = Random.Range(30, 50);
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
        }
    }

    private void LoadPlayers()
    {
        playerNames.Clear();
        driverNames.Clear();
        nonDriverPlayers.Clear();
        usedPlayers.Clear();

        int playerCount = PlayerPrefs.GetInt("PlayerCount", 0);
        for (int i = 1; i <= playerCount; i++)
        {
            string playerName = PlayerPrefs.GetString("Player" + i, "").Trim();
            int isDriver = PlayerPrefs.GetInt(playerName + "_IsDriver", 0);

            if (!string.IsNullOrEmpty(playerName))
            {
                playerNames.Add(playerName);
                if (isDriver == 1)
                {
                    driverNames.Add(playerName);
                }
                else
                {
                    nonDriverPlayers.Add(playerName);
                }
            }
        }
    }

    private void LoadAllTasks()
    {
        normalTasks = LoadTasksFromFile("normal.txt");
        exTasks = LoadTasksFromFile("ex.txt");
        duellTasks = LoadTasksFromFile("duell.txt");
        regelTasks = LoadTasksFromFile("regel.txt");
        rundeTasks = LoadTasksFromFile("runde.txt");
        lieberTasks = LoadTasksFromFile("lieber.txt");
    }

    private List<string> LoadTasksFromFile(string fileName)
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, fileName);
        return File.Exists(filePath) ? File.ReadAllLines(filePath).ToList() : new List<string>();
    }

    private void SetDrinkRange()
    {
        string difficulty = PlayerPrefs.GetString("SelectedDifficulty", "Easy");

        switch (difficulty)
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
                maxDrinks = 10;
                break;
            default:
                minDrinks = 1;
                maxDrinks = 5;
                break;
        }
    }

    public void ShowNextTask()
    {
        usedPlayers.Clear();

        if (tasksCompleted >= maxTasks)
        {
            taskText.text = "🎉 Runde Beendet! Tippe zum Beenden.";
            gameEnded = true;
            return;
        }

        title.gameObject.SetActive(false);
        mainCamera.backgroundColor = defaultColor;

        if (ruleActive)
        {
            ruleCountdown--;
            if (ruleCountdown <= 0)
            {
                ShowRuleEnd();
                return;
            }
        }

        if (Random.value < 0.25f) // 25% Chance für Special-Aufgabe
        {
            ShowSpecialTask();
            return;
        }

        if (normalTasks.Count == 0)
        {
            taskText.text = "❌ Keine Aufgaben verfügbar!";
            return;
        }

        string selectedTask = normalTasks[Random.Range(0, normalTasks.Count)];
        ReplacePlaceholders(ref selectedTask);
        taskText.text = selectedTask;
        tasksCompleted++;
    }

    private void ShowSpecialTask()
    {
        SpecialTaskType specialType = isEx ? SpecialTaskType.Exen : (SpecialTaskType)Random.Range(1, 5);

        if (specialType == SpecialTaskType.Regel && ruleActive)
        {
            ShowNextTask(); // Falls eine Regel aktiv ist, keine neue Regel starten
            return;
        }

        List<string> taskPool;
        string titleText = "";
        Color backgroundColor = defaultColor;

        switch (specialType)
        {
            case SpecialTaskType.Exen:
                if (Random.value > 0.01f) // 1% Wahrscheinlichkeit für EX
                {
                    ShowNextTask();
                    return;
                }
                taskPool = exTasks;
                titleText = "EX";
                backgroundColor = Color.red;
                break;
            case SpecialTaskType.Regel:
                taskPool = regelTasks;
                titleText = "REGEL";
                backgroundColor = Color.yellow;
                ruleActive = true;
                ruleCountdown = Random.Range(10, 20);
                currentRulePlayer = GetUniqueNonDriver();
                break;
            case SpecialTaskType.Runde:
                taskPool = rundeTasks;
                titleText = "RUNDE";
                backgroundColor = Color.blue;
                break;
            case SpecialTaskType.Duell:
                taskPool = duellTasks;
                titleText = "DUELL";
                backgroundColor = Color.magenta;
                break;
            case SpecialTaskType.Lieber:
                taskPool = lieberTasks;
                titleText = "ENTWEDER/ODER";
                backgroundColor = Color.green;
                break;
            default:
                ShowNextTask();
                return;
        }

        if (taskPool.Count == 0)
        {
            ShowNextTask();
            return;
        }

        title.gameObject.SetActive(true);
        title.text = titleText;
        mainCamera.backgroundColor = backgroundColor;

        string selectedTask = taskPool[Random.Range(0, taskPool.Count)];
        ReplacePlaceholders(ref selectedTask);
        taskText.text = selectedTask;
        tasksCompleted++;
    }

    private void ShowRuleEnd()
    {
        title.gameObject.SetActive(true);
        title.text = "REGEL ENDE";
        mainCamera.backgroundColor = Color.yellow;

        string ruleEndText = $"{currentRulePlayer}, deine Regel ist vorbei!";
        taskText.text = ruleEndText;

        ruleActive = false;
    }

    private void ReplacePlaceholders(ref string taskDescription)
    {
        if (taskDescription.Contains("{Spieler1}"))
            taskDescription = taskDescription.Replace("{Spieler1}", GetUniqueNonDriver());

        if (taskDescription.Contains("{Spieler2}"))
            taskDescription = taskDescription.Replace("{Spieler2}", GetUniqueNonDriver());

        if (taskDescription.Contains("{Fahrer1}"))
            taskDescription = taskDescription.Replace("{Fahrer1}", GetUniqueDriver());

        if (taskDescription.Contains("{Schlucke}"))
        {
            int randomDrinks = Random.Range(minDrinks, maxDrinks + 1);
            taskDescription = taskDescription.Replace("{Schlucke}", randomDrinks.ToString());
        }
    }
          private string GetUniqueNonDriver()
    {
        if (nonDriverPlayers.Count == 0) return "Niemand";

        List<string> availablePlayers = nonDriverPlayers.Except(usedPlayers).ToList();
        if (availablePlayers.Count == 0) availablePlayers = new List<string>(nonDriverPlayers);

        string selectedPlayer = availablePlayers[Random.Range(0, availablePlayers.Count)];
        usedPlayers.Add(selectedPlayer);
        return selectedPlayer;
    }

    private string GetUniqueDriver()
    {
        if (driverNames.Count == 0) return "Kein Fahrer";

        List<string> availableDrivers = driverNames.Except(usedPlayers).ToList();
        if (availableDrivers.Count == 0) availableDrivers = new List<string>(driverNames);

        string selectedDriver = availableDrivers[Random.Range(0, availableDrivers.Count)];
        usedPlayers.Add(selectedDriver);
        return selectedDriver;
    }

    private void EndRound()
    {
        SceneManager.LoadScene("Menu");
    }
}
