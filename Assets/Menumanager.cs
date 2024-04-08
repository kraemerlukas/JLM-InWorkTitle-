using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class MainMenu : MonoBehaviour
{
    public TMP_InputField playerNameInputField;
    public Transform playerListContainer;
    public GameObject playerListItemPrefab;

    private List<string> playerNames = new List<string>();

    private void Start()
    {
        PlayerPrefs.DeleteAll();
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
            SavePlayerNames();
            UpdatePlayerList();
            playerNameInputField.text = ""; // Clear input field after adding player
        }
    }

    public void RemovePlayer(string playerName)
    {
        playerNames.Remove(playerName);
        SavePlayerNames();
        UpdatePlayerList();

        // Aktualisiere den Spielerindex in den PlayerPrefs
        PlayerPrefs.SetInt("PlayerCount", playerNames.Count);
        for (int i = 0; i < playerNames.Count; i++)
        {
            PlayerPrefs.SetString("Player" + (i + 1), playerNames[i]);
        }
        PlayerPrefs.Save();
    }

    private void UpdatePlayerList()
    {
        // Clear existing player list items
        foreach (Transform child in playerListContainer)
        {
            Destroy(child.gameObject);
        }

        // Create new player list items
        foreach (string playerName in playerNames)
        {
            GameObject playerListItem = Instantiate(playerListItemPrefab, playerListContainer);
            playerListItem.GetComponentInChildren<TextMeshProUGUI>().text = playerName;
            Button removeButton = playerListItem.GetComponentInChildren<Button>();
            removeButton.onClick.AddListener(() => RemovePlayer(playerName));
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