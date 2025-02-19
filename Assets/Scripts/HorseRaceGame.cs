using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HorseRaceGame : MonoBehaviour
{
    public GameObject[] cardPrefabs; // Liste aller Karten-Prefabs im Ordner
    public Transform cardSpawnPoint; // Wo die gezogene Karte angezeigt wird
    public Transform deckPosition; // Position des Kartenstapels
    public Transform discardPilePosition; // Ablagestapel-Position
    public Image deckDisplay; // UI-Element für den Kartenstapel (Rückseite)
    public Image drawnCardDisplay; // UI-Element zur Anzeige der gezogenen Karte
    public GameObject[] horses; // Die Ass-Karten, die sich bewegen
    public Transform[] heartTrack;
    public Transform[] diamondTrack;
    public Transform[] spadeTrack;
    public Transform[] clubTrack;
    public TMP_Text winnerText; // TextMeshPro statt Standard-Text
    public Button drawCardButton; // Button zum Ziehen einer Karte
    public float moveSpeed = 2.0f; // Geschwindigkeit für den smoothen Übergang

    private GameObject lastDrawnCard; // Speichert die zuletzt gezogene Karte
    private List<GameObject> discardPile = new List<GameObject>(); // Liste für abgelegte Karten
    private Dictionary<string, int> horsePositions = new Dictionary<string, int>()
    {
        {"Heart", 0},
        {"Diamond", 0},
        {"Spade", 0},
        {"Club", 0}
    };

    void Start()
    {
        Screen.orientation = ScreenOrientation.LandscapeLeft; // Setzt die Bildschirm-Ausrichtung
        winnerText.text = "";
        drawCardButton.onClick.AddListener(DrawCard); // Button-Klick registrieren
    }

    public void DrawCard()
    {
        if (CheckWin()) return;

        // Letzte gezogene Karte zum Ablagestapel verschieben
        if (lastDrawnCard != null)
        {
            lastDrawnCard.transform.SetParent(discardPilePosition, false);
            lastDrawnCard.transform.localPosition = new Vector3(discardPile.Count * 10, 0, 0); // Stapelaufbau
            discardPile.Add(lastDrawnCard);
        }

        // Zufällige Karte aus dem Prefab-Ordner auswählen
        int randomIndex = Random.Range(0, cardPrefabs.Length);
        GameObject newCard = Instantiate(cardPrefabs[randomIndex], deckPosition);
        newCard.transform.SetParent(cardSpawnPoint, false); // Stellt sicher, dass die Karte korrekt spawnt
        newCard.transform.localPosition = Vector3.zero; // Setzt die Karte an den exakten Spawnpunkt
        lastDrawnCard = newCard; // Speichert die neue Karte als letzte gezogene Karte

        // Kartenbild in UI anzeigen
        Image cardImage = newCard.GetComponent<Image>();
        if (cardImage != null && drawnCardDisplay != null)
        {
            drawnCardDisplay.sprite = cardImage.sprite;
        }

        // Kartenwert überprüfen
        Card card = newCard.GetComponent<Card>();
        if (card != null)
        {
            MoveHorse(card.suit);
        }
    }

    void MoveHorse(string suit)
    {
        if (horsePositions.ContainsKey(suit) && horsePositions[suit] < 7)
        {
            int horseIndex = GetHorseIndex(suit);
            horsePositions[suit]++;

            Transform[] track = GetTrack(suit);
            if (track != null && horsePositions[suit] < track.Length)
            {
                StartCoroutine(SmoothMove(horses[horseIndex], track[horsePositions[suit]].position));
            }
        }

        if (horsePositions[suit] == 7)
        {
            winnerText.text = "Gewinner: " + suit;
        }
    }

    IEnumerator SmoothMove(GameObject horse, Vector3 targetPosition)
    {
        float elapsedTime = 0;
        Vector3 startingPosition = horse.transform.position;
        while (elapsedTime < 1f)
        {
            horse.transform.position = Vector3.Lerp(startingPosition, targetPosition, elapsedTime / moveSpeed);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        horse.transform.position = targetPosition;
    }

    bool CheckWin()
    {
        foreach (var pos in horsePositions.Values)
        {
            if (pos >= 7) return true;
        }
        return false;
    }

    int GetHorseIndex(string suit)
    {
        switch (suit)
        {
            case "Heart": return 0;
            case "Diamond": return 1;
            case "Spade": return 2;
            case "Club": return 3;
            default: return -1;
        }
    }

    Transform[] GetTrack(string suit)
    {
        switch (suit)
        {
            case "Heart": return heartTrack;
            case "Diamond": return diamondTrack;
            case "Spade": return spadeTrack;
            case "Club": return clubTrack;
            default: return null;
        }
    }
}
