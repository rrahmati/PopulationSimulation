using UnityEngine;
using System.Collections;

public class FoodSpawn : MonoBehaviour {

	public GameObject spawnObject;
	private float timeInterval = .1f;
	private float time =1f;




	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
		if (time > timeInterval) {
			time -= timeInterval;

			// Check the spawn location, if nothing there, then spawn the object
			if(!Physics.CheckSphere(transform.position, 0.5f)){
				Instantiate(spawnObject, transform.position, transform.rotation);
			}
		}

		time += Time.deltaTime;

	}
}
