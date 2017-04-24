using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class GameController : MonoBehaviour {

	public static GameController Instance = null;

	private GameObject player;
	private Transform ui;
	private Transform ui_dialoguebox;
	private Text ui_dialoguebox_speaker;
	private Text ui_dialoguebox_message;

	public int mapsize = 5;
	public int blocksize = 20;
	public int numberOfTrees = 20;
	public float treeScaleVariance = 0.2f;
	public float forestRadius = 30f;

	private object[,] ground;
	private Vector3[] treePositions;
	private Vector3 forestCentre = Vector3.zero;

	[HideInInspector]
	public GameObject worldObject;
	[HideInInspector]
	public GameObject worldObject_ground;
	[HideInInspector]
	public GameObject worldObject_trees;

	private bool looping = true;
	private float repositionThreshold = 50f;
	private float repositionAmount = 100f;

	void Awake () {
		if (Instance == null) {
			Instance = this;
			DontDestroyOnLoad (this);
		} else if (Instance != this) {
			Destroy (this);
			Destroy (gameObject);
		}
	}

	void Start () {
		player = GameObject.FindGameObjectWithTag ("Player");
		if (!player) {
			Debug.LogError ("Player couldn't be found by the GameController!");
		}

		if (!ui) ui = GameObject.FindGameObjectWithTag("UI Canvas").transform;
		if (!ui) {
			Debug.LogWarning ("UI Canvas couldn't be found by the GameController!");
		} else {
			ui_dialoguebox = ui.FindChild ("Dialogue Box");
			ui_dialoguebox_speaker = ui_dialoguebox.FindChild ("Speaker Text").GetComponent<Text> ();
			ui_dialoguebox_message = ui_dialoguebox.FindChild ("Message Text").GetComponent<Text> ();
			ui_dialoguebox.gameObject.SetActive (false);
		}


		GenerateWorld ();
		
		StartCoroutine (SlowUpdate ());
	}

	void Update () {
		
	}

	private void OnDrawGizmos () {
		Gizmos.color = Color.red;
		Gizmos.DrawWireCube (new Vector3 (0f, 5f, 0f), new Vector3 (100f, 10f, 100f));

		Gizmos.DrawWireSphere (forestCentre, 1f);
		Gizmos.color = Color.green;
		Gizmos.DrawWireSphere (forestCentre, forestRadius);
	}


	private IEnumerator SlowUpdate () {
		while(looping) {

			RepositionWorldObjects ();

			yield return new WaitForSeconds (0.2f);
		}
	}


	public void RepositionWorldObjects () {
		repositionThreshold = mapsize * blocksize / 2;
		repositionAmount = repositionThreshold * 2;

		Vector3 playerChangeAmount = Vector3.zero;
		if (player.transform.position.x != 0f || player.transform.position.z != 0f) {
			playerChangeAmount = player.transform.position * -1;
			player.transform.position = new Vector3 (0f, player.transform.position.y, 0f);
		}

		for (int i = 0; i < worldObject.transform.childCount; i++) {
			Transform child_i = worldObject.transform.GetChild (i);

			child_i.position = new Vector3 (
					child_i.position.x + playerChangeAmount.x,
					child_i.position.y,
					child_i.position.z + playerChangeAmount.z
				);

			for (int j = 0; j < child_i.childCount; j++) {
				Transform child_j = child_i.GetChild (j);

				if (child_j.position.x > repositionThreshold) {
					child_j.position = new Vector3(child_j.position.x - repositionAmount, child_j.position.y, child_j.position.z);
				}
				if (child_j.position.x < -repositionThreshold) {
					child_j.position = new Vector3 (child_j.position.x + repositionAmount, child_j.position.y, child_j.position.z);
				}
				if (child_j.position.z > repositionThreshold) {
					child_j.position = new Vector3 (child_j.position.x, child_j.position.y, child_j.position.z - repositionAmount);
				}
				if (child_j.position.z < -repositionThreshold) {
					child_j.position = new Vector3 (child_j.position.x, child_j.position.y, child_j.position.z + repositionAmount);
				}
			}
		}
	}


	private void GenerateWorld () {

		// First delete the demo world / any old generated world
		worldObject = GameObject.FindGameObjectWithTag ("World");
		if (worldObject)
			Destroy (worldObject);


		ground = new object[5, 5];


	// PARENT OBJECTS
		Debug.Log ("Creating World parent object...");

		worldObject = new GameObject ("World");
		worldObject.transform.position = Vector3.zero;
		worldObject.tag = "World";
		Rigidbody rb = worldObject.AddComponent<Rigidbody> ();
		rb.useGravity = false;
		rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY;
		rb.isKinematic = true;
		rb.mass = 0.01f;
		player.GetComponent<PlayerController> ().worldObject = worldObject;
		
		worldObject_ground = new GameObject ("Ground");
		worldObject_ground.transform.position = Vector3.zero;
		worldObject_ground.transform.SetParent (worldObject.transform);

		worldObject_trees = new GameObject ("Trees");
		worldObject_trees.transform.position = Vector3.zero;
		worldObject_trees.transform.SetParent (worldObject.transform);



		// GROUND
		Debug.Log ("Spawning " + (mapsize * mapsize).ToString() + " ground tiles...");

		float spawnOffset = -(mapsize - 1) / 2;

		for (int i = 0; i < mapsize; i++) {
			for (int j = 0; j < mapsize; j++) {
				GameObject tile = Instantiate ((GameObject)Resources.Load ("Ground/terrain-plane_grass_01"), worldObject_ground.transform, false);
				tile.name = "Grass (" + i + ", " + j + ")";
				tile.transform.localPosition = new Vector3 (
						(i + spawnOffset) * blocksize,
						0f,
						(j + spawnOffset) * blocksize
					);
			}
		}


	// TREES
		Debug.Log ("Spawning " + numberOfTrees + " trees...");

		int forestType = Mathf.RoundToInt ((Random.value * 3) + 0.5f);
		Debug.Log ("Forest type: " + forestType);

		//Vector2 randomPoint = Random.insideUnitCircle;
		//forestCentre = new Vector3 (randomPoint.x, 0, randomPoint.y) * mapsize * blocksize / 2;
		Vector2 randomPoint = new Vector2 (Random.Range (40f, 60f), Random.Range (-20f, 0f));
		forestCentre = new Vector3 (randomPoint.x, 0, randomPoint.y);

		int treeNumber = 8;
		Vector2 randPos = Vector2.zero;
		Vector3 treePosition = Vector3.zero;
		float treeScale = 1.3f;
		string treeType = "";
		bool insideAnotherTree = true;

		if (forestType == 3) {		// More tree for fir
			numberOfTrees = Mathf.RoundToInt (numberOfTrees * 1.3f);
		}
		
		// Make sure our array is big enough to hold the varying number of trees
		treePositions = new Vector3[numberOfTrees];

		for (int i = 0; i < numberOfTrees; i++) {
			if (forestType == 1 || forestType == 2) {
				treeNumber = Mathf.RoundToInt ((Random.value * 8) + 0.5f);
				treeType = "";
			} else if (forestType == 3) {
				treeNumber = Mathf.RoundToInt ((Random.value * 5) + 0.5f);
				treeType = "_fir";
			}

			
			randPos = Random.insideUnitCircle * forestRadius;       // Generate new position and assume that we're inside a tree

			// Try and prove that we aren't in another tree by looping through every existing tree and comparing positions
			for (int j = 0; j < (i - 1); j++) {
				if (Mathf.Abs (Vector3.Magnitude (new Vector3 ((randPos.x + forestCentre.x) - treePositions[j].x, 0f, (randPos.y + forestCentre.z) - treePositions[j].z))) < 2f) {
					Debug.Log ("Tree " + i + " " + (new Vector3(randPos.x + forestCentre.x, 0f, randPos.y + forestCentre.z)).ToString() + " was inside tree " + j + " " + treePositions[j].ToString());
					randPos = Random.insideUnitCircle * forestRadius;
					j = 0;		// If we find an existing tree that we're inside of, then start the for loop again from scratch
					// This for loop will keep starting from j=0 over and over until it can reach the end without finding any collision
					// This will ensure no trees overlap (though if we set the area too small this might end up looping forever so not ideal!)
					// TODO : fallback condition where it gives up after X loops
				}
			}

			treePosition = new Vector3 (randPos.x, 0f, randPos.y);
			treeScale = 1.3f + (Random.value * treeScaleVariance) - (treeScaleVariance / 2);

			GameObject tree = Instantiate ((GameObject)Resources.Load ("Trees/tree" + treeType + treeNumber), worldObject_trees.transform, false);
			tree.name = "Tree " + i + " (type: " + treeNumber + ")";
			tree.transform.position = forestCentre + treePosition;
			tree.transform.localScale = new Vector3 (treeScale, treeScale, treeScale);      // exaggerate the y value
			tree.transform.Rotate (new Vector3 (0f, Random.Range (0f, 360f), 0f));
			
			treePositions[i] = tree.transform.position;
		}



	// BUILDINGS
		Debug.Log ("Spawning buildings...");
		int buildingType = Mathf.RoundToInt ((Random.value * 3) + 0.5f);
		Debug.Log ("Building type: " + buildingType);

		randPos = Random.insideUnitCircle * 5f;


		switch (buildingType) {
			default:
			case 1:
				GameObject building = Instantiate ((GameObject)Resources.Load ("Buildings/Castle"), worldObject.transform, false);
				building.transform.position = new Vector3 (randPos.x, 0f, randPos.y + 30f);
				break;

		}
	}



	public void ShowDialogue (string speakerName, string message) {
		ui_dialoguebox_speaker.text = speakerName;
		ui_dialoguebox_message.text = message;
		ui_dialoguebox.gameObject.SetActive (true);
	}

	public void HideDialogue () {
		ui_dialoguebox.gameObject.SetActive (false);
	}
}
