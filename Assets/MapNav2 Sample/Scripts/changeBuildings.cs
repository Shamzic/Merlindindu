using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class changeBuildings : MonoBehaviour {

    // Use this for initialization
    void Start () {
        

    }
	
	// Update is called once per frame
	void Update () {
		
	}

    public void changeBuildingToMediumHouse()
    {
        MySquareMap myMap = (MySquareMap)FindObjectOfType(typeof(MySquareMap));
        myMap.changeBuildingToMediumHouse();
    }

    public void changeBuildingToBasicHouse()
    {
        MySquareMap myMap = (MySquareMap)FindObjectOfType(typeof(MySquareMap));
        myMap.changeBuildingToBasicHouse();
    }
}
