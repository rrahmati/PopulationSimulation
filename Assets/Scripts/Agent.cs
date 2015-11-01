using UnityEngine;
using System.Collections;
using System.IO;

public class Agent : MonoBehaviour {

    public int ID;
    public int species = 0;
    public float foodLevel = 100;
    public float foodMaxLevel = 100;
    public float foodGaveAway = 0;
    public float foodLevelDPS = 1;

    public float lifeTime = 0;
    private float time = 0;

    public float moveSpeed = 10f;
    public float rotateSpeed = 10f;

    public float coneDegree = 120;
    public int numRaycast = 5;
    public float rayRange = 10;

    public int numPieSlice = 3;
    public float sliceRange = 10;


    // Each pie slice have 2 input:
    // - distance to detected food cube	(maxRange - distancce) / maxRange
    // - distance to detected agent		(maxRange - distancce) / maxRange

    // Each raycast have 1 input:
    // - distance to the detected wall	(maxRange - distancce) / maxRange

    public float[] inputArray;
    private int inputsPerPieSlice = 2;
    private int inputsPerRaycast = 1;
    private int extraInputs = 2;


    // output from neural network
    // - turn left/right
    // - move forward/backward
    // - give food away or not
    public float[] outputArray;
    private int outputNum = 3;


    private Vector3[] rays;
    private float rayAngleStart = 0, rayAngle = 0;

    private float[] pieSliceAngles;


    public bool died = false;
    //public bool discreetMovement = false;


    private string NNInputFileName = "rtNEAT.1.0.2\\in_out\\NNinput";
    private string NNOutputFileName = "rtNEAT.1.0.2\\in_out\\NNoutput";

    // Use this for initialization
    void Start() {

        // need at least a raycast
        numRaycast = Mathf.Max(numRaycast, 1);
        rays = new Vector3[numRaycast];

        inputArray = new float[numRaycast * inputsPerRaycast + numPieSlice * inputsPerPieSlice + extraInputs];

        //outputNum += numRaycast + 1;
        outputArray = new float[outputNum];

        // setup angles for pie slice
        // need 4 numbers for 3 slices
        pieSliceAngles = new float[numPieSlice + 1];
        float degreeOfEachPieSlice = coneDegree / numPieSlice;
        pieSliceAngles[0] = -coneDegree / 2;
        for (int i = 1; i < pieSliceAngles.Length; i++) {
            pieSliceAngles[i] = pieSliceAngles[i - 1] + degreeOfEachPieSlice;
        }

        // setup angles for raycasts
        if (numRaycast <= 1) {
            rayAngleStart = 90;
            return;
        }

        rayAngleStart = 90 - (coneDegree / 2);
        rayAngle = coneDegree / Mathf.Max((numRaycast - 1), 1);


    }

    // Update is called once per frame
    void Update() {

        if (died)
            return;

        // change agent color based on the food level
        transform.GetComponent<Renderer>().material.color = new Color((100.0f - foodLevel) / 100, foodLevel / 100.0f, 0);

        // food level decrease over time
        if (time > 1) {
            foodLevel -= foodLevelDPS;
            time--;
        }
        time += Time.deltaTime;
        lifeTime += Time.deltaTime;


        // Raycast (range sensor) (wall detector)
        RaycastHit[] hits = new RaycastHit[numRaycast];
        for (int i = 0; i < numRaycast; i++) {

            rays[i] = new Vector3(Mathf.Cos(((rayAngle * i) + rayAngleStart) * Mathf.Deg2Rad),
                                  0,
                                  Mathf.Sin(((rayAngle * i) + rayAngleStart) * Mathf.Deg2Rad));
            rays[i] = rays[i].normalized;
            rays[i] = transform.TransformDirection(rays[i]);
            Debug.DrawRay(transform.position, rays[i] * rayRange, Color.red);
            Physics.Raycast(transform.position, rays[i], out hits[i], rayRange);

            // hit the wall, set input based on distance
            if (hits[i].distance > 0) {
                float tempDist = hits[i].distance;

                if (hits[i].transform.gameObject.tag == "Wall") {
                    // high activation when closer to the wall
                    inputArray[i] = (rayRange - hits[i].distance) / rayRange;
                    Debug.Log("ID#: " + ID + " Ray#: " + i + " Tag: " + hits[i].transform.gameObject.tag);
                }
            }
            // no hit? set everything related to zero
            else {
                inputArray[i] = 0;
            }
        }

        // food detector and agent detector
        // get array of object within radius
        Collider[] colliders = Physics.OverlapSphere(transform.position, sliceRange);
        int offset = 0;
        // for each detected object, determine what pie slice it belong to and update distance
        foreach (Collider c in colliders) {
            if (c.gameObject == this.gameObject)
                continue;

            // check if object is what we want
            if (c.gameObject.tag == "Food" || c.gameObject.tag == "Agent") {

                if (c.gameObject.tag == "Agent")
                    offset = numPieSlice;

                Vector3 target = c.transform.position - transform.position;
                float angle = Angle(target);

                // determine which pie slice the object belong
                for (int i = 0; i < numPieSlice; i++) {
                    if (angle >= pieSliceAngles[i] && angle < pieSliceAngles[i + 1]) {
                        float input = (sliceRange - target.magnitude) / sliceRange;

                        // is this the closest object in this pie slice
                        if (input > inputArray[numRaycast + i]) {
                            inputArray[numRaycast + i + offset] = input;
                        }
                        //Debug.Log("Angle: " + angle + ", Between " + pieSliceAngles[i] + " and " + pieSliceAngles[i+1]);
                    }
                }
            }
        }

        inputArray[numRaycast + 2 * numPieSlice] = (foodMaxLevel - foodLevel) / foodMaxLevel;
        RaycastHit hit;
        Physics.Raycast(transform.position, transform.forward, out hit, rayRange);
        if (hit.distance > 0) {
            if (hit.transform.gameObject.tag == "Agent") {
                Agent agentScript = hit.transform.gameObject.GetComponent<Agent>();
                inputArray[numRaycast + 2 * numPieSlice + 1] = (foodMaxLevel - agentScript.foodLevel) / foodMaxLevel;
                if (outputArray[1] > 0)
                    GiveFood(agentScript);
            }
        }

        // no need to reset output array since
        // either a function call will get the output from ANN, or
        // outputArray will be update by ANN from somewhere else


        // perform output behavior
        if (outputArray[0] > 0)
            Turn(outputArray[0]);
        if (outputArray[1] > 0)
            MoveFB(outputArray[1]);



        // save input to file
        WriteNNInput();
        // get output from file
        //ReadNNOutput();



        if (foodLevel < 0)
            died = true;
    }


    void Turn(float directionSpeed) {

        directionSpeed *= rotateSpeed;

        transform.Rotate(0, directionSpeed * Time.deltaTime, 0);
    }

    void MoveFB(float value) {
        value -= .5f;
        Move(transform.forward * value);
    }

    void Move(Vector3 directionVector) {
        transform.position += directionVector * moveSpeed * Time.deltaTime;
    }


    void GiveFood() {

    }
    // Agent may die after give away food, (Self-sacrifice)
    void GiveFood(Agent other) {
        float foodGiveAway = Mathf.Max(Mathf.Min(10.0f, foodLevel), 0.0f);

        // if agent ever five away food, we really want to know
        // especially if it is a self sacrifice action
        Debug.Log("Agent " + ID + " just give a way " + foodGiveAway + " food to Agent " + other.ID);
        if (foodGiveAway < 10)
            Debug.Log("Agent " + ID + " just sacrifice its life for Agent " + other.ID);

        other.foodLevel = Mathf.Min(foodMaxLevel, other.foodLevel + foodGiveAway);
        foodLevel -= foodGiveAway;
        foodGaveAway += foodGiveAway;
    }



    void OnTriggerEnter(Collider other) {
        if (other.gameObject.tag == "Food") {
            foodLevel = Mathf.Min(foodLevel + 10, foodMaxLevel);
            Destroy(other.gameObject);
        }
    }


    float Angle(Vector3 target) {

        // target is on the right, hence negative
        if (Vector3.Angle(transform.right, target) < 90) {
            return -Vector3.Angle(transform.forward, target);
        }
        // target is on the left, hence positive
        else {
            return Vector3.Angle(transform.forward, target);
        }
    }


    void ReadNNOutput() {
        string path = NNOutputFileName + "_" + ID;

        if (!File.Exists(path))
            return;

        StreamReader reader = new StreamReader(File.OpenRead(path));

        string line = reader.ReadLine();
        string[] values = line.Split(',');
        for (int output = 0; output < outputArray.Length; output++) {
            outputArray[output] = int.Parse(values[output]);
        }
        //print(outputArray);
    }

    void WriteNNInput() {
        string path = NNInputFileName + "_" + ID;
        //print(path);
        StreamWriter file = new StreamWriter(path);

        string lines = "";
        foreach (float input in inputArray) {
            lines += input + ",";
        }
        lines += "\n";

        file.WriteLine(lines);

        file.Close();
    }


}
