using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class DialogueManager : MonoBehaviour {

	[Header("Text")]
	public Text nameText;
	public Text dialogueText;

	[Header("Animator")]
	public Animator animator;

	[Header("Dialogue Components")]
	public GameObject DialogueBox;
    public GameObject nextButton;

	private Queue<string> sentences;
	
	[HideInInspector]
	public bool onDialogue;

	// Use this for initialization
	void Start ()
	{
		sentences = new Queue<string>();
	}

	public void StartDialogue (Dialogue dialogue)
	{
		DialogueBox.SetActive(true);
		onDialogue = true;
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(nextButton);

		animator.SetBool("IsOpen", true);

		nameText.text = dialogue.name;

		sentences.Clear();

		foreach (string sentence in dialogue.sentences)
		{
			sentences.Enqueue(sentence);
		}

		//Display first sentence of the dialogue
		DisplayNextSentence();
	}

	public void DisplayNextSentence ()
	{
		//If there is no more sentence to display then end the dialogue
		if (sentences.Count == 0)
		{
			EndDialogue();
			return;
		}

		string sentence = sentences.Dequeue();
		StopAllCoroutines();
		StartCoroutine(TypeSentence(sentence));
	}

	IEnumerator TypeSentence (string sentence)
	{
		dialogueText.text = "";
		foreach (char letter in sentence.ToCharArray())
		{
			dialogueText.text += letter;
			yield return null;
		}
	}

	void EndDialogue()
	{
		DialogueBox.SetActive(false);
		onDialogue = false;
		animator.SetBool("IsOpen", false);
	}

}