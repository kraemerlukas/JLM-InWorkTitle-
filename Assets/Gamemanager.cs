using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    private List<string> playerNames = new List<string>();
    private List<string> driverNames = new List<string>();

    private void Start()
    {
        LoadPlayerData();
        CountPlayersAndDrivers();
        RemoveDriversFromPlayerCount();
    }

    private void LoadPlayerData()
    {
        int playerCount = PlayerPrefs.GetInt("PlayerCount", 0);
        Debug.Log("Player count loaded: " + playerCount);
        for (int i = 0; i < playerCount; i++)
        {
            string playerName = PlayerPrefs.GetString("Player" + (i + 1));
            Debug.Log("Player " + (i + 1) + " loaded: " + playerName);

            bool isDriver = PlayerPrefs.GetInt(playerName + "_IsDriver", 0) == 1; // Überprüfe den Fahrerstatus des Spielers
            if (!isDriver) // Fahrer ausfiltern
            {
                playerNames.Add(playerName);
            }
            else
            {
                driverNames.Add(playerName);
            }
        }
    }

    private void CountPlayersAndDrivers()
    {
        Debug.Log("Total players: " + playerNames.Count);
        Debug.Log("Total drivers: " + driverNames.Count);

        Debug.Log("Driver names:");
        foreach (string driverName in driverNames)
        {
            Debug.Log(driverName);
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
}