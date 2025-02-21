using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class TaskManager : MonoBehaviour
{
    public TextMeshProUGUI taskText;
    public TextMeshProUGUI title;
    public Camera mainCamera;
    private List<string> lastDrinkTasks = new List<string>(); // Speichert die letzten Schluck-Aufgaben

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
    private List<string> usedTasks = new List<string>(); // Liste für bereits gespielte Aufgaben
    private bool lastDrinkEventShown = false; // Prüft, ob das letzte Event bereits angezeigt wurde
    public string gameMode; // Modi: Normal, Hardcore, Bar (erweiterbar)

    private enum SpecialTaskType { Exen, Regel, Runde, Duell, Lieber }

    private void Start()
    {
        Screen.orientation = ScreenOrientation.LandscapeLeft;
        title.gameObject.SetActive(true);
        defaultColor = mainCamera.backgroundColor;
        LoadPlayers();
        normalTasks = LoadTasksFromTextAsset("Tasks/normal");
        exTasks = LoadTasksFromTextAsset("Tasks/ex");
        duellTasks = LoadTasksFromTextAsset("Tasks/duell");

        SetDrinkRange();
        maxTasks = Random.Range(80, 120);
        ShowNextTask();
        LoadLastDrinkTasks();
    }
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // Falls der Touch auf einem UI-Button stattfindet, überspringe die weitere Verarbeitung.
            if (UIInputBlocker.Instance != null && UIInputBlocker.Instance.IsPointerOverUIButton())
                return;

            // Normale Spiel-Logik: z.B. nächsten Task anzeigen oder Runde beenden.
            if (gameEnded)
                EndRound();
            else
                ShowNextTask();
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

    private List<string> LoadTasksFromTextAsset(string assetPath)
    {
        // assetPath z. B. "Tasks/normal" ohne Dateiendung
        TextAsset textAsset = Resources.Load<TextAsset>(assetPath);
        if (textAsset != null)
        {
            // Splitte den Text in einzelne Zeilen
            return textAsset.text.Split(new[] { "\r\n", "\r", "\n" }, System.StringSplitOptions.None).ToList();
        }
        else
        {
            Debug.LogError($"❌ TextAsset '{assetPath}' nicht gefunden!");
            return new List<string>();
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
                maxDrinks = 10;
                break;
            default:
                minDrinks = 1;
                maxDrinks = 5;
                break;
        }

        Debug.Log($"📌 Schwierigkeitsstufe: {selectedDifficulty} | Min: {minDrinks}, Max: {maxDrinks}");
    }

    public void ShowNextTask()
    {
        usedPlayers.Clear();

        // Falls die Runde beendet wurde und das letzte Event bereits gezeigt wurde, Menü öffnen
        if (gameEnded && lastDrinkEventShown)
        {
            EndRound();
            return;
        }

        // Falls die maximale Anzahl an Aufgaben erreicht ist, Letzter-Schluck-Event auslösen
        if (tasksCompleted >= maxTasks && !lastDrinkEventShown)
        {
            ShowLastDrinkEvent();
            lastDrinkEventShown = true;
            return;
        }

        // Falls die letzte Aufgabe bereits gezeigt wurde, jetzt auf den finalen Klick warten
        if (lastDrinkEventShown)
        {
            gameEnded = true;
            return;
        }

        title.gameObject.SetActive(true);
        title.text = "Aufgabe";
        mainCamera.backgroundColor = defaultColor;

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

        // Wähle eine einzigartige Aufgabe aus der Liste
        title.text = "Aufgabe";
        string selectedTask = GetNextUniqueTask(normalTasks);
        ReplacePlaceholders(ref selectedTask);
        taskText.text = selectedTask;
        tasksCompleted++;
    }



    private string GetNextUniqueTask(List<string> taskPool)
    {
        if (taskPool.Count == 0)
            return "Keine Aufgaben verfügbar!";

        if (usedTasks.Count >= taskPool.Count)
            usedTasks.Clear(); // Falls alle Aufgaben schon dran waren, Liste zurücksetzen

        string selectedTask;
        do
        {
            selectedTask = taskPool[Random.Range(0, taskPool.Count)];
        } while (usedTasks.Contains(selectedTask)); // Stelle sicher, dass Aufgabe noch nicht kam

        usedTasks.Add(selectedTask);
        return selectedTask;
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
        string selectedTask = "";

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

                if (taskPool.Count > 0)
                {
                    selectedTask = taskPool[Random.Range(0, taskPool.Count)];

                    if (selectedTask.Contains("{Spieler1}"))
                    {
                        currentRulePlayer = GetUniqueNonDriver();
                        selectedTask = selectedTask.Replace("{Spieler1}", currentRulePlayer);
                    }
                    else
                    {
                        currentRulePlayer = "";
                    }

                    ReplacePlaceholders(ref selectedTask); // Hier wird {Schlucke} korrekt ersetzt
                }
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

        if (string.IsNullOrEmpty(selectedTask))
        {
            selectedTask = taskPool[Random.Range(0, taskPool.Count)];
            ReplacePlaceholders(ref selectedTask);
        }

        taskText.text = selectedTask;
        tasksCompleted++;
    }


    private void ShowRuleEnd()
    {
        title.gameObject.SetActive(true);
        title.text = "REGEL ENDE";
        mainCamera.backgroundColor = Color.yellow;

        if (!string.IsNullOrEmpty(currentRulePlayer))
        {
            taskText.text = $"{currentRulePlayer}, deine Regel ist vorbei!";
        }
        else
        {
            taskText.text = "Eure Regel ist zu Ende!";
        }

        ruleActive = false;
        currentRulePlayer = "";
    }



    private void ReplacePlaceholders(ref string taskDescription)
    {
        int normalPlayers = nonDriverPlayers.Count;
        int driverCount = driverNames.Count;

        if ((taskDescription.Contains("{Spieler3}") && normalPlayers < 3) ||
            (taskDescription.Contains("{Spieler4}") && normalPlayers < 4))
        {
            ShowNextTask();
            return;
        }

        if (taskDescription.Contains("{Fahrer1}") && driverCount == 0)
        {
            ShowNextTask();
            return;
        }

        if (taskDescription.Contains("{Spieler1}"))
        {
            string name = GetColoredName(GetUniqueNonDriver());
            taskDescription = taskDescription.Replace("{Spieler1}", name);
        }

        if (taskDescription.Contains("{Spieler2}"))
        {
            string name = GetColoredName(GetUniqueNonDriver());
            taskDescription = taskDescription.Replace("{Spieler2}", name);
        }

        if (taskDescription.Contains("{Spieler3}") && normalPlayers >= 3)
        {
            string name = GetColoredName(GetUniqueNonDriver());
            taskDescription = taskDescription.Replace("{Spieler3}", name);
        }

        if (taskDescription.Contains("{Spieler4}") && normalPlayers >= 4)
        {
            string name = GetColoredName(GetUniqueNonDriver());
            taskDescription = taskDescription.Replace("{Spieler4}", name);
        }

        if (taskDescription.Contains("{Fahrer1}") && driverCount > 0)
        {
            string name = GetColoredName(GetUniqueDriver());
            taskDescription = taskDescription.Replace("{Fahrer1}", name);
        }

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
    private void LoadLastDrinkTasks()
    {
        lastDrinkTasks = LoadTasksFromTextAsset("Tasks/letzterschluck");
      
    }
    private void ShowLastDrinkEvent()
    {
        if (lastDrinkTasks.Count == 0)
        {
            taskText.text = "🎉 Die Runde ist vorbei!";
            return;
        }

        string selectedTask = lastDrinkTasks[Random.Range(0, lastDrinkTasks.Count)];
        ReplacePlaceholders(ref selectedTask);

        // Setze den Titel auf SPIELENDE und ändere die Hintergrundfarbe
        title.gameObject.SetActive(true);
        title.text = "SPIELENDE";
        mainCamera.backgroundColor = Color.black; // Farbe anpassen (z. B. Schwarz für das Finale)

        taskText.text = selectedTask;
    }
    private Color GetRandomContrastingColor(Color backgroundColor)
    {
        List<Color> candidateColors = new List<Color>
    {
        Color.red,
        Color.green,
        new Color(1f, 0.65f, 0f), // Orange
        Color.yellow,
        Color.blue
    };

        candidateColors = candidateColors.OrderBy(c => Random.value).ToList();

        foreach (Color candidate in candidateColors)
        {
            if (HasSufficientContrast(candidate, backgroundColor))
                return candidate;
        }
        return candidateColors[0];
    }

    private bool HasSufficientContrast(Color textColor, Color backgroundColor)
    {
        float textLuminance = 0.299f * textColor.r + 0.587f * textColor.g + 0.114f * textColor.b;
        float bgLuminance = 0.299f * backgroundColor.r + 0.587f * backgroundColor.g + 0.114f * backgroundColor.b;
        return Mathf.Abs(textLuminance - bgLuminance) >= 0.5f;
    }

    private string GetColoredName(string name)
    {
        Color bg = mainCamera.backgroundColor;
        Color chosenColor = GetRandomContrastingColor(bg);
        string hexColor = ColorUtility.ToHtmlStringRGB(chosenColor);
        return $"<color=#{hexColor}>{name}</color>";
    }


}
