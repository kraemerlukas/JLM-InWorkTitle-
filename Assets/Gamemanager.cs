using UnityEngine;
using System.Collections.Generic;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    public List<string> playerNames;
    private TaskManager taskManager;

    private void Start()
    {
        Debug.Log("Start loading player data...");
        LoadPlayerData();
        Debug.Log("Player data loaded.");

        Debug.Log("Generating tasks...");
       
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
}