using UnityEngine;
using UnityEngine.UI;
using System.Collections;

using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Card : MonoBehaviour
{
    public Image frontImage;  // Vorderseite der Karte
    public Image backImage;   // Rückseite der Karte
    public float flipSpeed = 0.5f; // Geschwindigkeit der Drehung

    public string suit; // Herz, Karo, Pik, Kreuz

    public void FlipToFront()
    {
        StartCoroutine(FlipAnimation());
    }

    private IEnumerator FlipAnimation()
    {
        float elapsedTime = 0;
        Vector3 startScale = transform.localScale;
        Vector3 midScale = new Vector3(0, 1, 1);
        Vector3 endScale = new Vector3(1, 1, 1);

        while (elapsedTime < flipSpeed)
        {
            transform.localScale = Vector3.Lerp(startScale, midScale, elapsedTime / flipSpeed);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        backImage.gameObject.SetActive(false);
        frontImage.gameObject.SetActive(true);

        elapsedTime = 0;
        while (elapsedTime < flipSpeed)
        {
            transform.localScale = Vector3.Lerp(midScale, endScale, elapsedTime / flipSpeed);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }
}
