using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class TaskManager : MonoBehaviour
{
    public List<GameObject> taskPrefabs;
    public TextMeshProUGUI taskText;

    private List<string> playerNames = new List<string>();

    private void Start()
    {
        LoadPlayerData();
        ShowNextTask();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            ShowNextTask();
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

        int randomPlayerIndex = Random.Range(0, playerCount);
        string randomPlayerName = playerNames[randomPlayerIndex];

        int randomTaskIndex = Random.Range(0, taskPrefabs.Count);
        GameObject randomTaskPrefab = taskPrefabs[randomTaskIndex];

        GameObject taskInstance = Instantiate(randomTaskPrefab, transform.position, Quaternion.identity);
        string taskDescription = taskInstance.GetComponentInChildren<TextMeshProUGUI>().text;
        taskDescription = taskDescription.Replace("{Spieler}", randomPlayerName);

        taskText.text = taskDescription;
    }
}