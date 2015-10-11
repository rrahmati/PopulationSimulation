﻿using UnityEngine;
using System.Collections;
using System.IO;

public class NeuroEvolution : MonoBehaviour {

    private string NNInputFileName = "rtNEAT.1.0.2\\NNinput";
    private string NNOutputFileName = "rtNEAT.1.0.2\\NNoutput";

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        writeNNInput();
	}

    void readNNOutput()
    {
        var reader = new StreamReader(File.OpenRead(NNOutputFileName));
        var line = reader.ReadLine();
        var values = line.Split(',');
        int[] outputArray = GameObject.Find("Agent").GetComponent<Agent>().outputArray;
        for (int output = 0; output < outputArray.Length; output++)
        {
            outputArray[output] = int.Parse(values[output]);
        }
    }
    void writeNNInput()
    {
        string lines = "";
        float[] inputArray = GameObject.Find("Agent").GetComponent<Agent>().inputArray;
        foreach (float input in inputArray)
        {
            lines += input + ",";
        }
        lines += "\n";
        File.WriteAllText(NNInputFileName, lines);
    }
}
