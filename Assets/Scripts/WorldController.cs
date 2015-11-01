using UnityEngine;
using System.Collections;
using System.IO;

public class WorldController : MonoBehaviour {

	public int population = 20;
	public int currentPop = 0;
	private int IDCounter = 0;
	public GameObject spawnPoint;
	public GameObject spawnObject;

    private string NNInputFileName = "rtNEAT.1.0.2\\in_out\\Fitness_input";
	private string NNOutputFileName = "rtNEAT.1.0.2\\Fitness_output";
	
	public ArrayList agentList;

    public float EvaluationRateInSec = 30;
    private float timer = 0;

    private float[] fitnessList;

	// Use this for initialization
	void Start () {
		agentList = new ArrayList ();
	}
	
	// Update is called once per frame
	void Update () {

        SpawnAgent ();

        if (timer > EvaluationRateInSec) {
            EvaluatePopulation();
        }

        timer += Time.deltaTime;

	}

	void SpawnAgent(){
		if (currentPop >= population)
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


	void EvaluatePopulation(){
		// get data from each agent
        float[] fitnessList = new float[agentList.Count];

        for (int i = 0; i < agentList.Count; i++) {
            fitnessList[i] = FitnessFunction(((GameObject)agentList[i]).GetComponent<Agent>());
        }


	}

    float FitnessFunction(Agent agentScript) {




        return 0;
    }

    void WriteFitnessInput() {
        string path = NNInputFileName;
        //print(path);
        StreamWriter file = new StreamWriter(path);

        string lines = "";
        foreach (float input in fitnessList) {
            lines += input + ",";
        }
        lines += "\n";

        file.WriteLine(lines);

        file.Close();
    }

    void ReadFitnessOutput() {
        string path = NNOutputFileName + "_" + ID;

        if (!File.Exists(path))
            return;

        StreamReader reader = new StreamReader(File.OpenRead(path));

        string line = reader.ReadLine();
        string[] values = line.Split(',');
        for (int output = 0; output < values.Length; output++) {
            // do something based on this value
        
        }
        //print(outputArray);
    }
}
