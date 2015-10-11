using UnityEngine;
using System.Collections;

public class Agent : MonoBehaviour {

	public int ID = 0;
	public int foodLevel = 100;
	public int foodMaxLevel = 100;
	public int foodGaveAway = 0;
	public int foodLevelDPS = 1;
	private float time = 0;

	public float moveSpeed = 10f;		
	public float rotateSpeed = 10f;		

	public int coneDegree = 120;
	public int numRaycast = 7;
	public float rayRange = 10;


	// Each raycast have 2 for distance, 2 for object identification
	// and 1 for food level if the target is agent
	// Distance:
	// [00, 01] real number, first half of the range
	// [10, 11] real number, remain half of the range
	// Identification:
	// 00 = no object
	// 01 = wall
	// 10 = food cube
	// 11 = other agent
	// Food level:
	// 1 = dying
	// 0 = healthy/not agent
	public float[] inputArray;
	private int inputPerRaycast = 5;


	// output from neural network
	// [0, numRaycast - 1] should agent give food to agent at ray cast #
	// 
	// for numRaycast + i:
	// where i:
	// 0,1 turn left/right
	// 2,3,4,5 move forward/backward/left/right
	public int[] outputArray;
	private int outputNum = 6;


	private Vector3[] rays;
	private int rayAngleStart = 0, rayAngle = 0;

	public bool died = false;
	public bool discreetMovement = false;

	// Use this for initialization
	void Start () {

		if (discreetMovement) {
			transform.position = new Vector3(Mathf.Round(transform.position.x),
			                                 Mathf.Round(transform.position.y),
			                                 Mathf.Round(transform.position.z));
			transform.Rotate(new Vector3(0,0,1));
	 }

		// need at least a raycast
		numRaycast = Mathf.Max (numRaycast, 1);
		rays = new Vector3[numRaycast];

		inputArray = new float[numRaycast*inputPerRaycast];

		outputNum += numRaycast;
		outputArray = new int[outputNum];


		if (numRaycast <= 1)
			return;

		rayAngleStart = 90 - (coneDegree / 2);
		rayAngle = coneDegree / Mathf.Max((numRaycast - 1),1);
	}
	
	// Update is called once per frame
	void Update () {

		if (died)
			return;

		// food level decrease over time
		if (time > 1) {
			foodLevel -= foodLevelDPS;
			time--;
		}
		time += Time.deltaTime;


		RaycastHit[] hits = new RaycastHit[numRaycast];
		for (int i = 0; i < numRaycast; i++) {

			rays[i] = new Vector3(Mathf.Cos(((rayAngle * i) + rayAngleStart)*Mathf.Deg2Rad), 
			          			  0,
			                      Mathf.Sin(((rayAngle * i) + rayAngleStart)*Mathf.Deg2Rad));
			rays[i] = rays[i].normalized;
			rays[i] = transform.TransformDirection(rays[i]);
			Debug.DrawRay (transform.position, rays[i] * rayRange, Color.red);
			Physics.Raycast(transform.position, rays[i], out hits[i], rayRange);

			int index = i * inputPerRaycast;
			// no hit? set everything related to zero
			if(hits[i].distance < 0){
				inputArray[(index)+0] = 0;
				inputArray[(index)+1] = 0;
				inputArray[(index)+2] = 0;
				inputArray[(index)+3] = 0;
				inputArray[(index)+4] = 0;
			}
			// for each raycast, determine what kind of input it is
			else{
				float tempDist = hits[i].distance;
				// fill the distance inputs
				if(tempDist > rayRange/2){
					inputArray[(index)] = 1;
					tempDist -= rayRange/2;
					inputArray[(index)+1] = tempDist/rayRange;
				}
				else{
					inputArray[(index)] = 0;
					inputArray[(index)+1] = tempDist/rayRange;
				}


				Debug.Log("ID#: " + ID + " Ray#: " + i + " Tag: " + hits[i].transform.gameObject.tag);
				switch(hits[i].transform.gameObject.tag){
				case "Wall":
					inputArray[(index)+2] = 0;
					inputArray[(index)+3] = 1;
					inputArray[(index)+4] = 0;
					break;
				case "Food":
					inputArray[(index)+2] = 1;
					inputArray[(index)+3] = 0;
					inputArray[(index)+4] = 0;
					break;
				case "Agent":
					inputArray[(index)+2] = 1;
					inputArray[(index)+3] = 1;

					// lower food level mean higher activation value
					Agent other = (hits[i].transform.gameObject).GetComponent<Agent>();
					inputArray[(index)+4] = (foodMaxLevel - other.foodLevel) / foodMaxLevel;

					// if output is to give away food, than give food away
					if(outputArray[i] == 1)
						GiveFood(other);

					break;
				default:
					inputArray[(index)+2] = 0;
					inputArray[(index)+3] = 0;
					inputArray[(index)+4] = 0;
					break;
				}
			}
		}


		// no need to reset output array since
		// either a function call will get the output from ANN, or
		// outputArray will be update by ANN from somewhere else


		// perform output behavior
		if (outputArray [numRaycast + 0] > 0)
			TurnLeft ();
		if (outputArray [numRaycast + 1] > 0)
			TurnRight ();
		if (outputArray [numRaycast + 2] > 0)
			MoveFoward ();
		if (outputArray [numRaycast + 3] > 0)
			MoveBackward ();
		if (outputArray [numRaycast + 4] > 0)
			MoveLeft ();
		if (outputArray [numRaycast + 5] > 0)
			MoveRight ();

		if (foodLevel < 0)
			died = true;
	}


	void TurnLeft(){
		Turn (-rotateSpeed);
	}
	void TurnRight(){
		Turn (rotateSpeed);
	}
	void Turn(float directionSpeed){

		if (discreetMovement) {
			if(directionSpeed < 0)
				transform.Rotate (0, -90, 0);
			else
				transform.Rotate (0, 90, 0);

			return;
		}

		transform.Rotate (0, directionSpeed * Time.deltaTime, 0);
	}

	void MoveFoward(){
		//transform.position += transform.forward * moveSpeed * Time.deltaTime;
		Move (transform.forward);
	}
	void MoveBackward(){
		//transform.position -= transform.forward * moveSpeed * Time.deltaTime;
		Move (-transform.forward);
	}
	void MoveLeft(){
		//transform.position -= transform.right * moveSpeed * Time.deltaTime;
		Move (-transform.right);
	}
	void MoveRight(){
		//transform.position += transform.right * moveSpeed * Time.deltaTime;
		Move (transform.right);
	}
	void Move(Vector3 directionVector){
		// check if there is any thing in fron of it (only short distance
		RaycastHit hit;
		Physics.Raycast(transform.position, transform.forward, out hit, 1);
		// hit something?
		if (hit.distance > 0) {
			return;
		}

		// no hit? move
		if (discreetMovement) {
			transform.position += directionVector * 1;
			return;
		}
		transform.position += directionVector * moveSpeed * Time.deltaTime;
	}
	
	// Agent may die after give away food, (Self-sacrifice)
	void GiveFood(Agent other){
		int foodGiveAway = Mathf.Max(Mathf.Min (10, foodLevel), 0);

		// if agent ever five away food, we really want to know
		// especially if it is a self sacrifice action
		Debug.Log ("Agent " + ID + " just give a way " + foodGiveAway + " food to Agent " + other.ID);
		if(foodGiveAway < 10)
			Debug.Log ("Agent " + ID + " just sacrifice its life for Agent " + other.ID);

		other.foodLevel = Mathf.Min(foodMaxLevel, other.foodLevel + foodGiveAway);
		foodLevel -= foodGiveAway;
		foodGaveAway += foodGiveAway;
	}



	void OnTriggerEnter(Collider other){
		if (other.gameObject.tag == "Food") {
			foodLevel = Mathf.Min(foodLevel+10, foodMaxLevel);
			Destroy (other.gameObject);
		}
	}

}
