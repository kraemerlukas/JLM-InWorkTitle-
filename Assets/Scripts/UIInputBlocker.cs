using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UIInputBlocker : MonoBehaviour
{
    public static UIInputBlocker Instance;

    // Weist im Inspector den GraphicRaycaster deines Canvas zu.
    public GraphicRaycaster graphicRaycaster;
    // Weist im Inspector das EventSystem der Szene zu.
    public EventSystem eventSystem;

    void Awake()
    {
        // Singleton-Pattern
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    /// <summary>
    /// Gibt true zurück, wenn der aktuelle Pointer (Maus oder Touch) über einem UI-Element liegt,
    /// das in seiner Hierarchie eine Button-Komponente besitzt.
    /// </summary>
    public bool IsPointerOverUIButton()
    {
        // Erzeuge ein PointerEventData basierend auf der aktuellen Position
        PointerEventData pointerData = new PointerEventData(eventSystem)
        {
            position = Input.mousePosition
        };

        List<RaycastResult> results = new List<RaycastResult>();
        graphicRaycaster.Raycast(pointerData, results);

        // Prüfe, ob in den Ergebnissen ein Button (auch in einem Parent) gefunden wurde
        foreach (RaycastResult result in results)
        {
            if (result.gameObject.GetComponentInParent<Button>() != null)
                return true;
        }
        return false;
    }
}
