using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public class All3DObjects : MonoBehaviour {
    [Header("Liste des objects 3D utilisés dans le jeu")]
    [SerializeField]
    protected StringObject all3DObjects;

    [SerializeField]
    protected GameObject currentObject=null;
    [SerializeField]
    protected GameObject nextObject=null;

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public StringObject getAll3DObjects()
    {
        return all3DObjects;
    }

    public GameObject getCurrentObject()
    {
        return currentObject;
    }

    public GameObject getNextObject()
    {
        return nextObject;
    }
}
