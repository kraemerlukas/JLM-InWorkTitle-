using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuSwitcher : MonoBehaviour
{
    [SerializeField] private RectTransform StartPanel;
    [SerializeField] private RectTransform OptionsPanel;
    [SerializeField] private RectTransform InfoPanel;
    [SerializeField] private RectTransform ModePanel;
    [SerializeField] private RectTransform TeamPanel;
    [SerializeField] private RectTransform ShopPanel;

    public void StartButton()
    {
        StartPanel.gameObject.SetActive(false);
        ModePanel.gameObject.SetActive(true);
    }

    public void ModeButton(string _mode)
    {
        ModePanel.gameObject.SetActive(false);
        PlayerPrefs.SetString("mode", _mode);
    }

    public void BackButton()
    {
        StartPanel.gameObject.SetActive(true);
        ModePanel.gameObject.SetActive(false);
        InfoPanel.gameObject.SetActive(false);
        OptionsPanel.gameObject.SetActive(false);
        TeamPanel.gameObject.SetActive(false);
        ShopPanel.gameObject.SetActive(false);

    }

    public void TeamModus()
    {
        ModePanel.gameObject.SetActive(false);
        TeamPanel.gameObject.SetActive(true);
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
    public void ShopButtonl()
    {
        StartPanel.gameObject.SetActive(false);
        ShopPanel.gameObject.SetActive(true );
    }

    public void ModeChoose(string _Mode)
    {
        PlayerPrefs.SetString("SelectedMode", _Mode);
    }

}
