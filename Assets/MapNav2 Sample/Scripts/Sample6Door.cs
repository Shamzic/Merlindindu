
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MapNavKit;

public class Sample6Door : MonoBehaviour 
{
	// a simple script to remove a GameObject from the scene 
	// and change the link data of two nodes

	public MapNavBase map;
	public Vector2 node1;
	public Vector2 node2;

	// this is called when the object is clicked on
	protected void OnMouseUpAsButton()
	{
		gameObject.SetActive(false);

		// find the two nodes
		MapNavNode n1 = map.NodeAt<MapNavNode>((int)node1.x, (int)node1.y);
		MapNavNode n2 = map.NodeAt<MapNavNode>((int)node2.x, (int)node2.y);

		// tell the node to set link data with node 2 to zero(0)
		n1.UpdateOrCreateLinkData(n2.idx, 0, map);
	}

	// ------------------------------------------------------------------------------------------------------------
}
