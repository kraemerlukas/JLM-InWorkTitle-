using UnityEngine;

public class TeamDebug : MonoBehaviour
{
    private void Start()
    {
        string team1Name = PlayerPrefs.GetString("Team1Name", "Team 1");
        string team2Name = PlayerPrefs.GetString("Team2Name", "Team 2");

        Debug.Log("Team 1 (" + team1Name + "): ");
        int team1Count = PlayerPrefs.GetInt("Team1Count", 0);
        for (int i = 0; i < team1Count; i++)
        {
            string playerName = PlayerPrefs.GetString("Team1Player" + i);
            Debug.Log("- " + playerName);
        }

        Debug.Log("Team 2 (" + team2Name + "): ");
        int team2Count = PlayerPrefs.GetInt("Team2Count", 0);
        for (int i = 0; i < team2Count; i++)
        {
            string playerName = PlayerPrefs.GetString("Team2Player" + i);
            Debug.Log("- " + playerName);
        }
    }
}