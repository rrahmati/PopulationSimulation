using UnityEngine;
using System.Collections;

public class RandomFoodSpawn : MonoBehaviour {

    public GameObject spawnObject;
    public float timeInterval = 1f;
    public float time = 1f;
    public int foodAmountLimit = 50;
    public float deviationFromOrigin = 40;

    // Use this for initialization
    void Start() {

    }

    // Update is called once per frame
    void Update() {

        if (time > timeInterval) {

            if (GameObject.FindGameObjectsWithTag("Food").Length > foodAmountLimit-1) {
                time -= timeInterval;
                return;
            }

            Vector3 randomVector = transform.position + new Vector3(Random.Range(-deviationFromOrigin, deviationFromOrigin), 0, Random.Range(-deviationFromOrigin, deviationFromOrigin));
            randomVector.y = 1;
            // Check the spawn location, if nothing there, then spawn the object
            if (!Physics.CheckSphere(randomVector, 0.5f)) {
                Instantiate(spawnObject, randomVector, transform.rotation);
                time -= timeInterval;
            }
        }

        time += Time.deltaTime;

    }
}
