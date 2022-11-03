using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class DOFcontroller : MonoBehaviour
{
    public Volume volume;

    DepthOfField depthOfField;

    [Header("Focus")]
    [Range(5f, 15f)]
    public float focusSpeed = 10f;
    public float minDistance;
    public float maxDistance;

    [Header("Interaction")]
    [Range(3f, 7f)]
    public float pickUpDistance = 5f;
    [Range(0f, 30f)]
    public float pickUpRange = 20f;
    [Range(0f, 20f)]
    public float talkRange = 10f;

    public GameObject focusedObject;
    SelectableObject selectableObject;
    npcTest npc;
    public bool isFocusingObj;

    bool isHit;
    bool isActionPressed;

    [Header("Game Component")]
    public gameManager _gameManager;
    public CameraManager cameraManager;
    public DialogueManager dialogueManager;

    Ray raycast;

    RaycastHit hit;

    float hitDistance;

    private void Start()
    {
        volume.profile.TryGet<DepthOfField>(out depthOfField);
    }

    void Update()
    {
        raycast = new Ray(transform.position, transform.forward * maxDistance);

        if (focusedObject != null)
        {
            hitDistance =
                Vector3
                    .Distance(transform.position,
                    focusedObject.transform.position);
        }
        else
        {
            if (Physics.Raycast(raycast, out hit, 50f))
            {
                isHit = true;
                hitDistance = Vector3.Distance(transform.position, hit.point);

                if (cameraManager.isFPS || cameraManager.isTPS)
                {
                    // Object can only be select if player is in FPS
                    if (cameraManager.isFPS)
                    {
                        //Check if player is facing toward Selectable Object
                        if (
                            hit
                                .collider
                                .gameObject
                                .GetComponent<SelectableObject>() &&
                            hitDistance < pickUpRange
                        )
                        {
                            selectableObject =
                                hit
                                    .collider
                                    .gameObject
                                    .GetComponent<SelectableObject>();
                            selectableObject.OnHoverEnter();

                            _gameManager.Interaction("selectableObject");

                            //Cancel selection while the object is not null
                            if (_gameManager.showingCursor)
                            {
                                _gameManager.Interaction("deselect");

                                selectableObject.OnHoverExit();
                                selectableObject = null;
                            }
                        }
                        else if (selectableObject != null)
                        {
                            _gameManager.Interaction("deselect");

                            selectableObject.OnHoverExit();
                            selectableObject = null;
                        }
                    }
                    else
                    {
                        if (selectableObject != null)
                        {
                            _gameManager.Interaction("deselect");

                            selectableObject.OnHoverExit();
                            selectableObject = null;
                        }
                    }

                    //Check if player is facing toward NPC
                    if (
                        hit.collider.gameObject.GetComponent<npcTest>() &&
                        hitDistance < talkRange
                    )
                    {
                        npc = hit.collider.gameObject.GetComponent<npcTest>();

                        _gameManager.Interaction("npc");

                        //Cancel selection while the object is not null
                        if (_gameManager.showingCursor)
                        {
                            _gameManager.Interaction("deselect");

                            npc = null;
                        }
                    }
                    else if (npc != null)
                    {
                        _gameManager.Interaction("deselect");

                        npc = null;
                    }
                }
                else
                {
                    _gameManager.Interaction("deselect");
                }
            }
            else
            {
                isHit = false;
                _gameManager.Interaction("deselect");

                if (hitDistance < maxDistance)
                {
                    hitDistance++;
                }
            }
        }

        //if hit distance reached minimum distance Or the game is pausing then shift the focus distance to minimum distance
        if (hitDistance < minDistance || _gameManager.onPause)
        {
            hitDistance = minDistance;
        }

        if (!cameraManager.isBEV)
        {
            if (
                Input.GetMouseButtonDown(0) &&
                !dialogueManager.onDialogue &&
                !_gameManager.onPause
            )
            {
                if (selectableObject != null) Interact(selectableObject);
                if (npc != null) Interact(npc);
            }
        }

        SetFocus();
    }

    void Interact(SelectableObject selectableObject)
    {
        // Interact conditions
        // 1. Left mouse button is pressing
        // 2. selectableObject or NPC is available
        // 3. Camera is on FPS mode
        // 4. NOT on dialogue
        // 5. NOT Pausing
        if (focusedObject != selectableObject.gameObject)
        {
            _gameManager.Interaction("deselect");

            focusedObject = selectableObject.gameObject;
            selectableObject
                .Select(transform.position + transform.forward * pickUpDistance,
                transform.position);
            selectableObject.OnHoverExit();
            isFocusingObj = true;
        }
        else
        {
            selectableObject.Deselect();
            focusedObject = null;
            isFocusingObj = false;
        }
    }

    void Interact(npcTest npc)
    {
        // Interact conditions
        // 1. Left mouse button is pressing
        // 2. selectableObject or NPC is available
        // 3. NOT showing cursor
        // 4. NOT on dialogue
        // 5. NOT Pausing

        _gameManager.Interaction("deselect");

        npc.Interact();
    }

    void SetFocus()
    {
        depthOfField.focusDistance.value =
            Mathf
                .Lerp(depthOfField.focusDistance.value,
                hitDistance,
                focusSpeed * Time.deltaTime);
    }

    private void OnDrawGizmos()
    {
        if (isHit)
        {
            Gizmos.DrawSphere(hit.point, 0.1f);

            Debug
                .DrawRay(transform.position,
                transform.forward *
                Vector3.Distance(transform.position, hit.point));
        }
        else
        {
            Debug.DrawRay(transform.position, transform.forward * 100f);
        }
    }
}
