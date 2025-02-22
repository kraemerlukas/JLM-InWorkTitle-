using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class HorseRaceGame : MonoBehaviour
{
    // UI-Elemente
    public GameObject instructionPanel; // Panel, das den ganzen Bildschirm abdeckt
    public TMP_Text instructionText;    // Text im Panel

    // Spielfelder, Pferde, Karten, etc.
    public GameObject[] horses;
    public Transform[] heartTrack;
    public Transform[] diamondTrack;
    public Transform[] spadeTrack;
    public Transform[] clubTrack;
    public GameObject[] cardPrefabs;
    public Transform cardSpawnPoint;
    private GameObject lastDrawnCard;
    public Transform[] fieldCardPositions;
    private GameObject[] fieldCards;
    private bool[] fieldCardFlipped;

    private Dictionary<string, int> horsePositions = new Dictionary<string, int>()
    {
        {"Heart", 0},
        {"Diamond", 0},
        {"Spade", 0},
        {"Club", 0}
    };

    // Steuerungsvariablen
    private bool canDrawCard = true;
    private bool gameFinished = false;
    private string winnerSuit = "";
    public float moveSpeed = 2.0f;

    void Start()
    {
        Screen.orientation = ScreenOrientation.LandscapeLeft;
        fieldCardFlipped = new bool[fieldCardPositions.Length];
        fieldCards = new GameObject[fieldCardPositions.Length];
        SpawnFieldCards();

        // Setze das Instruction-Panel initial
        instructionPanel.SetActive(true);
        instructionText.text = "Touch zum Ziehen einer Karte...";
    }

    void Update()
    {
        // Prüfe, ob der Klick auf ein UI-Element erfolgt – falls ja, ignoriere den Input
        if (Input.GetMouseButtonDown(0) && UIInputBlocker.Instance != null && UIInputBlocker.Instance.IsPointerOverUIButton())
            return;

        // Reagiere auf Touch bzw. Mausklick, falls Eingaben gerade erlaubt sind
        if (Input.GetMouseButtonDown(0) && canDrawCard)
        {
            // Verberge das Instruction-Panel und starte den Karten-Zieh-Prozess
            instructionPanel.SetActive(false);
            DrawCard();
        }
    }


    public void DrawCard()
    {
        // Falls die Runde beendet ist, starte den Neustart
        if (gameFinished)
        {
            RestartGame();
            return;
        }

        // Sperre weitere Eingaben
        canDrawCard = false;

        // Entferne letzte gezogene Karte, falls vorhanden
        if (lastDrawnCard != null)
        {
            Destroy(lastDrawnCard);
        }

        // Wähle zufällig eine Karte und instanziiere sie
        int randomIndex = Random.Range(0, cardPrefabs.Length);
        lastDrawnCard = Instantiate(cardPrefabs[randomIndex], cardSpawnPoint);
        lastDrawnCard.transform.SetParent(cardSpawnPoint, false);
        lastDrawnCard.transform.localPosition = Vector3.zero;

        // Zeige die Karte initial mit Rückseite an
        Card card = lastDrawnCard.GetComponent<Card>();
        if (card != null)
        {
            card.frontImage.gameObject.SetActive(false);
            card.backImage.gameObject.SetActive(true);

            // Starte die Animation und den weiteren Ablauf
            StartCoroutine(HandleCardFlip(card));
        }
    }

    private IEnumerator HandleCardFlip(Card card)
    {
        yield return new WaitForSeconds(0.2f);

        // Starte den Kartenflip (Rotation um die Y-Achse)
        card.FlipToFront();

        // Warte, bis die Flip-Animation abgeschlossen ist
        yield return new WaitForSeconds(card.flipSpeed * 2);

        // Bewege das Pferd gemäß dem Kartenwert
        yield return StartCoroutine(MoveHorse(card.suit));

        // Warte auf alle Fieldcard-Flips (falls welche starten)
        yield return StartCoroutine(CheckFieldCardsAndWait());

        // Instruction-Panel mit passendem Text wieder einblenden:
        if (gameFinished)
        {
            instructionText.text = "Runde Vorbei: Gewinner ist " + winnerSuit + ", Touch für neues Spiel!";
        }
        else
        {
            instructionText.text = "Touch zum Ziehen einer Karte...";
        }
        instructionPanel.SetActive(true);

        // Nun sind wieder Eingaben möglich
        canDrawCard = true;
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
                // Warte, bis die Bewegung abgeschlossen ist
                yield return StartCoroutine(SmoothMove(horses[horseIndex], track[horsePositions[suit]].position));
            }
        }

        if (horsePositions[suit] == 7)
        {
            winnerSuit = suit;
            gameFinished = true;
        }
    }
    private IEnumerator CheckFieldCardsAndWait()
    {
        for (int i = 0; i < fieldCards.Length; i++)
        {
            // Falls diese Karte schon einmal geflippt wurde, überspringen
            if (fieldCardFlipped[i]) continue;

            // Prüfe, ob alle Pferde diese Position bereits passiert haben
            bool allHorsesHere = true;
            foreach (var pos in horsePositions.Values)
            {
                if (pos <= i)
                {
                    allHorsesHere = false;
                    break;
                }
            }

            // Falls die Bedingungen erfüllt sind, starte und warte auf den Flip
            if (allHorsesHere)
            {
                fieldCardFlipped[i] = true; // Verhindert mehrfaches Flippen
                yield return StartCoroutine(FlipFieldCard(i));
            }
        }
    }

    private IEnumerator SmoothMove(GameObject horse, Vector3 targetPosition)
    {
        Vector3 startPosition = horse.transform.position;
        float elapsedTime = 0f;
        while (elapsedTime < moveSpeed)
        {
            float t = Mathf.SmoothStep(0f, 1f, elapsedTime / moveSpeed);
            horse.transform.position = Vector3.Lerp(startPosition, targetPosition, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        horse.transform.position = targetPosition;
    }

    void SpawnFieldCards()
    {
        for (int i = 0; i < fieldCardPositions.Length; i++)
        {
            fieldCards[i] = Instantiate(cardPrefabs[Random.Range(0, cardPrefabs.Length)], fieldCardPositions[i].position, Quaternion.identity);
            fieldCards[i].transform.SetParent(fieldCardPositions[i], false);
            fieldCards[i].transform.localPosition = Vector3.zero;

            // Setze Feldkarte initial auf Rückseite
            Card cardComponent = fieldCards[i].GetComponent<Card>();
            if (cardComponent != null)
            {
                cardComponent.frontImage.gameObject.SetActive(false);
                cardComponent.backImage.gameObject.SetActive(true);
            }
        }
    }

    void CheckFieldCards()
    {
        for (int i = 0; i < fieldCards.Length; i++)
        {
            if (fieldCardFlipped[i]) continue;

            bool allHorsesHere = true;
            foreach (var pos in horsePositions.Values)
            {
                if (pos <= i)
                {
                    allHorsesHere = false;
                    break;
                }
            }

            if (allHorsesHere)
            {
                fieldCardFlipped[i] = true;
                StartCoroutine(FlipFieldCard(i));
            }
        }
    }

    IEnumerator FlipFieldCard(int index)
    {
        float flipSpeed = 0.5f;
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

        // Beim Erreichen von 90° den Bildwechsel durchführen
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

        // Strafaktion: Pferd zurückbewegen und auf diese Bewegung warten
        if (cardComponent != null)
        {
            yield return StartCoroutine(MoveHorseBack(cardComponent.suit));
        }
    }

    private IEnumerator MoveHorseBack(string suit)
    {
        if (horsePositions.ContainsKey(suit) && horsePositions[suit] > 0)
        {
            int horseIndex = GetHorseIndex(suit);
            horsePositions[suit]--;

            Transform[] track = GetTrack(suit);
            if (track != null)
            {
                yield return StartCoroutine(SmoothMove(horses[horseIndex], track[horsePositions[suit]].position));
            }
        }
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

    void RestartGame()
    {
        gameFinished = false;
        winnerSuit = "";
        instructionText.text = "Touch zum Ziehen einer Karte...";
        instructionPanel.SetActive(true);
        canDrawCard = true;

        // Zurücksetzen der Pferdepositionen
        horsePositions["Heart"] = 0;
        horsePositions["Diamond"] = 0;
        horsePositions["Spade"] = 0;
        horsePositions["Club"] = 0;

        horses[GetHorseIndex("Heart")].transform.position = heartTrack[0].position;
        horses[GetHorseIndex("Diamond")].transform.position = diamondTrack[0].position;
        horses[GetHorseIndex("Spade")].transform.position = spadeTrack[0].position;
        horses[GetHorseIndex("Club")].transform.position = clubTrack[0].position;

        if (lastDrawnCard != null)
        {
            Destroy(lastDrawnCard);
            lastDrawnCard = null;
        }

        // Alte Feldkarten entfernen
        for (int i = 0; i < fieldCards.Length; i++)
        {
            if (fieldCards[i] != null)
            {
                Destroy(fieldCards[i]);
            }
        }
        for (int i = 0; i < fieldCardFlipped.Length; i++)
        {
            fieldCardFlipped[i] = false;
        }
        SpawnFieldCards();
    }
}
