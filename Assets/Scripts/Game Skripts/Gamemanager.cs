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

        Debug.Log("Direkt nach dem Laden:");
        for (int i = 1; i <= PlayerPrefs.GetInt("PlayerCount", 0); i++)
        {
            string playerName = PlayerPrefs.GetString("Player" + i, "UNKNOWN");
            int isDriver = PlayerPrefs.GetInt(playerName + "_IsDriver", -1);
            Debug.Log($"Spieler {playerName}, Fahrer: {isDriver}");
        }

        RemoveDriversFromPlayerCount();
    }


    private void LoadPlayerData()
    {
        playerNames.Clear();
        driverNames.Clear();
        HashSet<string> addedPlayers = new HashSet<string>(); // Verhindert doppelte Spieler
        HashSet<string> addedDrivers = new HashSet<string>(); // Verhindert doppelte Fahrer

        int playerCount = PlayerPrefs.GetInt("PlayerCount", 0);
        for (int i = 1; i <= playerCount; i++)
        {
            string playerName = PlayerPrefs.GetString("Player" + i, "").Trim();
            int isDriver = PlayerPrefs.GetInt(playerName + "_IsDriver", 0);

            if (!string.IsNullOrEmpty(playerName))
            {
                // Falls der Name schon existiert, hänge eine Nummer an
                int count = 1;
                string newPlayerName = playerName;
                while (addedPlayers.Contains(newPlayerName))
                {
                    count++;
                    newPlayerName = playerName + "(" + count + ")";
                }

                addedPlayers.Add(newPlayerName);
                playerNames.Add(newPlayerName);

                if (isDriver == 1 && !addedDrivers.Contains(newPlayerName))
                {
                    driverNames.Add(newPlayerName);
                    addedDrivers.Add(newPlayerName);
                }
            }
        }

        Debug.Log("✅ Geladene Spieler: " + string.Join(", ", playerNames));
        Debug.Log("✅ Geladene Fahrer: " + string.Join(", ", driverNames));
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
        HashSet<string> uniquePlayers = new HashSet<string>(playerNames); // Speichert Spieler ohne Duplikate
        HashSet<string> uniqueDrivers = new HashSet<string>(driverNames); // Speichert Fahrer ohne Duplikate

        // Speichere alle Spieler erneut mit Fahrerstatus
        PlayerPrefs.SetInt("PlayerCount", uniquePlayers.Count + uniqueDrivers.Count);

        int index = 1;
        foreach (string player in uniquePlayers)
        {
            PlayerPrefs.SetString("Player" + index, player);
            int isDriver = PlayerPrefs.GetInt(player + "_IsDriver", 0);
            PlayerPrefs.SetInt(player + "_IsDriver", isDriver);
            index++;
        }

        // Fahrer separat speichern, aber doppelte vermeiden
        foreach (string driver in uniqueDrivers)
        {
            if (!uniquePlayers.Contains(driver))  // Nur speichern, wenn nicht bereits als Spieler vorhanden
            {
                PlayerPrefs.SetString("Player" + index, driver);
                PlayerPrefs.SetInt(driver + "_IsDriver", 1);
                index++;
            }
        }

        PlayerPrefs.Save();
    }




}