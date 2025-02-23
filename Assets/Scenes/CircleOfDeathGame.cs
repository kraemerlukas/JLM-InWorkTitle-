using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections.Generic;

public class CircleOfDeathGame : MonoBehaviour
{
    // UI-Elemente
    public GameObject instructionPanel;
    public TMP_Text instructionText;

    // Karteneinstellungen
    public GameObject[] cardPrefabs;
    public RectTransform[] circleCardPositions; // UI-Positionen (RectTransform)
    private GameObject[] circleCards;
    private bool[] cardFlipped;

    // Referenz zum GraphicRaycaster des Canvas
    public GraphicRaycaster uiRaycaster;

    void Start()
    {
                Screen.orientation = ScreenOrientation.LandscapeLeft;

        // Überprüfe, ob der GraphicRaycaster gesetzt ist
        if (uiRaycaster == null)
        {
            Debug.LogError("GraphicRaycaster ist nicht zugewiesen!");
        }

        cardFlipped = new bool[circleCardPositions.Length];
        circleCards = new GameObject[circleCardPositions.Length];
        SpawnCircleCards();

        if (instructionPanel != null && instructionText != null)
        {
            instructionPanel.SetActive(true);
            instructionText.text = "Tippe eine Karte, um sie umzudrehen...";
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began))
        {
            Vector2 pointerPosition = Input.mousePosition;
            if (Input.touchCount > 0)
            {
                pointerPosition = Input.GetTouch(0).position;
            }

            PointerEventData pointerData = new PointerEventData(EventSystem.current);
            pointerData.position = pointerPosition;

            List<RaycastResult> results = new List<RaycastResult>();
            uiRaycaster.Raycast(pointerData, results);

            Debug.Log("Anzahl der getroffenen UI-Elemente: " + results.Count);

            foreach (RaycastResult result in results)
            {
                Debug.Log("Getroffen: " + result.gameObject.name);
                // Hole das Card-Skript vom getroffenen Objekt oder dessen Eltern
                Card card = result.gameObject.GetComponentInParent<Card>();
                if (card != null)
                {
                    // Prüfe, ob diese Karte bereits in unserem Array vorhanden ist und noch nicht umgedreht wurde
                    for (int i = 0; i < circleCards.Length; i++)
                    {
                        // Vergleiche den Parent, der das Card-Skript enthält, mit unserem gespeicherten Card-Objekt
                        if (circleCards[i] == card.gameObject && !cardFlipped[i])
                        {
                            cardFlipped[i] = true;
                            card.FlipToFront();
                            break;
                        }
                    }
                }
            }
        }
    }

 
   void SpawnCircleCards()
    {
        // Instanziiere an jeder vordefinierten Position eine zufällige Karte
        for (int i = 0; i < circleCardPositions.Length; i++)
        {
            int randomIndex = Random.Range(0, cardPrefabs.Length);
            // Instanziierung als Child des jeweiligen UI-Feldes
            GameObject cardInstance = Instantiate(cardPrefabs[randomIndex], circleCardPositions[i].position, Quaternion.identity, circleCardPositions[i]);
            cardInstance.transform.localPosition = Vector3.zero;

            // Passe die Größe der Karte so an, dass sie exakt das Elternfeld ausfüllt
            RectTransform cardRect = cardInstance.GetComponent<RectTransform>();
            RectTransform parentRect = circleCardPositions[i];
            if (cardRect != null && parentRect != null)
            {
                // Setze den Pivot in die Mitte
                cardRect.anchorMin = new Vector2(0.5f, 0.5f);
                cardRect.anchorMax = new Vector2(0.5f, 0.5f);
                cardRect.anchoredPosition = Vector2.zero;
                // Übernehme die exakte Größe des Elternfeldes
                cardRect.sizeDelta = parentRect.rect.size;
                cardRect.localScale = Vector3.one;
            }

            // Stelle sicher, dass die Karte initial die Rückseite zeigt
            Card card = cardInstance.GetComponent<Card>();
            if (card != null)
            {
                card.frontImage.gameObject.SetActive(false);
                card.backImage.gameObject.SetActive(true);
            }
            circleCards[i] = cardInstance;
        }
    }


}
