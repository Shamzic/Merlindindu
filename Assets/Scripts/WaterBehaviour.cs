using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterBehaviour : MonoBehaviour
{

    public float waterLvl;
    public float montainLvl;

    void Update()
    {

        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player.transform.position.y <= waterLvl)

        {
            GameObject closerSpawn = null;
            float distanceMin = 10000;
            GameObject[] respawnPoints = GameObject.FindGameObjectsWithTag("respawn");

            for (int i = 0; i < respawnPoints.Length; i++)
            {
                float distanceSpawn = (respawnPoints[i].transform.position - player.transform.position).magnitude;
                if (distanceMin >= distanceSpawn)
                {
                    distanceMin = distanceSpawn;
                    closerSpawn = respawnPoints[i];
                }
            }

            player.transform.position = closerSpawn.transform.position;

        }

        if (player.transform.position.y >= montainLvl)
        {
            player.transform.position = new Vector3(player.transform.position.x,montainLvl, player.transform.position.z);
        }
    }
}

    

