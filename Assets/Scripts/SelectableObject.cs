using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof (DialogueTrigger))]
public class SelectableObject : MonoBehaviour
{
    [Header("Material")]
    public Material defaultMaterial;

    public Material highlightMaterial;

    [Header("Interact")]
    public bool canPickUp;

    [Range(3f, 7f)]
    public float pickUpSpeed = 5f;

    public Vector3 startingPosition;

    public Vector3 targetPosition;

    public Quaternion startingRotation;

    public Quaternion targetRotation;

    bool isHover = false;

    DialogueTrigger dialogueTrigger;

    private void Awake()
    {
        defaultMaterial = gameObject.GetComponent<Renderer>().sharedMaterial;
    }

    private void Start()
    {
        startingPosition = transform.position;
        targetPosition = startingPosition = transform.position;

        startingRotation = transform.rotation;
        targetRotation = startingRotation;

        dialogueTrigger = GetComponent<DialogueTrigger>();
    }

    private void Update()
    {
        if (canPickUp) MoveObject();
    }

    void MoveObject()
    {
        transform.position =
            Vector3
                .Lerp(transform.position,
                targetPosition,
                Time.deltaTime * pickUpSpeed);
        if (Input.GetMouseButton(1)) return;
        transform.rotation =
            Quaternion
                .Slerp(transform.rotation,
                targetRotation,
                Time.deltaTime * pickUpSpeed);
    }

    public void OnHoverEnter()
    {
        if (isHover) return;

        //Debug.Log("Set Hover");
        if (highlightMaterial != null)
            gameObject.GetComponent<Renderer>().sharedMaterial =
                highlightMaterial;
        else
            Debug
                .Log("Highlight material is not attach to Object " +
                gameObject.name);

        isHover = true;
    }

    public void OnHoverExit()
    {
        if (!isHover) return;

        //Debug.Log("Hover Exit");
        gameObject.GetComponent<Renderer>().sharedMaterial = defaultMaterial;
        isHover = false;
    }

    public void Select(Vector3 focusPosition, Vector3 target)
    {
        if (canPickUp)
        {
            //Set target position to player
            targetPosition = focusPosition;

            //Set rotation of the object to face the player
            transform.LookAt (target);
            targetRotation = transform.rotation;
        }

        //Enable selected object's dialogue
        dialogueTrigger.TriggerDialogue();
    }

    public void Deselect()
    {
        if (!canPickUp) return;

        //Reset target position and rotation to origin
        targetPosition = startingPosition;
        targetRotation = startingRotation;
    }
}
