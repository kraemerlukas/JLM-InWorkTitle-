using UnityEngine;
using System.Collections.Generic;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    private List<string> playerNames = new List<string>();
    private List<string> driverNames = new List<string>();

    private void Start()
    {
        LoadPlayerData();
        CountPlayersAndDrivers();
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

            bool isDriver = PlayerPrefs.GetInt("IsDriver" + (i + 1), 0) == 1;
            if (isDriver)
            {
                driverNames.Add(playerName);
            }
        }
    }

    private void CountPlayersAndDrivers()
    {
        Debug.Log("Total players: " + playerNames.Count);
        Debug.Log("Total drivers: " + driverNames.Count);
    }
}