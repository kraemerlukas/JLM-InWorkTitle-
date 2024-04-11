using TMPro;
using UnityEngine;

public class PlayerInfo : MonoBehaviour
{
    public string playerName;

    public void Initialize(string name)
    {
        playerName = name;
        // F�hre hier die Initialisierung f�r den Spieler durch
        // Zum Beispiel: Setze den Namen des Spielers auf das entsprechende UI-Element
        GetComponentInChildren<TextMeshProUGUI>().text = playerName;
    }

    // Weitere Methoden und Eigenschaften f�r die Spielerinformationen
}