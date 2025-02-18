using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class TeamTaskManager : MonoBehaviour
{
    public TextMeshProUGUI taskText;
    public TextMeshProUGUI title;
    public Camera mainCamera;

    private List<string> teamTasks = new List<string>();
    private List<string> usedTasks = new List<string>();
    private Color defaultColor;
    private int tasksCompleted = 0;
    private int maxTasks;
    private bool gameEnded = false;

    private int minDrinks;
    private int maxDrinks;

    private List<string> team1Players = new List<string>();
    private List<string> team2Players = new List<string>();

    private void Start()
    {
        defaultColor = mainCamera.backgroundColor;
        LoadPlayerData();
        LoadTasksFromFile();
        SetDrinkRange();
        maxTasks = Random.Range(5, 8);
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

    private void LoadPlayerData()
    {
        team1Players.Clear();
        team2Players.Clear();
        int team1Count = PlayerPrefs.GetInt("Team1Count", 0);
        for (int i = 0; i < team1Count; i++)
        {
            team1Players.Add(PlayerPrefs.GetString("Team1Player" + i));
        }

        int team2Count = PlayerPrefs.GetInt("Team2Count", 0);
        for (int i = 0; i < team2Count; i++)
        {
            team2Players.Add(PlayerPrefs.GetString("Team2Player" + i));
        }
    }

    private void LoadTasksFromFile()
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, "team.txt");
        if (File.Exists(filePath))
        {
            teamTasks = File.ReadAllLines(filePath).ToList();
        }
        else
        {
            Debug.LogError("❌ Datei 'team.txt' nicht gefunden!");
            teamTasks = new List<string>();
        }
    }

    private void SetDrinkRange()
    {
        string difficulty = PlayerPrefs.GetString("SelectedDifficulty", "Easy");
        switch (difficulty)
        {
            case "Easy":
                minDrinks = 1; maxDrinks = 5; break;
            case "Medium":
                minDrinks = 2; maxDrinks = 7; break;
            case "Hard":
                minDrinks = 3; maxDrinks = 10; break;
            default:
                minDrinks = 1; maxDrinks = 5; break;
        }
    }

    private void ShowNextTask()
    {
        if (tasksCompleted >= maxTasks)
        {
            title.gameObject.SetActive(true);
            title.text = "SPIELENDE";
            mainCamera.backgroundColor = Color.red;
            taskText.text = "🎉 Runde Beendet! Tippe zum Beenden.";
            gameEnded = true;
            return;
        }

        if (teamTasks.Count == 0)
        {
            taskText.text = "❌ Keine Aufgaben verfügbar!";
            return;
        }

        string selectedTask = GetNextUniqueTask();
        ReplacePlaceholders(ref selectedTask);
        taskText.text = selectedTask;
        tasksCompleted++;
    }

    private string GetNextUniqueTask()
    {
        if (usedTasks.Count >= teamTasks.Count) usedTasks.Clear();

        string task;
        do
        {
            task = teamTasks[Random.Range(0, teamTasks.Count)];
        } while (usedTasks.Contains(task));

        usedTasks.Add(task);
        return task;
    }

    private void ReplacePlaceholders(ref string taskDescription)
    {
        if (taskDescription.Contains("{Team1}"))
            taskDescription = taskDescription.Replace("{Team1}", PlayerPrefs.GetString("Team1Name", "Team 1"));

        if (taskDescription.Contains("{Team2}"))
            taskDescription = taskDescription.Replace("{Team2}", PlayerPrefs.GetString("Team2Name", "Team 2"));

        if (taskDescription.Contains("{Team1:Spieler}"))
            taskDescription = taskDescription.Replace("{Team1:Spieler}", GetRandomPlayer(team1Players));

        if (taskDescription.Contains("{Team2:Spieler}"))
            taskDescription = taskDescription.Replace("{Team2:Spieler}", GetRandomPlayer(team2Players));

        if (taskDescription.Contains("{Schlucke}"))
        {
            int randomDrinks = Random.Range(minDrinks, maxDrinks + 1);
            taskDescription = taskDescription.Replace("{Schlucke}", randomDrinks.ToString());
        }
    }

    private string GetRandomPlayer(List<string> teamPlayers)
    {
        if (teamPlayers.Count > 0)
            return teamPlayers[Random.Range(0, teamPlayers.Count)];

        return "Kein Spieler verfügbar";
    }

    private void EndRound()
    {
        SceneManager.LoadScene("Menu");
    }
}
