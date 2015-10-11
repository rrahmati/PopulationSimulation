using UnityEngine;
using System.Collections;
using System.IO;

public class NeuroEvolution : MonoBehaviour {

    private string sceneStateFileName = "rtNEAT.1.0.2\\sceneState";

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        writeSceneState();
	}

    void writeSceneState()
    {
        string lines = "";
        File.WriteAllText(sceneStateFileName, lines);
    }
}
