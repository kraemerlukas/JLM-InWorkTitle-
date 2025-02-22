using UnityEngine;
using UnityEngine.UI;

public class HelpPanelController : MonoBehaviour
{
    // Dem Inspector zuweisen: Das UI-Panel mit der Erklärung des Spielmodus.
    public GameObject helpPanel;
    // Dem Inspector zuweisen: Der "?" Button.
    public Button helpButton;

    // Flag, ob das Spiel pausiert ist (Panel offen)
    private bool isPaused = false;

    void Start()
    {
        // Stelle sicher, dass das Panel initial ausgeblendet ist.
        helpPanel.SetActive(false);
        // Button-Event abonnieren
        helpButton.onClick.AddListener(ToggleHelpPanel);
    }

    void Update()
    {
        // Wenn das Panel offen ist, reagiert ein Klick (egal wo) auf das Schließen.
        if (isPaused && Input.GetMouseButtonDown(0))
        {
            // Schließe das Panel nur, wenn der Klick nicht direkt auf den "?" Button erfolgt
            // (damit der Button nicht sofort wieder öffnet)
            if (!RectTransformUtility.RectangleContainsScreenPoint(
                    helpButton.GetComponent<RectTransform>(),
                    Input.mousePosition, Camera.main))
            {
                ToggleHelpPanel();
            }
        }
    }

    // Wechselt zwischen Pause und Spiel
    public void ToggleHelpPanel()
    {
        isPaused = !isPaused;
        helpPanel.SetActive(isPaused);

        if (isPaused)
        {
            // Spiel pausieren
            Time.timeScale = 0f;
        }
        else
        {
            // Spiel fortsetzen
            Time.timeScale = 1f;
        }
    }
}

