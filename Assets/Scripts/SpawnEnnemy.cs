using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnEnnemy : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}

    public Transform instance;
    private int count = 1900;

    void OnTriggerEnter(Collider other)

    {
        if (other.tag == "Player" && count > 2000)
        {
            Instantiate(instance, transform.position + new Vector3(10, 0, 10), transform.rotation);
            count = 0;
        }


    }

    // Update is called once per frame
    void Update () {
        count++;
	}
}
