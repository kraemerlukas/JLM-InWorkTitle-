using UnityEngine;
using UnityEngine.UIElements;

public class IconController : MonoBehaviour
{
    public bool isDriver;
   [SerializeField] public GameObject Car;
   [SerializeField] public GameObject Beer;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
  

    // Update is called once per frame
    void Start()
    {
        if (isDriver)
        {
            Car.gameObject.SetActive(true);
            Beer.gameObject.SetActive(false);
        }
        else
        {
            Beer.gameObject.SetActive(true);
            Car.gameObject.SetActive(false);
        }
    }
}
