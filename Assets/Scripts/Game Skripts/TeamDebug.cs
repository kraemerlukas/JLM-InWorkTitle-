using UnityEngine;

public class TeamDebug : MonoBehaviour
{
    private void Start()
    {
        // Teams aus den PlayerPrefs abrufen
        string team1Name = PlayerPrefs.GetString("Team1Name", "Team 1");
        string team2Name = PlayerPrefs.GetString("Team2Name", "Team 2");

        // Spieler aus den PlayerPrefs für jedes Team abrufen und in der Konsole ausgeben
        Debug.Log("Team 1 (" + team1Name + "): " + PlayerPrefs.GetString("Team1Players", "No players"));
        Debug.Log("Team 2 (" + team2Name + "): " + PlayerPrefs.GetString("Team2Players", "No players"));
    }
}