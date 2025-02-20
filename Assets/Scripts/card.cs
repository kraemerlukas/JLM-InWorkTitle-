using UnityEngine;
using UnityEngine.UI;
using System.Collections;

using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Card : MonoBehaviour
{
    public RawImage frontImage;  // Vorderseite der Karte
    public RawImage backImage;   // Rückseite der Karte
    public float flipSpeed = 0.5f; // Geschwindigkeit der Drehung

    public string suit; // Herz, Karo, Pik, Kreuz

    public void FlipToFront()
    {
        StartCoroutine(FlipAnimation());
    }

    private IEnumerator FlipAnimation()
    {
        float elapsedTime = 0;
        // Ausgangsrotation (0°) beibehalten
        Quaternion startRotation = transform.rotation;
        // Ziel der ersten Hälfte: um 90° drehen
        Quaternion midRotation = Quaternion.Euler(transform.eulerAngles.x, transform.eulerAngles.y + 90, transform.eulerAngles.z);
        // Ziel der zweiten Hälfte: insgesamt 180° Drehung
        Quaternion endRotation = Quaternion.Euler(transform.eulerAngles.x, transform.eulerAngles.y + 180, transform.eulerAngles.z);

        // Erste Hälfte: Drehung von 0 bis 90 Grad
        while (elapsedTime < flipSpeed)
        {
            transform.rotation = Quaternion.Lerp(startRotation, midRotation, elapsedTime / flipSpeed);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        transform.rotation = midRotation;

        // Beim Erreichen von 90°: Bildwechsel (Rückseite aus, Vorderseite an)
        backImage.gameObject.SetActive(false);
        frontImage.gameObject.SetActive(true);

        elapsedTime = 0;
        // Zweite Hälfte: Drehung von 90 bis 180 Grad
        while (elapsedTime < flipSpeed)
        {
            transform.rotation = Quaternion.Lerp(midRotation, endRotation, elapsedTime / flipSpeed);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        transform.rotation = endRotation;
    }


}
