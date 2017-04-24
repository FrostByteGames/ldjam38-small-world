using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable : MonoBehaviour {

	private PlayerController player;
	private GameController gameController;

	public string speakerName;
	[TextArea]
	public string message;

	// Use this for initialization
	void Start () {
		player = GameObject.FindGameObjectWithTag ("Player").GetComponent<PlayerController> ();
		if (!player) {
			 player = FindObjectOfType<PlayerController> ();
			if (!player) {
				Debug.LogWarning ("Interactable (" + name + ") couldn't locate the player object!");
			}
		}

		gameController = GameController.Instance;
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void OnTriggerEnter (Collider other) {
		if (other.tag == "Player") {
			gameController.ShowDialogue (speakerName, message);
		}
	}

	void OnTriggerExit (Collider other) {
		if (other.tag == "Player") {
			gameController.HideDialogue ();
		}
	}
}
