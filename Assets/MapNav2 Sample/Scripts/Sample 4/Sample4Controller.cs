
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using MapNavKit;

public class Sample4Controller : MonoBehaviour 
{
	// ------------------------------------------------------------------------------------------------------------
	#region properties

	public GameObject unitFab; // prefab of unit

	#endregion
	// ------------------------------------------------------------------------------------------------------------
	#region runtime

	// these are set as protected and not private since Sample5Controller derives from Sample4Controller and might need them

	protected MapNavBase map;								// reference to the mapNav grid
	protected LayerMask clickMask = (1 << 8 | 1 << 9);	// in this sample layer 8 = tiles collider and layer 9 = unit's collider
	protected List<Sample4Unit> units = new List<Sample4Unit>();
	protected Sample4Unit activeUnit = null;				// the currently selected unit
	protected List<GameObject> moveMarkers = new List<GameObject>();
	protected List<Sample4Tile> validMoveNodes = new List<Sample4Tile>();
	protected Rect winRect = new Rect(0, 0, 270, 500);	// used by GUI
	protected bool unitMoving = false;					// set when a unit is moving and no input should be accepted

	#endregion
	// ------------------------------------------------------------------------------------------------------------
	#region start

	protected void Start()
	{
		// get reference to the mapnav map
		map = GameObject.Find("Map").GetComponent<Sample4Map>();

		// also used in sample4b
		if (map == null) map = GameObject.Find("Map").GetComponent<Sample4bMap>();

		// in this sample I will simply spawn 10 units on random locations of the map
		for (int i = 0; i < 10; i++)
		{
			Sample4Tile selectedTile = null;

			// find a tile to place the unit on. I will randomly select one.
			// but I need to check if there is not already a unit on the 
			// selected tile before accepting it.
			while (true)
			{
				int idx = Random.Range(0, map.grid.Length);

				// make sure this is a valid node
				if (map.ValidIDX(idx))
				{
					// now check if there is not already a unit on it
					selectedTile = map.grid[idx] as Sample4Tile;
					if (selectedTile.unit == null)
					{
						// no unit on it, lets use it
						break;
					}
				}
			}

			// create the unit and place it on the tile
			GameObject go = (GameObject)Instantiate(unitFab);
			go.transform.position = selectedTile.position;
			go.layer = 9; // in this sample Units must be in layer 9

			// be sure to tell the tile that this Unit is on it
			Sample4Unit unit = go.GetComponent<Sample4Unit>();
			unit.Resetunit(); // reset its moves now too
			unit.LinkWithTile(selectedTile);
			units.Add(unit); // keep a list of all units for quick access
		}
	}
	
	#endregion
	// ------------------------------------------------------------------------------------------------------------
	#region update/ input

	protected void Update()
	{
		if (Input.GetMouseButtonDown(0) && GUIUtility.hotControl == 0 && unitMoving == false)
		{
			// Check what the player clicked on. I've set Tiles to be on Layer 8 and Unit on Layer 9.
			// Each tile has its own collider attached but I could have used a big invisible collider
			// that stretches the whole map as a catch all for clicks that did not fall on a unit.
			// The only reason the tiles each have a collider is because they are used in other
			// samples too that requires it.

			RaycastHit hit;
			Ray r = Camera.main.ScreenPointToRay(Input.mousePosition);
			if (Physics.Raycast(r, out hit, Mathf.Infinity, clickMask))
			{
				if (hit.transform.gameObject.layer == 8)
				{
					// a tile was clicked
					Sample4Tile n = map.NodeAtWorldPosition<Sample4Tile>(hit.point);
					//Sample4Tile n = (Sample4Tile)map.NodeAtWorldPosition(hit.point);
					if (n != null) 
					{
						Debug.Log("Clicked on: " + n.tileName);

						// check if a tile that the active unit may move to
						if (activeUnit != null && validMoveNodes.Contains(n))
						{
							// get the path
							List<MapNavNode> path = map.Path<MapNavNode>(activeUnit.tile, n, OnNodeCostCallback);
							if (path != null)
							{
								unitMoving = true; // need to wait while unit is moving
								ClearMoveMarkers();
								activeUnit.Move(path, OnUnitMoveComplete);
							}
						}
					}
				}
				else
				{
					// a unit was clicked. first clear the previously selected unit
					if (activeUnit != null)
					{
						activeUnit.UnitDeSelected();
						ClearMoveMarkers();
					}

					activeUnit = hit.transform.GetComponent<Sample4Unit>();
					if (activeUnit != null)
					{
						// tell the unit it was selected so that it can change colour
						activeUnit.UnitSelected();

						// show the player the area that the unit may move to
						UpdateMoveMarkers();
					}
				}
			}

		}
	}

	protected virtual void ClearMoveMarkers()
	{
		for (int i = 0; i < moveMarkers.Count; i++)
		{
			moveMarkers[i].GetComponent<Renderer>().material.color = Color.white;
		}

		validMoveNodes.Clear();
		moveMarkers.Clear();
	}

	protected virtual void UpdateMoveMarkers()
	{
		// do not bother to do anything if the unit got no moves left
		if (activeUnit.movesLeft == 0) return;

		// in this example I will simply change the colour of the whole tile.
		// You could use textured planes or projectors for your markers.

		// first find out which nodes the unit could move to and keep that 
		// list around to later use when checking what tile the player click
		validMoveNodes = map.NodesAround<Sample4Tile>(activeUnit.tile, activeUnit.movesLeft, OnNodeCostCallback);

		// now show the nodes that can be moved to
		for (int i = 0; i < validMoveNodes.Count; i++)
		{
			// you could simply update the position of planes/ projectors but I want to update the 
			// actual tile object; luckily I saved a reference to it.
			GameObject go = validMoveNodes[i].tileObj;
			go.GetComponent<Renderer>().material.color = Color.green;
			moveMarkers.Add(go);
		}
	}

	#endregion
	// ------------------------------------------------------------------------------------------------------------
	#region callbacks

	/// <summary>
	/// This will be called by map.NodesAround to find out if the target node is valid
	/// and how much it will cost to move to that node. In my case I do not have extra
	/// cost to move from tile to tile, so I will simply return 1. The only condition
	/// is that units may not move to tile that are too high/ low; in that case I 
	/// need to return 0.
	/// </summary>
	protected virtual float OnNodeCostCallback(MapNavNode fromNode, MapNavNode toNode)
	{
		if ((toNode as Sample4Tile).unit != null) return 0f; // there is a unit on target node
		int heightDiff = Mathf.Abs(fromNode.h - toNode.h);
		if (heightDiff >= 2) return 0f; // too height
		return 1f;
	}

	/// <summary>
	/// Called by the unit when it is done moving
	/// </summary>
	protected void OnUnitMoveComplete()
	{
		unitMoving = false;
		UpdateMoveMarkers();
	}

	#endregion
	// ------------------------------------------------------------------------------------------------------------
	#region GUI

	protected virtual void OnGUI()
	{
		// just a simple Window to show some info about the sample and
		// a button to go back to the main menu of samples
		winRect = GUILayout.Window(0, winRect, Window, GUIContent.none);
	}

	private void Window(int id)
	{
		GUILayout.Label("This sample shows how to move Units around a map. In this case the Units can only move to a Tile that is is not more than 1 unit higher/ lower than its neighbour.");

		GUILayout.Space(10);
		if (GUILayout.Button("Reset Moves"))
		{
			for (int i = 0; i < units.Count; i++) units[i].Resetunit();
		}

		GUILayout.Space(10);
		if (GUILayout.Button("Back to Menu")) SceneManager.LoadScene("00_menu");

		GUI.DragWindow();
	}

	#endregion
	// ------------------------------------------------------------------------------------------------------------
}
