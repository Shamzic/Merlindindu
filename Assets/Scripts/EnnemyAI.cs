using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnnemyAI : MonoBehaviour {

	// Use this for initialization
	void Start () {

        Player = GameObject.FindGameObjectWithTag("Player");
		
	}

    private GameObject Player;
    public float attackRange;
    public float chaseRange;
    public float distance;
    public float RotSpeed = 3; // the target's rotation speed
    public float MoveSpeed = 1; // the target's moving speed

    // Update is called once per frame
    void Update () {

        distance = (Player.transform.position - transform.position).magnitude;

        if (distance <= chaseRange && distance > attackRange)
        {
            GetComponent<Animator>().SetBool("Walking", true);
            Vector3 direction = Player.transform.position - transform.position;
            direction.y = 0; // so the target won't rotate in the y-axis

            // rotate the target toward the player
            transform.rotation = Quaternion.Slerp(transform.rotation,
                                                  Quaternion.LookRotation(direction), RotSpeed * Time.deltaTime);
            transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);

            // so when the target rotates toward the player then we can move the target to forward direction,
            // Suppose it Chases the player
            transform.position += transform.forward * MoveSpeed * Time.deltaTime;
        }
        else
        {
            GetComponent<Animator>().SetBool("Walking", false);
        }
        if (distance <= attackRange)
        {
            GetComponent<Animator>().SetBool("Attack", true);

        }
        else GetComponent<Animator>().SetBool("Attack", false);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chaseRange);



    }
}
