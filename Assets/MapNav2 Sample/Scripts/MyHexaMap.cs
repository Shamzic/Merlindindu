
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MapNavKit;

/// <summary> Used in Sample 1 </summary>
public class MyHexaMap : MapNavHexa
{
	public GameObject tileFlatFab;		// for flat-top hexa grid
	public GameObject tilePointyFab;	// for pointy-top hexa grid
	public GameObject pathMarkerFab;	// used when marking out calculated path

	// ------------------------------------------------------------------------------------------------------------

	private MapNavNode selectedNode = null;		// keeps track of the selected node
	private MapNavNode altSelectedNode = null;	// keeps track of the second selected node
	private List<GameObject> markedObjects = new List<GameObject>();	// keeps track of nodes that where "marked"
	private List<MapNavNode> invalidNodes = new List<MapNavNode>();		// helper that tracks the nodes marked as "invalid" at runtime

	// ------------------------------------------------------------------------------------------------------------

	protected void Start()
	{
		FocusCamera();
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
			selectedNode = null;
			altSelectedNode = null;
			invalidNodes.Clear();
			ClearMarkedNodes();
			GeneratePath();

			for (int i = parent.childCount - 1; i >= 0; i--)
			{
				if (Application.isPlaying)
				{	// was called at runtime
					Object.Destroy(parent.GetChild(i).gameObject);
				}
				else
				{	// was called from editor
					Object.DestroyImmediate(parent.GetChild(i).gameObject);
				}
			}

			// Place tiles according to the generated grid
			for (int idx = 0; idx < grid.Length; idx++)
			{
				// make sure it is a valid node before placing tile here
				if (false == grid[idx].isValid) continue;

				// create a new tile
				GameObject go = (GameObject)Instantiate(gridOrientation == GridOrientation.FlatTop ? tileFlatFab : tilePointyFab);
				go.name = "Tile " + idx.ToString();
				go.transform.position = grid[idx].position;
				go.transform.parent = parent;
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

		// focus the camera on the center tile
		if (Application.isPlaying) FocusCamera();
	}

	private void FocusCamera()
	{
		Vector3 p = Vector3.zero;
		int x = (int)(mapHorizontalSize / 2);
		int y = (int)(mapVerticalSize / 2);
		MapNavNode n = NodeAt<MapNavNode>(x, y);
		if (n != null) p = n.position;
		Camera.main.GetComponent<CameraController>().Refocus(p);
	}

	// ------------------------------------------------------------------------------------------------------------

	public void UpdateNeighbours()
	{
		UpdateMarked();
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
				// should be a tile since I have nothing else in scene that has a collider
				// I remove the "Tile " bit from name to get the tile number (node index)
				GameObject go = hit.collider.gameObject;
				string tileIndexName = go.name.Replace("Tile ", "");
				int idx = -1;
				if (int.TryParse(tileIndexName, out idx))
				{
					ClearMarkedNodes();

					if (SampleGUI.Instance.markInvalids)
					{
						if (invalidNodes.Contains(grid[idx]))
						{
							invalidNodes.Remove(grid[idx]);
							go.GetComponent<Renderer>().material.color = Color.white;
						}
						else
						{
							invalidNodes.Add(grid[idx]);
							go.GetComponent<Renderer>().material.color = Color.black;
						}
					}
					else
					{
						if (false == invalidNodes.Contains(grid[idx]))
						{
							if (Input.GetMouseButtonDown(1))
							{
								if (grid[idx] != selectedNode)
								{
									altSelectedNode = grid[idx];
								}
							}
							else
							{
								if (grid[idx] != altSelectedNode)
								{
									selectedNode = grid[idx];
								}
							}
						}
					}

					UpdateMarked();
					GeneratePath();
				}
			}
		}
	}

	private void ClearMarkedNodes()
	{
		for (int i = 0; i < markedObjects.Count; i++)
		{
			markedObjects[i].GetComponent<Renderer>().material.color = Color.white;
		}

		markedObjects.Clear();
	}

	private void UpdateMarked()
	{
		ClearMarkedNodes();

		// update marked neighbours
		MarkNeighbours();

		// "mark" the selected node(s)
		if (selectedNode != null)
		{
			GameObject go = transform.Find("Tile " + selectedNode.idx.ToString()).gameObject;
			go.GetComponent<Renderer>().material.color = Color.green;
			markedObjects.Add(go);
		}

		// "mark" the selected node
		if (altSelectedNode != null)
		{
			GameObject go = transform.Find("Tile " + altSelectedNode.idx.ToString()).gameObject;
			go.GetComponent<Renderer>().material.color = Color.yellow;
			markedObjects.Add(go);
		}

		// calculate the distance between the two selected nodes to show on UI
		if (selectedNode != null && altSelectedNode != null)
		{
			SampleGUI.Instance.distance = Distance(selectedNode.idx, altSelectedNode.idx).ToString();
		}
		else SampleGUI.Instance.distance = "0";
	}

	private void MarkNeighbours()
	{
		if (selectedNode == null) return;
		int radius = 0;
		if (int.TryParse(SampleGUI.Instance.selRadius, out radius))
		{
			if (radius > 0)
			{
				List<MapNavNode> nodes;
				if (SampleGUI.Instance.neighbourRing)
				{
					// want to show the neighbour tiles as a ring with the entered
					// number meaning how far from selected node
					nodes = NodesRing<MapNavNode>(selectedNode, radius + 1, 1, OnValidateNode);
				}

				else
				{
					// want to show neighbours within a certain radius of the selected node
					//nodes = NodesAround(selectedNode, radius, false, OnValidateNode);

					// use this one to see how to select "valid move tiles"
					nodes = NodesAround<MapNavNode>(selectedNode, radius, OnNodeCostCallback);
				}

				if (nodes != null)
				{
					for (int i = 0; i < nodes.Count; i++)
					{
						GameObject go = transform.Find("Tile " + nodes[i].idx.ToString()).gameObject;
						go.GetComponent<Renderer>().material.color = Color.cyan;
						markedObjects.Add(go);
					}
				}
			}
		}
	}

	// this is called by NodesAround when wanting to know if the specified tile is valid
	// in the example the user can mark certain nodes "invalid" so I will use that here
	// in a game this could represent a node that is occupied by a unit when you want
	// to find out which nodes a selected unit may move to
	private bool OnValidateNode(MapNavNode node)
	{
		return (false == invalidNodes.Contains(node));
	}

	// shows path from green to yellow marked node
	private void GeneratePath()
	{
		// remove old path markers
		GameObject go = GameObject.Find("PathMarkers");
		if (go == null) go = new GameObject("PathMarkers");
		for (int i = go.transform.childCount - 1; i >= 0; i--) Destroy(go.transform.GetChild(i).gameObject);

		// get path
		List<MapNavNode> path = Path<MapNavNode>(selectedNode, altSelectedNode, OnNodeCostCallback);
		for (int i = 0; i < path.Count; i++)
		{
			GameObject marker = (GameObject)Instantiate(pathMarkerFab, path[i].position, Quaternion.identity);
			marker.transform.parent = go.transform;
		}
	}

	// this is called by the Path function to find out what the cost is to move from one
	// node to the neighbouring node, or if the move is even allowed
	private float OnNodeCostCallback(MapNavNode fromNode, MapNavNode toNode)
	{
		return (invalidNodes.Contains(toNode) ? 0f : 1f);
	}

	// ------------------------------------------------------------------------------------------------------------
}
