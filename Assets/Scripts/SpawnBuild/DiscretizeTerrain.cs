using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiscretizeTerrain : MonoBehaviour {
    public GameObject landElement0;
    public GameObject landElement1;
    public GameObject landElement2;

    public int sizeOflandElement = 3;
    public int landWidth = 10;
    public int landHeight = 10;

    public Vector3 origin;

    public void buildTerrain()
    {
        for (int i = 0; i < landWidth; i++)
        {
            for (int j = 0; j < landHeight; j++)
            {
                //we determine the position of the cell
                Vector3 elementPosition = new Vector3(i * sizeOflandElement+origin[0], origin[1], j * sizeOflandElement + origin[2]);
                float randomNumber = Random.Range(0, 3.0f);

                if (randomNumber>2)Instantiate(landElement2, elementPosition, Quaternion.identity);
                else if (randomNumber > 1) Instantiate(landElement1, elementPosition, Quaternion.identity);
                else  Instantiate(landElement0, elementPosition, Quaternion.identity);


            }
        }
    }
    

    // Use this for initialization
    void Start()
    {
        buildTerrain();



    }

    // Update is called once per frame
    void Update()
    {

    }
}

