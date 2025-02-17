using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using Unity.VisualScripting;

public class MainMenu : MonoBehaviour
{
    public InputField playerNameInputField;
    public Transform playerListContainer1;
    public Transform playerListContainer2;
    public GameObject playerListItemPrefab;
    public Toggle driverToggle; // Der Toggle für alle Spieler
    public Button teamModeButton;

    private List<string> playerNames = new List<string>();

    private bool addToFirstContainer = true; // Flag, um zu entscheiden, welcher Container verwendet werden soll

    private void Start()
    {
        Screen.orientation = ScreenOrientation.Portrait;
        LoadPlayerNames();
        UpdatePlayerList();
    }

    public void AddPlayer()
    {
        string playerName = playerNameInputField.text.Trim();
        if (!string.IsNullOrEmpty(playerName))
        {
            // Überprüfen, ob der Name bereits existiert
            int count = 1;
            string newPlayerName = playerName;
            while (playerNames.Contains(newPlayerName))
            {
                count++;
                newPlayerName = playerName + "(" + count + ")";
            }

            // Füge den Spieler mit eindeutigem Namen hinzu
            int playerCount = PlayerPrefs.GetInt("PlayerCount", 0) + 1;
            PlayerPrefs.SetInt("PlayerCount", playerCount);
            PlayerPrefs.SetString("Player" + playerCount, newPlayerName);
            playerNames.Add(newPlayerName);

            // Speichere den Fahrerstatus basierend auf dem Toggle
            int isDriver = driverToggle.isOn ? 1 : 0;
            PlayerPrefs.SetInt(newPlayerName + "_IsDriver", isDriver);

            SavePlayerNames();
            UpdatePlayerList();
            playerNameInputField.text = ""; // Eingabefeld leeren
        }
    }


    public void RemovePlayer(string playerName)
    {
        if (playerNames.Contains(playerName))
        {
            playerNames.Remove(playerName);

            // Lösche Spieler aus PlayerPrefs
            int playerCount = playerNames.Count;
            PlayerPrefs.SetInt("PlayerCount", playerCount);

            // Alle Einträge neu setzen, um die Lücke zu schließen
            for (int i = 0; i < playerNames.Count; i++)
            {
                PlayerPrefs.SetString("Player" + (i + 1), playerNames[i]);
            }

            // Lösche den alten letzten Eintrag aus PlayerPrefs (damit kein "Geist-Spieler" bleibt)
            PlayerPrefs.DeleteKey("Player" + (playerCount + 1));
            PlayerPrefs.DeleteKey(playerName + "_IsDriver");

            PlayerPrefs.Save();
            UpdatePlayerList();

            Debug.Log("Nach dem Löschen:");
            for (int i = 1; i <= PlayerPrefs.GetInt("PlayerCount", 0); i++)
            {
                Debug.Log("Player " + i + ": " + PlayerPrefs.GetString("Player" + i, "GELÖSCHT"));
            }
        }
    }

    private void UpdatePlayerList()
    {
        foreach (Transform child in playerListContainer1)
        {
            Destroy(child.gameObject);
        }
        foreach (Transform child in playerListContainer2)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < playerNames.Count; i++)
        {
            GameObject playerListItem = Instantiate(playerListItemPrefab, (i % 2 == 0) ? playerListContainer1 : playerListContainer2);
            TextMeshProUGUI playerNameText = playerListItem.GetComponentInChildren<TextMeshProUGUI>();

            string playerName = playerNames[i];
            int isDriver = PlayerPrefs.GetInt(playerName + "_IsDriver", 0); // Fahrerstatus abrufen

            playerNameText.text = playerName; // Setze Standardnamen

            // Falls IconController vorhanden ist, setze den Fahrerstatus
            IconController iconController = playerListItem.GetComponent<IconController>();
            if (iconController != null)
            {
                iconController.isDriver = isDriver == 1;
                iconController.Car.SetActive(isDriver == 1);  // Auto-Icon aktivieren, falls Fahrer
                iconController.Beer.SetActive(isDriver == 0); // Bier-Icon aktivieren, falls kein Fahrer
            }

            // Button-Funktion für Spieler-Entfernung setzen
            Button removeButton = playerListItem.GetComponentInChildren<Button>();
            removeButton.onClick.AddListener(() => RemovePlayer(playerName));
        }
    }


    private void LoadPlayerNames()
    {
        playerNames.Clear(); // Vorherige Liste leeren
        int playerCount = PlayerPrefs.GetInt("PlayerCount", 0);

        Debug.Log(" Lade Spieler in Menumanager: " + playerCount);

        for (int i = 1; i <= playerCount; i++)
        {
            string playerName = PlayerPrefs.GetString("Player" + i, "");
            int isDriver = PlayerPrefs.GetInt(playerName + "_IsDriver", -1); // Fahrerstatus abrufen

            if (!string.IsNullOrEmpty(playerName))
            {
                playerNames.Add(playerName);
                Debug.Log($" Spieler {playerName}, Fahrer: {isDriver}");
            }
        }

        UpdatePlayerList(); // Aktualisiere das UI nach dem Laden
    }


    private void SavePlayerNames()
    {
        string playerNamesString = string.Join(";", playerNames.ToArray());
        PlayerPrefs.SetString("PlayerNames", playerNamesString);
    }
}