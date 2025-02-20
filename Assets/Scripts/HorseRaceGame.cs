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
    private bool gameFinished = false;

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

            // Setze die Karte initial auf Rückseite
            Card cardComponent = fieldCards[i].GetComponent<Card>();
            if (cardComponent != null)
            {
                cardComponent.frontImage.gameObject.SetActive(false);
                cardComponent.backImage.gameObject.SetActive(true);
            }
        }
    }


    public void DrawCard()
    {
        if (gameFinished)
        {
            RestartGame();
            return;
        }

        // Button deaktivieren, um Spammen zu verhindern
        drawCardButton.interactable = false;

        // Letzte gezogene Karte löschen
        if (lastDrawnCard != null)
        {
            Destroy(lastDrawnCard);
        }

        // Zufällige Karte auswählen und instanziieren
        int randomIndex = Random.Range(0, cardPrefabs.Length);
        lastDrawnCard = Instantiate(cardPrefabs[randomIndex], cardSpawnPoint);
        lastDrawnCard.transform.SetParent(cardSpawnPoint, false);
        lastDrawnCard.transform.localPosition = Vector3.zero;

        // Karte initial mit Rückseite anzeigen
        Card card = lastDrawnCard.GetComponent<Card>();
        if (card != null)
        {
            card.frontImage.gameObject.SetActive(false);
            card.backImage.gameObject.SetActive(true);

            // Starte die Coroutine, die den Flip ausführt und anschließend weitere Aktionen
            StartCoroutine(HandleCardFlip(card));
        }
    }



    private IEnumerator HandleCardFlip(Card card)
    {
        yield return new WaitForSeconds(0.2f);

        // Starte die Flip-Animation (drehbasierter Flip)
        card.FlipToFront();

        // Warte die Gesamtdauer der Flip-Animation ab (flipSpeed * 2, da es zwei Phasen gibt)
        yield return new WaitForSeconds(card.flipSpeed * 2);

        // Fahre fort: Pferd bewegen (als Coroutine, damit wir bis zum Ende der Bewegung warten)
        yield return StartCoroutine(MoveHorse(card.suit));

        // Überprüfe, ob Feldkarten umgedreht werden sollen
        CheckFieldCards();

        // Nach allen Animationen: Button wieder aktivieren (falls nicht GameOver)
        drawCardButton.interactable = true;
    }


    private IEnumerator MoveHorse(string suit)
    {
        if (horsePositions.ContainsKey(suit) && horsePositions[suit] < 7)
        {
            int horseIndex = GetHorseIndex(suit);
            horsePositions[suit]++;

            Transform[] track = GetTrack(suit);
            if (track != null && horsePositions[suit] < track.Length)
            {
                // Warte, bis die Pferdebewegung abgeschlossen ist
                yield return StartCoroutine(SmoothMove(horses[horseIndex], track[horsePositions[suit]].position));
            }
        }

        if (horsePositions[suit] == 7)
        {
            winnerText.text = "Gewinner: " + suit;
            gameFinished = true;
            drawCardButton.GetComponentInChildren<TMP_Text>().text = "Restart?";
        }
    }


    void RestartGame()
    {
        // Zurücksetzen des Spielzustands
        gameFinished = false;
        winnerText.text = "";
        drawCardButton.GetComponentInChildren<TMP_Text>().text = "Draw";

        // Zurücksetzen der Pferdepositionen im Dictionary
        horsePositions["Heart"] = 0;
        horsePositions["Diamond"] = 0;
        horsePositions["Spade"] = 0;
        horsePositions["Club"] = 0;

        // Pferde auf die Startpositionen (Index 0) setzen
        horses[GetHorseIndex("Heart")].transform.position = heartTrack[0].position;
        horses[GetHorseIndex("Diamond")].transform.position = diamondTrack[0].position;
        horses[GetHorseIndex("Spade")].transform.position = spadeTrack[0].position;
        horses[GetHorseIndex("Club")].transform.position = clubTrack[0].position;

        // Letzte gezogene Karte entfernen
        if (lastDrawnCard != null)
        {
            Destroy(lastDrawnCard);
            lastDrawnCard = null;
        }

        // Alte Feldkarten zerstören
        for (int i = 0; i < fieldCards.Length; i++)
        {
            if (fieldCards[i] != null)
            {
                Destroy(fieldCards[i]);
            }
        }

        // Flags für die Feldkarten zurücksetzen
        for (int i = 0; i < fieldCardFlipped.Length; i++)
        {
            fieldCardFlipped[i] = false;
        }

        // Neue Feldkarten spawnen
        SpawnFieldCards();
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
        float flipSpeed = 0.5f; // Dauer der halben Drehung
        float elapsedTime = 0;
        Transform cardTransform = fieldCards[index].transform;
        Quaternion startRotation = cardTransform.rotation;
        Quaternion midRotation = Quaternion.Euler(cardTransform.eulerAngles.x, cardTransform.eulerAngles.y + 90, cardTransform.eulerAngles.z);
        Quaternion endRotation = Quaternion.Euler(cardTransform.eulerAngles.x, cardTransform.eulerAngles.y + 180, cardTransform.eulerAngles.z);

        // Erste Hälfte der Drehung: 0 bis 90 Grad
        while (elapsedTime < flipSpeed)
        {
            cardTransform.rotation = Quaternion.Lerp(startRotation, midRotation, elapsedTime / flipSpeed);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        cardTransform.rotation = midRotation;

        // Beim Erreichen der 90° den Bildwechsel durchführen
        Card cardComponent = fieldCards[index].GetComponent<Card>();
        if (cardComponent != null)
        {
            cardComponent.backImage.gameObject.SetActive(false);
            cardComponent.frontImage.gameObject.SetActive(true);
        }

        // Zweite Hälfte der Drehung: 90 bis 180 Grad
        elapsedTime = 0;
        while (elapsedTime < flipSpeed)
        {
            cardTransform.rotation = Quaternion.Lerp(midRotation, endRotation, elapsedTime / flipSpeed);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        cardTransform.rotation = endRotation;

        // Nach dem Flip: Strafaktion (Pferd zurückziehen) ausführen
        if (cardComponent != null)
        {
            MoveHorseBack(cardComponent.suit);
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

    private IEnumerator SmoothMove(GameObject horse, Vector3 targetPosition)
    {
        Vector3 startPosition = horse.transform.position;
        float elapsedTime = 0f;

        while (elapsedTime < moveSpeed)
        {
            // Erzeuge einen weichen Übergang von 0 bis 1
            float t = Mathf.SmoothStep(0f, 1f, elapsedTime / moveSpeed);
            horse.transform.position = Vector3.Lerp(startPosition, targetPosition, t);
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

