using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mouselook : MonoBehaviour
{

    private InputMaster inputmaster;
    public float mouseSensitivity = 100f;

    public Transform playerBody;
    public gameManager GameManager;
    public DialogueManager dialogueManager;

    float xRoatation = 0f;

    private void Awake()
    {
        //inputmaster = new InputMaster();
    }

    private void OnEnable()
    {
        //inputmaster.Enable();
    }

    private void OnDisable()
    {
        //inputmaster.Disable();
    }

    void Update()
    {
        if (GameManager.onPause == false && GameManager.showingCursor == false && dialogueManager.onDialogue == false)
        {
            Look();
        }
    }

    void Look()
    {
        /*
        Vector2 mouse = inputmaster.Player.Look.ReadValue<Vector2>();
        mouse.x = Mathf.Clamp(mouse.x, -1, 1);
        mouse.y = Mathf.Clamp(mouse.y, -1, 1);
        */

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRoatation -= mouseY;
        xRoatation = Mathf.Clamp(xRoatation, -90f, 90f);

        transform.localRotation = Quaternion.Euler(xRoatation, 0f, 0f);
        playerBody.Rotate(Vector3.up * mouseX);
    }
}