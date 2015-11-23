using UnityEngine;
using System.Collections;
using System.IO;

public class WorldController : MonoBehaviour
{

    public int population = 20;
    public int currentPop = 0;
    private int IDCounter = 0;
    public float Hamilton_rate = 10;
    public float alpha = 1;
    public float beta = 1;
    public GameObject spawnPoint;
    public GameObject spawnObject;

    private string NNInputFileName = "rtNEAT.1.0.2\\src\\in_out\\Fitness_input";
    private string NNOutputFileName = "rtNEAT.1.0.2\\src\\Fitness_output";
    private string agentIDsFilename = "rtNEAT.1.0.2\\src\\in_out\\agentIDs";

    public ArrayList agentList;

    public float EvaluationRateInSec = 30;
    private float timer = 0;

    private float[] fitnessList;

    // Use this for initialization
    void Start()
    {
        agentList = new ArrayList();
    }

    // Update is called once per frame
    void Update()
    {

        SyncAgents();
        if (timer > EvaluationRateInSec)
        {
            EvaluatePopulation();
        }

        timer += Time.deltaTime;

    }

    void SpawnAgent(int ID, int sp)
    {
        if (currentPop >= population)
            return;

        // Check the spawn location, if nothing there, then spawn the object
        if (!Physics.CheckSphere(spawnPoint.transform.position, 0.5f))
        {

            // spawn Agent
            GameObject gameObj = (GameObject)Instantiate(spawnObject, spawnPoint.transform.position, spawnPoint.transform.rotation);

            // Initialize ID, species for the agent
            Agent script = gameObj.GetComponent<Agent>();
            script.ID = ID;
            script.species = sp;

            agentList.Add(gameObj);
            currentPop++;
        }
    }

    void SyncAgents()
    {
        print("syncing agents...");
        if (!File.Exists(agentIDsFilename))
            return;
        try
        {
            string line = System.IO.File.ReadAllText(agentIDsFilename);
            string[] values = line.Split(',');
            ArrayList IDs = new ArrayList();
            ArrayList SPs = new ArrayList();

            for (int i = 0; i < values.Length; i++)
            {
                if (values[i].Length > 0)
                {
                    if (i % 2 == 0)//skip species
                        IDs.Add(System.Convert.ToInt32(values[i]));
                    else
                        SPs.Add(System.Convert.ToInt32(values[i]));
                }
            }
            print(IDs.Count);
            for (int i = 0; i < agentList.Count; i++)
            {
                int agentID = ((GameObject)agentList[i]).GetComponent<Agent>().ID;
                if (IDs.Contains(agentID))
                {
                    IDs.Remove(agentID);
                }
                else
                {
                    Destroy((GameObject)agentList[i]);
                    agentList.RemoveAt(i);
                    currentPop--;
                }
            }
            print(IDs.Count);
            for (int i = 0; i < IDs.Count; i++)
            {
                SpawnAgent((int)IDs[i], (int)SPs[i]);
            }
        }
        catch
        {
            print("Could not read the agentIDs file");
        }
        
        
    }

    void EvaluatePopulation()
    {
        // get data from each agent
        float[] fitnessList = new float[agentList.Count];

        for (int i = 0; i < agentList.Count; i++)
        {
            fitnessList[i] = FitnessFunction(((GameObject)agentList[i]).GetComponent<Agent>());
            
        }


    }

    double alt_penalize(double giver_old_food_level, double food_granted, double rec_old_food_level, double r)
    {
        double giver_curr_food_level = giver_old_food_level - food_granted;
        double rec_curr_food_level = rec_old_food_level + food_granted;
        // compute the altruism cost for this org upon this particular help
        if (food_granted == 0f)
            return 0;
        double c = 0;
        if (giver_curr_food_level == 0f && food_granted > 0)
            c = 100000; // this agent killed itself for that agent or was in critical point --> should not help
        else
            c = 1 / giver_curr_food_level;
        double b = 1 / (rec_curr_food_level);
        if (b > c / r)
            return 0; // the rule meets

        return (b - c / r) * Hamilton_rate;

    }
    float FitnessFunction(Agent agentScript)
    {
        double age = agentScript.lifeTime;
        double food_level = agentScript.foodLevel;
		float recent = 0;
		float beta = 0.1f;
        double penalty = 0; // for now
		if(Time.time - agentScript.last_time_food > 10 && age > 15) // for now it is 10
		        recent = Time.time - agentScript.last_time_food;
	    Debug.Log(Time.time );
		Debug.Log(Time.time - agentScript.last_time_food );
		
        double fitness = alpha * food_level - beta * recent - penalty;
        agentScript.fitness = (float) fitness;
        agentScript.hamiltonSatisfied = agentScript.species * 1;
        return 0;
    }

    void WriteFitnessInput()
    {
        string path = NNInputFileName;
        //print(path);
        StreamWriter file = new StreamWriter(path);

        string lines = "";
        foreach (float input in fitnessList)
        {
            lines += input + ",";
        }
        lines += "\n";

        file.WriteLine(lines);

        file.Close();
    }

    void ReadFitnessOutput()
    {
        string path = NNOutputFileName;

        if (!File.Exists(path))
            return;

        StreamReader reader = new StreamReader(File.OpenRead(path));

        string line = reader.ReadLine();
        string[] values = line.Split(',');
        for (int output = 0; output < values.Length; output++)
        {
            // do something based on this value

        }
        //print(outputArray);
    }
}
