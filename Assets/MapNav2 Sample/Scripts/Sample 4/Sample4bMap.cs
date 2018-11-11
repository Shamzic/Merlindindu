
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MapNavKit;

/// <summary> Used in Sample 4b </summary>
public class Sample4bMap : MapNavSquare
{
	public GameObject tileFab;			// used when placing tile objects

	private static int tileLayer = 8;   // in this sample tiles use layer 8

	// ------------------------------------------------------------------------------------------------------------


	// ------------------------------------------------------------------------------------------------------------

	protected void Start()
	{
		Vector3 p = Vector3.zero;
		int x = (int)(mapHorizontalSize / 2);
		int y = (int)(mapVerticalSize / 2);
		MapNavNode n = NodeAt<MapNavNode>(x, y);
		if (n != null) p = n.position;
		Camera.main.GetComponent<CameraController>().Refocus(p);
	}

	/// <summary>
	/// I override this callback so that I can respond on the grid being
	/// changed and place/ update the actual tile objects
	/// </summary>
	public override void OnGridChanged(bool created) 
	{		
		// The parent object that will hold all the instantiated tile objects
		Transform parent = gameObject.transform;

		// Remove existing tiles and place new ones if map was (re)created
		// since the number of tiles might be different now
		if (created)
		{
			for (int i = parent.childCount - 1; i >= 0; i--)
			{
				// was called from editor
				Object.DestroyImmediate(parent.GetChild(i).gameObject);
			}

			// Place tiles according to the generated grid
			for (int idx = 0; idx < grid.Length; idx++)
			{
				// make sure it is a valid node before placing tile here
				if (false == grid[idx].isValid) continue;

				// create the visual tile
				GameObject go = (GameObject)Instantiate(tileFab);
				go.name = "Tile " + idx.ToString();
				go.layer = tileLayer; // set the layer since I will need it in Sample4Controller to check what the player clicked on
				go.transform.position = grid[idx].position;
				go.transform.parent = parent;

				// MapNav created the node as the custom Sample4Tile type
				// Now init the tileName property in it
				Sample4Tile node = grid[idx] as Sample4Tile;
				node.tileName = go.name;
				node.tileObj = go;
			}
		}

		// else, simply update the position of existing tiles
		else
		{
			for (int idx = 0; idx < grid.Length; idx++)
			{
				// make sure it is a valid node before processing it
				if (false == grid[idx].isValid) continue;

				// Since I gave the tiles proper names I can easily find them by name
				GameObject go = parent.Find("Tile " + idx.ToString()).gameObject;
				go.transform.position = grid[idx].position;
			}
		}
	}

	// ------------------------------------------------------------------------------------------------------------

	protected void Update()
	{
		if ((Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1)) && GUIUtility.hotControl == 0)
		{
			// check if clicked on a tile and make it selected
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;
			if (Physics.Raycast(ray, out hit))
			{
			}
		}
	}





	// this is called by NodesAround when wanting to know if the specified tile is valid
	// in the example the user can mark certain nodes "invalid" so I will use that here
	// in a game this could represent a node that is occupied by a unit when you want
	// to find out which nodes a selected unit may move to
	private bool OnValidateNode(MapNavNode node)
	{
		return true;
	}

	// this is called by the Path function to find out what the cost is to move from one
	// node to the neighbouring node, or if the move is even allowed
	private float OnNodeCostCallback(MapNavNode fromNode, MapNavNode toNode)
	{
		return 1f;
	}

	// ------------------------------------------------------------------------------------------------------------
}
