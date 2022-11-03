using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(DialogueTrigger))]
public class npcTest : MonoBehaviour
{
    DialogueTrigger dialogueTrigger;

    // Start is called before the first frame update
    void Start()
    {
        dialogueTrigger = GetComponent<DialogueTrigger>();
    }

    public void Interact()
    {
        dialogueTrigger.TriggerDialogue();
    }
}
