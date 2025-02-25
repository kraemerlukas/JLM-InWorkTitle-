using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class OrientationTransition : MonoBehaviour
{
    public GameObject transitionPanel;
    public string targetOrientationString = "LandscapeLeft";
    public Camera mainCamera; // Im Inspector zuweisen

    private void Awake()
    {
        StartTransition();
    }
    public void StartTransition()
    {
        StartCoroutine(DoOrientationTransition());
    }

    private IEnumerator DoOrientationTransition()
    {
        // Versuche, das Image-Component des Panels zu bekommen
        Image panelImage = transitionPanel.GetComponent<Image>();
        if (panelImage != null && mainCamera != null)
        {
            // Setzt die Farbe des Panels auf die Hintergrundfarbe der MainCamera
            panelImage.color = mainCamera.backgroundColor;
        }

        // Panel aktivieren, damit der Übergang sofort sichtbar ist
        transitionPanel.SetActive(true);

        // Versuche, den String in den entsprechenden ScreenOrientation-Enum zu parsen
        if (!Enum.TryParse(targetOrientationString, out ScreenOrientation targetOrientation))
        {
            Debug.LogError("Ungültiger Orientierungswert: " + targetOrientationString);
            yield break;
        }

        // Setzt die gewünschte Ausrichtung
        Screen.orientation = targetOrientation;

        // Warte einen Frame und eine kurze Zeit, damit die Änderung übernommen wird
        yield return null;
        yield return new WaitForSeconds(0.1f);

        // Übergang abgeschlossen – Panel deaktivieren
        transitionPanel.SetActive(false);
    }
}
