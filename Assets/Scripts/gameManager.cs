using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class gameManager : MonoBehaviour
{
    [Header("Game Components")]
    public AudioManager audioManager;

    public CameraManager cameraManager;

    public DialogueManager dialogueManager;

    public DOFcontroller dofController;

    [Header("Light source")]
    public GameObject light_1;

    public GameObject light_2;

    public GameObject light_3;

    [Header("UI Pages")]
    public GameObject OnplayUI;

    public GameObject OnpauseUI;

    public GameObject OnhelpUI;

    public GameObject OnsettingUI;

    [Header("Game's State")]
    public bool onPause = false;

    public bool onHelp = false;

    public bool onSetting = false;

    public bool subMenu = false;

    public bool showingCursor = false;

    [Header("Button Components")]
    public GameObject ResumeButton;

    public GameObject HelpButton;

    public GameObject SettingButton;

    public GameObject BackButton;

    [Header("Interaction Text")]
    public GameObject interact;

    public Text interactText;

    string text;

    public void Pause()
    {
        OnplayUI.SetActive(false);
        OnpauseUI.SetActive(true);
        OnhelpUI.SetActive(false);
        OnsettingUI.SetActive(false);
        BackButton.SetActive(false);

        if (onHelp)
        {
            FindObjectOfType<AudioManager>().Play("cancel");
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject (HelpButton);
        }
        else if (onSetting)
        {
            FindObjectOfType<AudioManager>().Play("cancel");
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject (SettingButton);
        }
        else
        {
            FindObjectOfType<AudioManager>().Play("pause");
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject (ResumeButton);
        }

        onPause = true;
        onHelp = false;
        onSetting = false;
        subMenu = false;
    }

    public void Help()
    {
        OnpauseUI.SetActive(false);
        OnhelpUI.SetActive(true);
        BackButton.SetActive(true);
        onHelp = true;
        subMenu = true;
        FindObjectOfType<AudioManager>().Play("confirm");
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject (BackButton);
    }

    public void Setting()
    {
        OnpauseUI.SetActive(false);
        OnsettingUI.SetActive(true);
        BackButton.SetActive(true);
        onSetting = true;
        subMenu = true;
        FindObjectOfType<AudioManager>().Play("confirm");
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject (BackButton);
    }

    public void Resume()
    {
        OnplayUI.SetActive(true);
        OnpauseUI.SetActive(false);
        if (onPause)
        {
            FindObjectOfType<AudioManager>().Play("resume");
        }
        onPause = false;
        subMenu = false;
    }

    public void Quitgame()
    {
        Application.Quit();
        Debug.Log("Quit game");
    }

    void Awake()
    {
        OnplayUI.SetActive(true);
        OnpauseUI.SetActive(false);
        OnhelpUI.SetActive(false);
        OnsettingUI.SetActive(false);
        BackButton.SetActive(false);
        interact.SetActive(false);
        EventSystem.current.SetSelectedGameObject(null);
    }

    void Start()
    {
        Resume();

        light_1.SetActive(true);
        light_2.SetActive(false);
        light_3.SetActive(false);

        showingCursor = false;
    }

    void Update()
    {
        Check();
        HandleCursor();
        HandleLight();
    }

    void HandleLight()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            light_1.SetActive(true);
            light_2.SetActive(false);
            light_3.SetActive(false);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            light_1.SetActive(false);
            light_2.SetActive(true);
            light_3.SetActive(false);
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            light_1.SetActive(false);
            light_2.SetActive(false);
            light_3.SetActive(true);
        }
    }

    void Check()
    {
        //Pause and Resume while not on dialog
        if (
            Input.GetKeyDown(KeyCode.Escape) &&
            !dialogueManager.onDialogue &&
            !subMenu
        )
        {
            //If already on pause menu then resume
            if (onPause)
            {
                Resume();
                return;
            }

            //If not then enter pause menu
            Pause();
            return;
        }

        //Enter help and Setting and Quit
        if (!subMenu && onPause)
        {
            if (Input.GetKeyDown(KeyCode.Q)) Help();
            if (Input.GetKeyDown(KeyCode.E)) Setting();
            if (Input.GetKeyDown(KeyCode.Backspace)) Quitgame();
            return;
        }

        //Exit from Help or Setting page
        if (
            (onHelp && Input.GetKeyDown(KeyCode.Q)) ||
            (onSetting && Input.GetKeyDown(KeyCode.E)) ||
            (subMenu && Input.GetKeyDown(KeyCode.Escape))
        )
        {
            Pause();
            return;
        }
    }

    void HandleCursor()
    {
        //Always show cursor while BEv camera is active
        //Show cursor when hold left alt key while play AND not in BEV mode
        if (
            cameraManager.isBEV ||
            onPause ||
            Input.GetKey(KeyCode.LeftAlt) ||
            dialogueManager.onDialogue ||
            dofController.isFocusingObj
        )
        {
            showingCursor = true;
        }
        else
        {
            showingCursor = false;
        }

        //Show cursor during dialogue
        if (showingCursor)
        {
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    public void Interaction(string type)
    {
        if (type == "deselect")
        {
            interact.SetActive(false);
            return;
        }

        if (type == "selectableObject") text = "Interact";
        if (type == "npc") text = "Talk";

        interact.SetActive(true);
        interactText.text = text;
    }
}
