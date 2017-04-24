using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour {

	private InputManager inputManager;
	private GameController gameController;

	public GameObject worldObject;
	//public new Rigidbody rigidbody;
	private Transform characterModel;
	private Animator anim;
	
	public float speed = 3f;
	public float acceleration = 2f;
	public float dragFactor = 0.8f;
	private Vector3 velocity;

	public bool hasControl { get; private set; }

	private RaycastHit rayhit;


	void Start () {
		inputManager = InputManager.Instance;
		gameController = GameController.Instance;
		//rigidbody = worldObject.GetComponent<Rigidbody> ();
		if (transform.childCount == 0) {
			Debug.LogError ("No character model child found on the player!");
		} else {
			characterModel = transform.GetChild (0);
			anim = characterModel.GetComponent<Animator> ();
			if (!anim) {
				Debug.LogError ("No animator found on the character model child object on the player!");
			}
		}

		hasControl = true;		// TODO : set to false here until world is generated!
	}

	void FixedUpdate () {

		//  Basic input and drag
		Vector3 input = new Vector3 (
				inputManager.GetMovementVector ().x,
				0f,
				inputManager.GetMovementVector ().z
			);

		if (input.x == 0) {
			velocity.x = velocity.x * dragFactor;
		} else {
			velocity.x = velocity.x + (input.x * acceleration / 500f);
		}
		if (input.z == 0) {
			velocity.z = velocity.z * dragFactor;
		} else {
			velocity.z = velocity.z + (input.z * acceleration / 500f);
		}


		// Check it's valid with raycasts
		if (input != Vector3.zero) {
			Debug.DrawLine (transform.position, transform.position + input * 0.85f);
			RaycastHit hitInfo = new RaycastHit();
			Physics.Raycast (transform.position, input, out hitInfo, 0.85f);
			if (hitInfo.collider) {
				if (!hitInfo.collider.isTrigger) {
					velocity.x = velocity.x / 10;
					velocity.z = velocity.z / 10;
				}
			}
		}


		// Cap velocity values at the max speed
		velocity = Vector3.ClampMagnitude (velocity, speed / 50f);


		// Apply calculated velocity to the world!
		worldObject.transform.position = new Vector3 (
				worldObject.transform.position.x - velocity.x,
				worldObject.transform.position.y,
				worldObject.transform.position.z - velocity.z
			);


		// Animate character
		if (input != Vector3.zero) {
			anim.SetBool ("moving", true);

			// Rotate the character according to velocity
			characterModel.rotation = Quaternion.Euler (0f, Mathf.Atan2 (velocity.x, velocity.z) * Mathf.Rad2Deg, 0f);

		} else {
			anim.SetBool ("moving", false);
		}
	}
	



	void Update () {
		// Make sure our player is always at (0, 0) (ish)
		Vector3 changeAmount = Vector3.zero;
		if (transform.position.x != 0f || transform.position.z != 0f) {
			changeAmount = transform.position * -1;
			transform.position = new Vector3 (0f, transform.position.y, 0f);

			for (int i = 0; i < worldObject.transform.childCount; i++) {
				Transform child_i = worldObject.transform.GetChild (i);

				child_i.position = new Vector3 (
						child_i.position.x + changeAmount.x,
						child_i.position.y,
						child_i.position.z + changeAmount.z
					);
			}
		}
	}
}
