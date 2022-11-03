using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{

    [Header("UI Pages")]
    public GameObject MainMenuUI;
    public GameObject OnsettingUI;

    [Header("Game's State")]
    public bool onSetting = false;

    [Header("Button Component")]
    public GameObject StartButton;

    public GameObject SettingButton;

    public GameObject BackButton;

    // Start is called before the first frame update
    void Start()
    {
        Back();
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject (StartButton);
    }

    // Update is called once per frame
    void Update()
    {
        Check();
    }

    void Check()
    {
        if (Input.GetKeyDown(KeyCode.E)) Setting();
    }

    public void Setting()
    {
        if (onSetting)
        {
            Back();
            return;
        }
        MainMenuUI.SetActive(false);
        OnsettingUI.SetActive(true);

        onSetting = true;

        FindObjectOfType<AudioManager>().Play("confirm");
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject (BackButton);
    }

    public void Back()
    {
        MainMenuUI.SetActive(true);
        OnsettingUI.SetActive(false);

        if (onSetting)
        {
            FindObjectOfType<AudioManager>().Play("cancel");
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject (SettingButton);
        }

        onSetting = false;
    }
}
