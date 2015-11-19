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

    private string NNInputFileName = "rtNEAT.1.0.2\\in_out\\Fitness_input";
    private string NNOutputFileName = "rtNEAT.1.0.2\\Fitness_output";
    private string agentIDsFilename = "rtNEAT.1.0.2\\in_out\\agentIDs";

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

    void SpawnAgent(int ID)
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


            agentList.Add(gameObj);
            currentPop++;
        }
    }

    void SyncAgents()
    {
        if (!File.Exists(agentIDsFilename))
            return;
        StreamReader reader = new StreamReader(File.OpenRead(agentIDsFilename));

        string line = reader.ReadLine();
        string[] values = line.Split(',');
        ArrayList IDs = new ArrayList();
        for (int i = 0; i < values.Length; i++)
        {
            if(values[i].Length > 0)
                IDs.Add(System.Convert.ToInt32(values[i]));
        }
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
                agentList.Remove((GameObject)agentList[i]);
            }
        }
        for (int i = 0; i < IDs.Count; i++)
        {
            SpawnAgent((int)IDs[i]);
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
        double food_gain = agentScript.foodLevel;
        double penalty = 0; // for now
        double fitness = alpha * food_gain - penalty;
        agentScript.fitness = (float) fitness;
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
