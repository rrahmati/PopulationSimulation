using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic;

public class WorldController : MonoBehaviour
{

    public int population = 20;
    public int currentPop = 0;
    private int IDCounter = 0;
    public float Hamilton_rate = 10;
    public float alpha = 1;
    public float beta = 1;
    public float deviationFromOrigin = 80;
    public GameObject spawnPoint;
    public GameObject spawnObject;
    public float last_check=0;
    public double pop_fit = 0D;
    private string NNInputFileName = "rtNEAT.1.0.2\\src\\in_out\\Fitness_input";
    private string NNOutputFileName = "rtNEAT.1.0.2\\src\\Fitness_output";
    private string agentIDsFilename = "rtNEAT.1.0.2\\src\\in_out\\agentIDs";

    public ArrayList agentList;
    public ArrayList DataList;


    public float EvaluationRateInSec = 3;
    private float timer = 0;

    private double[] fitnessList;

    public bool ExportAllData = false;
    public float DataGatheringInteval = 5f;
    public float dataTimer = 0;
    private string ExportDataFileName = "rtNEAT.1.0.2\\src\\in_out\\Experiment_Data";
    double avgFitness = 0;


    // Use this for initialization
    void Start()
    {
        agentList = new ArrayList();
        DataList = new ArrayList();
    }

    // Update is called once per frame
    void Update()
    {

        SyncAgents();
        if (timer > EvaluationRateInSec)
        {
            EvaluatePopulation();
            timer = 0;
        }

        if (dataTimer > DataGatheringInteval) {
            GatherData();
            dataTimer -= DataGatheringInteval;
        }
        if (ExportAllData) {
            ExportData();
            ExportAllData = false;
        }

        dataTimer += Time.deltaTime;
        timer += Time.deltaTime;

    }

    void GatherData() {

        
        double maxFitness = -1;
        double maxFoodGaveAway = -1;
        double maxLifeTime = -1;
        Data exData = new Data();

        Agent maxFitnessAgent = null;
        Agent maxFoodGaveAwayAgent = null;
        Agent maxLifeTimeAgent = null;

        exData.TimeStamp = Time.time;

        for (int i = 0; i < agentList.Count; i++) {
            Agent agentScript = ((GameObject)agentList[i]).GetComponent<Agent>();
            exData.AvgFoodLevel += agentScript.foodLevel;
            exData.AvgFitness += agentScript.fitness;
            exData.AvgLifeTime += agentScript.lifeTime;
            exData.AvgFoodGaveAway += agentScript.foodGaveAway;
            if (agentScript.fitness > maxFitness) {
                maxFitness = agentScript.fitness;
                maxFitnessAgent = agentScript;
            }
            if (agentScript.foodGaveAway > maxFoodGaveAway) {
                maxFoodGaveAway = agentScript.foodGaveAway;
                maxFoodGaveAwayAgent = agentScript;
            }
            if (agentScript.lifeTime > maxLifeTime) {
                maxLifeTime = agentScript.lifeTime;
                maxLifeTimeAgent = agentScript;
            }
        }

        exData.AvgFoodLevel /= agentList.Count;
        exData.AvgFitness /= agentList.Count;
        exData.AvgLifeTime /= agentList.Count;
        exData.AvgFoodGaveAway /= agentList.Count;

        if (maxFitnessAgent != null) {
            exData.MaxFitness = maxFitness;
            exData.MaxFitnessLifeTime = maxFitnessAgent.lifeTime;
            exData.MaxFitnessGenerosity = maxFitnessAgent.foodGaveAway;
        }
        if (maxFoodGaveAwayAgent != null) {
            exData.MaxGenerousFitness = maxFoodGaveAwayAgent.fitness;
            exData.MaxGenerousLifeTime = maxFoodGaveAwayAgent.lifeTime;
            exData.MaxGenerous = maxFoodGaveAway;
        }
        if (maxLifeTimeAgent != null) {
            exData.MaxLifeTimeFitness = maxLifeTimeAgent.fitness;
            exData.MaxLifeTime = maxLifeTime;
            exData.MaxLifeTimeGenerosity = maxLifeTimeAgent.foodGaveAway;
        }

        
        DataList.Add(exData);
    }

    void ExportData() {
        string path = ExportDataFileName + "_" + Time.time + "_.csv";
        //print(path);
        StreamWriter file = new StreamWriter(path);

        string lines = "";
        

        lines += "Time Stamp,Average Food Level,Avage Fitness,Avage Life Time,Avage Food Gave Away,"+
            "Avg Food Gave Away/AVG lifetime," +
            "Max Fitness,Max Fitness Life Time,Max Fitness Food Gave Away," + 
            "Max Generous Fitness,Max Generous Life Time,Max Generous," +
            "Max Life Time Fitness,Max Life Time,Max Life Time Food Gave Away\n";
        
        for (int i = 0; i < DataList.Count; i++) {
            Data exData = (Data)DataList[i];
            lines += exData.TimeStamp + "," + exData.AvgFoodLevel + "," +
                exData.AvgFitness + "," + exData.AvgLifeTime + "," + exData.AvgFoodGaveAway + "," + (exData.AvgFoodGaveAway /exData.AvgLifeTime) + "," +
                exData.MaxFitness + "," + exData.MaxFitnessLifeTime + "," + exData.MaxFitnessGenerosity + "," +
                exData.MaxGenerousFitness + "," + exData.MaxGenerousLifeTime + "," + exData.MaxGenerous + "," +
                exData.MaxLifeTimeFitness + "," + exData.MaxLifeTime + "," + exData.MaxLifeTimeGenerosity + "\n";
        }
        
        
        file.WriteLine(lines);

        file.Close();
    
    }

    void SpawnAgent(int ID, int sp)
    {
        if (currentPop >= population)
            return;

        // Check the spawn location, if nothing there, then spawn the object
        Vector3 spawnPosition = spawnPoint.transform.position + new Vector3(Random.Range(-deviationFromOrigin, deviationFromOrigin), 0, Random.Range(-deviationFromOrigin, deviationFromOrigin));
        if (!Physics.CheckSphere(spawnPosition, 0.5f))
        {

            // spawn Agent
            GameObject gameObj = (GameObject)Instantiate(spawnObject, spawnPosition, spawnPoint.transform.rotation);

            // Initialize ID, species for the agent
            Agent script = gameObj.GetComponent<Agent>();
            script.ID = ID;
            script.species = sp;
            avgFitness = 0f;
            double avgAge = 0f;
            for (int i = 0; i < agentList.Count; i++)
            {
                Agent agentScript = ((GameObject)agentList[i]).GetComponent<Agent>();
                avgFitness += agentScript.fitness;
                avgAge += agentScript.lifeTime;
            }
            if (agentList.Count > 0)
            {
                avgFitness /= agentList.Count;
                avgAge /= agentList.Count;
            }
            //script.foodLevel = (float)(avgFitness / 12);
            if(avgFitness > 0)
                script.foodLevel = (float)(avgFitness);

            agentList.Add(gameObj);
            currentPop++;
        }
    }

    void SyncAgents()
    {
       // print("syncing agents...");
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
            for (int i = 0; i < IDs.Count; i++)
            {
                SpawnAgent((int)IDs[i], (int)SPs[i]);
            }
        }
        catch
        {
          //  print("Could not read the agentIDs file");
        }


    }

    void EvaluatePopulation()
    {
        // get data from each agent
        double[] fitnessList = new double[agentList.Count];

        Dictionary<int, double> hashtable = new Dictionary<int, double>() ;
        Dictionary<int, int> number = new Dictionary<int, int>();

        for (int i = 0; i < agentList.Count; i++) {
            if (hashtable.ContainsKey(((GameObject)agentList[i]).GetComponent<Agent>().species))
            {
                hashtable[((GameObject)agentList[i]).GetComponent<Agent>().species] += ((GameObject)agentList[i]).GetComponent<Agent>().fitness;
                //Debug.Log(hashtable[((GameObject)agentList[i]).GetComponent<Agent>().species] + " !!!!!!!  " + ((GameObject)agentList[i]).GetComponent<Agent>().fitness);
            }
            else
                hashtable.Add(((GameObject)agentList[i]).GetComponent<Agent>().species, ((GameObject)agentList[i]).GetComponent<Agent>().fitness);

            if (number.ContainsKey(((GameObject)agentList[i]).GetComponent<Agent>().species))
                number[((GameObject)agentList[i]).GetComponent<Agent>().species] += 1;
            else
                number.Add(((GameObject)agentList[i]).GetComponent<Agent>().species, 1);
        }
        

        for (int i = 0; i < agentList.Count; i++)
        {
            //fitnessList[i] = Inclusive_FitnessFunction(((GameObject)agentList[i]).GetComponent<Agent>(), hashtable[((GameObject)agentList[i]).GetComponent<Agent>().species], number[((GameObject)agentList[i]).GetComponent<Agent>().species]);
            // Debug.Log(hashtable[((GameObject)agentList[i]).GetComponent<Agent>().species] + " ||| " + number[((GameObject)agentList[i]).GetComponent<Agent>().species]);
            fitnessList[i] = FitnessFunction(((GameObject)agentList[i]).GetComponent<Agent>(), 1);
            // either Inclusive_FitnessFunction or FitnessFunction 

        }
        //if (Time.time - last_check > 30) // every 30 seconds print the information
        //{
        //    AVG_fit_generosity_species();
        //    last_check = Time.time;
        //}
    }

   /* double alt_penalize(double giver_old_food_level, double food_granted, double rec_old_food_level, double r)
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

    }*/
    double FitnessFunction(Agent agentScript, int s)
    {
        double age = agentScript.lifeTime;
        double food_level = agentScript.foodLevel;
        float recent = 0;
        float beta = 0.1f;
        double penalty = 0; // for now
        if (Time.time - agentScript.last_time_food > 10 && age > 15) // for now it is 10
            recent = Time.time - agentScript.last_time_food;
        //   Debug.Log(Time.time );
        //Debug.Log(Time.time - agentScript.last_time_food );

        //double fitness = alpha * food_level - beta * recent - penalty;
        //double fitness = alpha * food_level /(age /60 + 1);
        double fitness = alpha * food_level;
        //if (agentScript.lifeTime < 30)
        //{
        //    fitness = avgFitness;
        //}
        if (s==1)
            agentScript.fitness = fitness;
        agentScript.hamiltonSatisfied = agentScript.species * 1;
        return fitness;
    }

    double Inclusive_FitnessFunction(Agent agentScript, double same_sp, int n)
    {
        double eps = 0.000001D;
        double fit = FitnessFunction(agentScript, 0);
        double r = 0.5;
     //   double same_sp_fit = 0D; //same_sp_fit would be the sum of fitness of those agent with the same species as agentScript
      //  double pop_fit = 0D; // sum of fitness of whole population
       /* for (int i = 0; i < agentList.Count; i++)
        {
            if (((GameObject)agentList[i]).GetComponent<Agent>().species == agentScript.species)
                same_sp_fit += ((GameObject)agentList[i]).GetComponent<Agent>().fitness; // I am not sure whether these fitness are already updated or not
            pop_fit += ((GameObject)agentList[i]).GetComponent<Agent>().fitness;
           // Debug.Log(((GameObject)agentList[i]).GetComponent<Agent>().fitness + " ^" );
        }*/
        if (pop_fit < 1)
            pop_fit = 1;
        double inc_fitn = fit + r * same_sp / (pop_fit * n); // n is number of agents in the same sp
        agentScript.fitness = inc_fitn;
        Debug.Log("fit= " + fit + " ###### " + " " + (r * same_sp / (pop_fit * n)) + " " + (r * (same_sp / (pop_fit ))) + " "+n);
        return inc_fitn;
    }
    void AVG_fit_generosity_species()
    {
        HashSet<int> sp = new HashSet<int>();

        for (int i = 0; i < agentList.Count; i++)
        {
            int s = ((GameObject)agentList[i]).GetComponent<Agent>().species;
            if (!sp.Contains(s))
                sp.Add(s);
        }
        foreach (int s in sp)
        {
            double fit = 0;
            double generosity = 0;
            int np = 0;
            for (int i = 0; i < agentList.Count; i++)
            {
                if (((GameObject)agentList[i]).GetComponent<Agent>().species == s)
                {
                    np++;
                    generosity += ((GameObject)agentList[i]).GetComponent<Agent>().foodGaveAway;
                    fit += ((GameObject)agentList[i]).GetComponent<Agent>().fitness; // I am not sure whether these fitness are already updated or not
                }
            }
            fit /= np;
            generosity /= np;
            //Debug.Log(s + " avg_fit= " + fit + " avg_generosity= " + generosity);
        }
        Debug.Log("finished");

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

    private class Data {
        public double TimeStamp = 0;

        public double AvgFoodLevel = 0;
        public double AvgFitness = 0;
        public double AvgLifeTime = 0;
        public double AvgFoodGaveAway = 0;
        //public float AvgFoodReceived = 0;


        public double MaxFitness = 0;
        public double MaxFitnessLifeTime = 0;
        public double MaxFitnessGenerosity = 0;


        public double MaxGenerousFitness = 0;
        public double MaxGenerousLifeTime = 0;
        public double MaxGenerous = 0;

        public double MaxLifeTimeFitness = 0;
        public double MaxLifeTime = 0;
        public double MaxLifeTimeGenerosity = 0;


    }
}
