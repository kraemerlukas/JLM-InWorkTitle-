using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DifficultyButtonManager : MonoBehaviour
{
    public TextMeshProUGUI buttonText;

    private string[] coolDifficultyNames = { "Pussy", "Standart", "Vollsuff" }; // Vordefinierte coole Namen für die Schwierigkeitsgrade
    private string[] difficultyLevels = { "Easy", "Medium", "Hard" }; // Schwierigkeitsgrade für PlayerPrefs
    private int currentDifficultyIndex = 0;

    private void Start()
    {
        // Lade den aktuellen Schwierigkeitsgrad aus den PlayerPrefs
        string currentDifficulty = PlayerPrefs.GetString("SelectedDifficulty", "Easy"); // Standardmäßig "Easy"
        // Aktualisiere den Text des Buttons basierend auf dem aktuellen Schwierigkeitsgrad
        for (int i = 0; i < difficultyLevels.Length; i++)
        {
            if (difficultyLevels[i] == currentDifficulty)
            {
                buttonText.text = coolDifficultyNames[i];
                currentDifficultyIndex = i;
                break;
            }
        }
    }

    public void ChangeDifficulty()
    {
        // Inkrementiere den aktuellen Schwierigkeitsgrad-Index
        currentDifficultyIndex = (currentDifficultyIndex + 1) % difficultyLevels.Length;
        // Aktualisiere den Schwierigkeitsgrad im Text des Buttons
        buttonText.text = coolDifficultyNames[currentDifficultyIndex];
        // Aktualisiere die PlayerPrefs mit dem neuen Schwierigkeitsgrad
        PlayerPrefs.SetString("SelectedDifficulty", difficultyLevels[currentDifficultyIndex]);
        PlayerPrefs.Save();
    }
}