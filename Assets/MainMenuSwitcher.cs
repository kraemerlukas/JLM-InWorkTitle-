using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuSwitcher : MonoBehaviour
{
    [SerializeField] private RectTransform StartPanel;
    [SerializeField] private RectTransform OptionsPanel;
    [SerializeField] private RectTransform InfoPanel;
    [SerializeField] private RectTransform GamePanel;

    public void StartButton()
    {
        StartPanel.gameObject.SetActive(false);
        GamePanel.gameObject.SetActive(true);
    }

    public void BackButton()
    {
        StartPanel.gameObject.SetActive(true);
        GamePanel.gameObject.SetActive(false);
        InfoPanel.gameObject.SetActive(false);
        OptionsPanel.gameObject.SetActive(false);
    }

    public void InfoButton()
    {
        StartPanel.gameObject.SetActive(false);
        InfoPanel.gameObject.SetActive(true);
    }

    public void OptionsButton()
    {
        StartPanel.gameObject.SetActive(false);
        OptionsPanel.gameObject.SetActive(true);
    }

}
