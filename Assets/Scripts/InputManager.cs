using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour {

	public static InputManager Instance = null;

	public KeyCode Key_up;
	public KeyCode Key_left;
	public KeyCode Key_down;
	public KeyCode Key_right;
	public KeyCode Key_activate;

	
	void Awake () {
		if (Instance == null) {
			Instance = this;
			InitializeDefaults ();
			DontDestroyOnLoad (this);
		} else if (Instance != this) {
			Destroy (this);
			Destroy (gameObject);
		}
	}


	public void InitializeDefaults () {
		Key_up = KeyCode.W;
		Key_left = KeyCode.A;
		Key_down = KeyCode.S;
		Key_right = KeyCode.D;
		Key_activate = KeyCode.E;
}

	public Vector3 GetMovementVector() {
		float vert = 0f;
		float hor = 0f;

		if (Input.GetKey (Key_up))
			vert += 1;
		if (Input.GetKey (Key_down))
			vert -= 1;
		if (Input.GetKey (Key_left))
			hor -= 1;
		if (Input.GetKey (Key_right))
			hor += 1;

		return new Vector3 (hor, 0f, vert);
	}
}
