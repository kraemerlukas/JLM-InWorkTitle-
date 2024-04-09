using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuSwitcher : MonoBehaviour
{
    [SerializeField] private RectTransform StartPanel;
    [SerializeField] private RectTransform OptionsPanel;
    [SerializeField] private RectTransform InfoPanel;
    [SerializeField] private RectTransform GamePanel;
    [SerializeField] private RectTransform ModePanel;

    public void StartButton()
    {
        StartPanel.gameObject.SetActive(false);
        ModePanel.gameObject.SetActive(true);
    }

    public void ModeButton(string _mode)
    {
        ModePanel.gameObject.SetActive(false);
        PlayerPrefs.SetString("mode", _mode);
        GamePanel.gameObject.SetActive(true);
    }

    public void BackButton()
    {
        StartPanel.gameObject.SetActive(true);
        ModePanel.gameObject.SetActive(false);
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
