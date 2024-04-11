using TMPro;
using UnityEngine;

public class PlayerInfo : MonoBehaviour
{
    public string playerName;

    public void Initialize(string name)
    {
        playerName = name;
        // Führe hier die Initialisierung für den Spieler durch
        // Zum Beispiel: Setze den Namen des Spielers auf das entsprechende UI-Element
        GetComponentInChildren<TextMeshProUGUI>().text = playerName;
    }

    // Weitere Methoden und Eigenschaften für die Spielerinformationen
}