using UnityEngine;
using System.Collections;
using System.IO;

public class Agent : MonoBehaviour {

	public int ID = 0;
	public int species = 0;
	public int foodLevel = 100;
	public int foodMaxLevel = 100;
	public int foodGaveAway = 0;
	public int foodLevelDPS = 1;

	public float lifeTime = 0;
	private float time = 0;

	public float moveSpeed = 10f;		
	public float rotateSpeed = 10f;		

	public int coneDegree = 120;
	public int numRaycast = 2;
	public float rayRange = 10;


	// Each raycast have 2 for distance, 2 for object identification
	// and 1 for food level if the target is agent
	// Distance:
	// Identification:
	// 00 = no object
	// 01 = wall
	// 10 = food cube
	// 11 = other agent
	// Food level:
	// 1 = dying
	// 0 = healthy/not agent
	public float[] inputArray;
	private int inputPerRaycast = 4;


	// output from neural network
	// [0, numRaycast - 1] should agent give food to agent at ray cast #
	// 
	// for numRaycast + i:
	// where i:
	// 0 turn left/right
	// 1 move forward/backward
    // 
	public float[] outputArray;
	private int outputNum = 2;


	private Vector3[] rays;
	private int rayAngleStart = 0, rayAngle = 0;

	public bool died = false;
	public bool discreetMovement = false;


    private string NNInputFileName = "rtNEAT.1.0.2\\in_out\\NNinput";
    private string NNOutputFileName = "rtNEAT.1.0.2\\in_out\\NNoutput";
	
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
		outputArray = new float[outputNum];


		if (numRaycast <= 1) {
			rayAngleStart = 90;
            return;
        }

		rayAngleStart = 90 - (coneDegree / 2);
		rayAngle = coneDegree / Mathf.Max((numRaycast - 1),1);
	}
	
	// Update is called once per frame
	void Update () {

		if (died)
			return;

		// change agent color based on the food level
		transform.GetComponent<Renderer> ().material.color = new Color ((100.0f - foodLevel) / 100, foodLevel / 100.0f, 0);

		// food level decrease over time
		if (time > 1) {
			foodLevel -= foodLevelDPS;
			time--;
		}
		time += Time.deltaTime;
		lifeTime += Time.deltaTime;


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
			if(hits[i].distance <= 0){
				inputArray[(index)+0] = 0;
				inputArray[(index)+1] = 0;
				inputArray[(index)+2] = 0;
				inputArray[(index)+3] = 0;
			}
			// for each raycast, determine what kind of input it is
			else{
				float tempDist = hits[i].distance;
                inputArray[(index)] = hits[i].distance / rayRange;

				Debug.Log("ID#: " + ID + " Ray#: " + i + " Tag: " + hits[i].transform.gameObject.tag);
				switch(hits[i].transform.gameObject.tag){
				case "Wall":
					inputArray[(index)+1] = 0;
					inputArray[(index)+2] = 1;
					inputArray[(index)+3] = 0;
					break;
				case "Food":
					inputArray[(index)+1] = 1;
					inputArray[(index)+2] = 0;
					inputArray[(index)+3] = 0;
					break;
				case "Agent":
					inputArray[(index)+1] = 1;
					inputArray[(index)+2] = 1;

					// lower food level mean higher activation value
					Agent other = (hits[i].transform.gameObject).GetComponent<Agent>();
					inputArray[(index)+3] = (foodMaxLevel - other.foodLevel) / foodMaxLevel;

					// if output is to give away food, than give food away
					if(outputArray[i] == 1)
						GiveFood(other);

					break;
				default:
					inputArray[(index)+1] = 0;
					inputArray[(index)+2] = 0;
					inputArray[(index)+3] = 0;
					break;
				}
			}
		}


		// no need to reset output array since
		// either a function call will get the output from ANN, or
		// outputArray will be update by ANN from somewhere else


		// perform output behavior
		if (outputArray [numRaycast + 0] > 0)
            Turn(outputArray[numRaycast + 0]);
		if (outputArray [numRaycast + 1] > 0)
            MoveFB(outputArray[numRaycast + 1]);



		// save input to file
		WriteNNInput ();
		// get output from file
		ReadNNOutput ();

		if (foodLevel < 0)
			died = true;
	}


	void Turn(float directionSpeed){

        directionSpeed *= rotateSpeed;
		if (discreetMovement) {
			if(directionSpeed < 0)
				transform.Rotate (0, -90, 0);
			else
				transform.Rotate (0, 90, 0);

			return;
		}

		transform.Rotate (0, directionSpeed * Time.deltaTime, 0);
	}

    void MoveFB(float value)
    {
        value -= .5f;
        Move(transform.forward * value);
    }

	void Move(Vector3 directionVector){
		// check if there is any thing in fron of it (only short distance
		RaycastHit hit;
		Physics.Raycast(transform.position, transform.forward, out hit, 1);
        //// hit something?
        //if (hit.distance > 0) {
        //    return;
        //}

        //// no hit? move
        //if (discreetMovement) {
        //    transform.position += directionVector * 1;
        //    return;
        //}
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


	void ReadNNOutput()
	{
		string path = NNOutputFileName + "_" + ID;

		if (!File.Exists (path))
			return;

		StreamReader reader = new StreamReader(File.OpenRead(path));

		string line = reader.ReadLine();
		string[] values = line.Split(',');
		for (int output = 0; output < outputArray.Length; output++)
		{
			outputArray[output] = int.Parse(values[output]);
		}
		//print(outputArray);
	}

	void WriteNNInput()
	{
		string path = NNInputFileName + "_" + ID;
        print(path);
		StreamWriter file = new StreamWriter (path);

		string lines = "";
		foreach (float input in inputArray)
		{
			lines += input + ",";
		}
		lines += "\n";

		file.WriteLine (lines);

		file.Close ();
	}
	
	
}
