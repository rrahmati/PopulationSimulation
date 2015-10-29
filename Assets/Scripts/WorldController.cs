using UnityEngine;
using System.Collections;

public class WorldController : MonoBehaviour {

	public int population = 20;
	public int currentPop = 0;
	private int IDCounter = 0;
	public GameObject spawnPoint;
	public GameObject spawnObject;

	private string NNInputFileName = "rtNEAT.1.0.2\\NNinput";
	private string NNOutputFileName = "rtNEAT.1.0.2\\NNoutput";
	
	public ArrayList agentList;



	// Use this for initialization
	void Start () {
		agentList = new ArrayList ();

	}
	
	// Update is called once per frame
	void Update () {
		SpawnAgent ();


	}

	void SpawnAgent(){
		if (currentPop > population)
			return;
		
		// Check the spawn location, if nothing there, then spawn the object
		if (!Physics.CheckSphere (spawnPoint.transform.position, 0.5f)) {
			
			// spawn Agent
			GameObject gameObj = (GameObject)Instantiate (spawnObject, spawnPoint.transform.position, spawnPoint.transform.rotation);
			
			// Initialize ID, species for the agent
			Agent script = gameObj.GetComponent<Agent> ();
			script.ID = IDCounter;
			
			
			agentList.Add (gameObj);
			currentPop++;
			IDCounter++;
		}
	}


	void Evalutation(){
		// get data from each agent



	}

}
