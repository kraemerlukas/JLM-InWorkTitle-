using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HorseRaceGame : MonoBehaviour
{
    public GameObject[] horses; // Die Ass-Karten, die sich bewegen
    public Transform[] heartTrack;
    public Transform[] diamondTrack;
    public Transform[] spadeTrack;
    public Transform[] clubTrack;
    public TMP_Text winnerText; // TextMeshPro statt Standard-Text
    public Button drawCardButton; // Button zum Ziehen einer Karte
    public float moveSpeed = 2.0f; // Geschwindigkeit für den smoothen Übergang

    public GameObject[] cardPrefabs; // Liste aller Karten-Prefabs
    public Transform cardSpawnPoint; // Position, an der die Karte erscheinen soll
    private GameObject lastDrawnCard;

    public Transform[] fieldCardPositions; // Positionen für die Karten über den Feldern
    private GameObject[] fieldCards; // Karten, die über den Feldern liegen
    private bool[] fieldCardFlipped; // Merkt sich, ob eine Feldkarte schon einmal umgedreht wurde
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
        fieldCardFlipped = new bool[fieldCardPositions.Length]; // Initialisiert das Tracking für Kartenflip
        fieldCards = new GameObject[fieldCardPositions.Length];
        SpawnFieldCards();
    }

    void SpawnFieldCards()
    {
        for (int i = 0; i < fieldCardPositions.Length; i++)
        {
            fieldCards[i] = Instantiate(cardPrefabs[Random.Range(0, cardPrefabs.Length)], fieldCardPositions[i].position, Quaternion.identity);
            fieldCards[i].transform.SetParent(fieldCardPositions[i], false);
            fieldCards[i].transform.localPosition = Vector3.zero; // Exakte Position
        }
    }

    public void DrawCard()
    {
        if (CheckWin()) return;

        // Letzte gezogene Karte löschen
        if (lastDrawnCard != null)
        {
            Destroy(lastDrawnCard);
        }

        // Zufällige Karte aus dem Prefab-Ordner auswählen und an fester Position spawnen
        int randomIndex = Random.Range(0, cardPrefabs.Length);
        lastDrawnCard = Instantiate(cardPrefabs[randomIndex], cardSpawnPoint);
        lastDrawnCard.transform.SetParent(cardSpawnPoint, false);
        lastDrawnCard.transform.localPosition = Vector3.zero;

        // Kartenwert überprüfen
        Card card = lastDrawnCard.GetComponent<Card>();
        if (card != null)
        {
            MoveHorse(card.suit);
            CheckFieldCards();
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

    void CheckFieldCards()
    {
        for (int i = 0; i < fieldCards.Length; i++)
        {
            if (fieldCardFlipped[i]) continue; // Falls diese Karte bereits einmal geflippt wurde, nichts tun

            bool allHorsesHere = true;
            foreach (var position in horsePositions.Values)
            {
                if (position <= i)
                {
                    allHorsesHere = false;
                    break;
                }
            }

            if (allHorsesHere)
            {
                fieldCardFlipped[i] = true; // Karte als bereits geflippt markieren
                StartCoroutine(FlipFieldCard(i));
            }
        }
    }

    IEnumerator FlipFieldCard(int index)
    {
        float elapsedTime = 0;
        Quaternion startRotation = fieldCards[index].transform.rotation;
        Quaternion endRotation = Quaternion.Euler(0, 180, 0);

        while (elapsedTime < 0.5f)
        {
            fieldCards[index].transform.rotation = Quaternion.Lerp(startRotation, endRotation, elapsedTime / 0.5f);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        fieldCards[index].transform.rotation = endRotation;

        // Bestrafung: Pferd zurückziehen, aber nur beim ersten Mal
        Card fieldCardComponent = fieldCards[index].GetComponent<Card>();
        if (fieldCardComponent != null)
        {
            MoveHorseBack(fieldCardComponent.suit);
        }
    }

    void MoveHorseBack(string suit)
    {
        if (horsePositions.ContainsKey(suit) && horsePositions[suit] > 0)
        {
            int horseIndex = GetHorseIndex(suit);
            horsePositions[suit]--;

            Transform[] track = GetTrack(suit);
            if (track != null)
            {
                StartCoroutine(SmoothMove(horses[horseIndex], track[horsePositions[suit]].position));
            }
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
            case "Heart": return 2;
            case "Diamond": return 1;
            case "Spade": return 0;
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

