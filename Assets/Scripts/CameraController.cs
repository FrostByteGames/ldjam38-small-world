using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {
	
	public Vector3 offset = new Vector3 (0f, 15f, -8f);
	public float maxDistance = 0.2f;
	
	public float smoothness = 3f;

	[SerializeField]
	private GameObject player;
	private InputManager inputManager;
	private Vector3 distance;


	void Start () {
		inputManager = InputManager.Instance;

		player = GameObject.FindGameObjectWithTag ("Player");
		if (!player) {
			Debug.LogError ("Camera could not find a player object!");
		}

	}
	

	void FixedUpdate () {
		/*
		distance = new Vector3 (
				transform.position.x - player.transform.position.x - offset.x,
				0f,
				transform.position.z - player.transform.position.z - offset.z
			);
		
		if (Mathf.Abs(distance.magnitude) > maxDistance) {
			transform.position -= distance / smoothness;
		}
		*/

		distance = new Vector3 (
				transform.position.x - player.transform.position.x - offset.x,
				0f,
				transform.position.z - player.transform.position.z - offset.z
			);


		transform.position = Vector3.zero + offset;
	}
}
