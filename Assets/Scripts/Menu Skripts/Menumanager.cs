using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

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
        string playerName = playerNameInputField.text;
        if (!string.IsNullOrEmpty(playerName))
        {
            // Füge den Spieler zur PlayerPrefs hinzu
            int playerCount = PlayerPrefs.GetInt("PlayerCount", 0);
            playerCount++;
            PlayerPrefs.SetInt("PlayerCount", playerCount);
            PlayerPrefs.SetString("Player" + playerCount, playerName);
            playerNames.Add(playerName);

            // Speichere den Fahrerstatus basierend auf dem Toggle
            int isDriver = driverToggle.isOn ? 1 : 0;
            PlayerPrefs.SetInt(playerName + "_IsDriver", isDriver);

            SavePlayerNames();
            UpdatePlayerList();
            playerNameInputField.text = ""; // Clear input field after adding player
        }
    }

    public void RemovePlayer(string playerName)
    {
        // Entferne den Spieler aus der Liste der Spieler
        playerNames.Remove(playerName);

        // Entferne den Spieler aus den PlayerPrefs
        PlayerPrefs.SetInt("PlayerCount", playerNames.Count);
        for (int i = 0; i < playerNames.Count; i++)
        {
            PlayerPrefs.SetString("Player" + (i + 1), playerNames[i]);
        }
        PlayerPrefs.DeleteKey(playerName + "_IsDriver"); // Lösche den Eintrag für den Spieler als Fahrer
        PlayerPrefs.Save();

        UpdatePlayerList();
    }

    private void UpdatePlayerList()
    {
        // Clear existing player list items
        foreach (Transform child in playerListContainer1)
        {
            Destroy(child.gameObject);
        }
        foreach (Transform child in playerListContainer2)
        {
            Destroy(child.gameObject);
        }

        // Create new player list items, alternating between containers
        for (int i = 0; i < playerNames.Count; i++)
        {
            GameObject playerListItem = Instantiate(playerListItemPrefab, addToFirstContainer ? playerListContainer1 : playerListContainer2);
            TextMeshProUGUI playerNameText = playerListItem.GetComponentInChildren<TextMeshProUGUI>();
            playerNameText.text = playerNames[i];

            // Set text color based on driver status
            int isDriver = PlayerPrefs.GetInt(playerNames[i] + "_IsDriver", 0);
            if (isDriver == 1)
            {
                playerNameText.color = Color.blue; // Fahrer in Blau anzeigen
            }
            else
            {
                playerNameText.color = Color.green; // Normale Spieler in Grün anzeigen
            }

            // Set up remove button callback
            Button removeButton = playerListItem.GetComponentInChildren<Button>();
            int index = i; // Speichere den aktuellen Index in einer lokalen Variable
            removeButton.onClick.AddListener(() => RemovePlayer(playerNames[index]));

            addToFirstContainer = !addToFirstContainer; // Wechseln zwischen den Containern
        }
    }

    private void LoadPlayerNames()
    {
        if (PlayerPrefs.HasKey("PlayerNames"))
        {
            string[] savedPlayerNames = PlayerPrefs.GetString("PlayerNames").Split(';');
            playerNames.AddRange(savedPlayerNames);
        }
    }

    private void SavePlayerNames()
    {
        string playerNamesString = string.Join(";", playerNames.ToArray());
        PlayerPrefs.SetString("PlayerNames", playerNamesString);
    }
}