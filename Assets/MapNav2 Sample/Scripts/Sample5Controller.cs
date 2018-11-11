
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using MapNavKit;

// Note that I derive this controller from controller 4 since they do basically the same thing.
// Sample 5 changes from showing all valid move nodes to showing a border.
public class Sample5Controller : Sample4Controller 
{
	// ------------------------------------------------------------------------------------------------------------
	#region properties

	public GameObject borderFab;	// the border prefab

	// these values will help me to rotate the border object correctly
	// have a look at the Prefabs/Sample5Boder.prefab
	// you will note that it is a place with just one hexa side drawn so, placing this plane over
	// a tile and rotating it correctly will give the illusion of a border on a side of the tile
	private static readonly float[] BorderRotations = { 60f, 120f, 180f, 240f, 300f, 0f };

	#endregion
	// ------------------------------------------------------------------------------------------------------------
	#region runtime

	private Transform bordersContainer; // all the border objects will be placed under this game object. I'll create the object in Awake()

	#endregion
	// ------------------------------------------------------------------------------------------------------------
	#region start

	protected void Awake()
	{
		GameObject go = new GameObject("Borders");
		bordersContainer = go.transform;
	}
	
	#endregion
	// ------------------------------------------------------------------------------------------------------------
	#region update/ input

	protected override void ClearMoveMarkers()
	{
		// hide the borders - since I placed them in bordersContainer
		// I can simply destroy all its child objects
		for (int i = bordersContainer.childCount - 1; i >= 0; i--)
		{
			Destroy(bordersContainer.GetChild(i).gameObject);
		}

		validMoveNodes.Clear();
		moveMarkers.Clear();
	}

	protected override void UpdateMoveMarkers()
	{
		// do not bother to do anything if the unit got no moves left
		if (activeUnit.movesLeft == 0) return;

		// first find out which nodes the unit could move to and keep that 
		// list around to later use when checking what tile the player click
		validMoveNodes = map.NodesAround<Sample4Tile>(activeUnit.tile, activeUnit.movesLeft, OnNodeCostCallback);

		// In this example I will show a border to indicate the valid movement area
		// I need to pass a list of the valid node to GetBroderNodes() and it will give back
		// a list of those nodes that are neighbouring invalid nodes
		
		// I suggest you have a look at the GetBorderNodes() function and simply copy its
		// code and create your own variation that can instantiate the borders in the same loop
		// that the border node is discovered since the code below will basically copy the inner
		// loop of the function. (for a little optimization of this)
		List<Sample4Tile> borderNodes = map.GetBorderNodes<Sample4Tile>(validMoveNodes, CheckIfValidNode);
		
		// now create the border objects. might be better to use a object cache solution
		// with this or you might even want to mesh combine the result if needed
		for (int i = 0; i < borderNodes.Count; i++)
		{
			// I run through all the border nodes and then check the neighbours for each to find out
			// in what direction the placed border object should be rotated to create the correct border
			// Each hexa node has 6 neighbours. An invalid neighbour will be marked as null
			List<Sample4Tile> neighbours = map.NodesAround<Sample4Tile>(borderNodes[i], true, false, CheckIfValidNode);
			for (int j = 0; j < neighbours.Count; j++)
			{
				// each hexa node has 6 neighbours.
				// an invalid neighbour will be null or not in the list of valid nodes.
				// also check that the node is not the one the active unit is on that
				// a border is not drawn around the selected unit's tile
				if (neighbours[j] != activeUnit.tile && (neighbours[j] == null || false == validMoveNodes.Contains(neighbours[j])))
				{
					GameObject go = (GameObject)Instantiate(borderFab);
					go.transform.position = borderNodes[i].position + new Vector3(0f, 0.1f, 0f);
					go.transform.rotation = Quaternion.Euler(0f, BorderRotations[j], 0f);
					go.transform.parent = bordersContainer;
				}
			}
		}
	}

	private bool CheckIfValidNode(MapNavNode toNode)
	{
		Sample4Tile n = toNode as Sample4Tile;

		// this will help prevent a border being drawn around the active unit's tile
		// also see the IF(neighbours[j] != activeUnit.tile ...) that must be used above
		if (n == activeUnit.tile) return true;

		// can't move to a tile that has a unit on it
		if (n.unit != null) return false;

		// finally, return tile's default state
		return toNode.isValid;
	}

	#endregion
	// ------------------------------------------------------------------------------------------------------------
	#region GUI

	protected override void OnGUI()
	{
		// just a simple Window to show some info about the sample and
		// a button to go back to the main menu of samples
		winRect = GUILayout.Window(0, winRect, Window, GUIContent.none);
	}

	private void Window(int id)
	{
		GUILayout.Label("This is similar to sample 4 but rather than showing all valid tiles it shows a border.");

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
