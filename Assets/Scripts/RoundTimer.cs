using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RoundTimer : MonoBehaviour
{
    public TextMeshProUGUI roundsPlayedText;

    private void Start()
    {
        DisplayRoundsPlayed();
    }

    private void DisplayRoundsPlayed()
    {
        int roundsPlayed = PlayerPrefs.GetInt("RoundsPlayed", 0);
        roundsPlayedText.text = "Runden gespielt: " + roundsPlayed;
    }
}
