
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using MapNavKit;

// this example shows how to move a unit around via keyboard
public class Sample7Controller : MonoBehaviour 
{
	// ------------------------------------------------------------------------------------------------------------

	public MapNavSquare map;	// reference to the map 
	public GameObject unitFab;	// prefab of the unit/ character that will be moved around

	private Rect winRect = new Rect(0, 0, 270, 500);	// used by GUI
	
	private Transform unitTr;	 // the unit's transform
	private MapNavNode node;	// keep track of which tile the unit is on

	// this will help me determine what tile is up. back, left, or right from the unit
	private static int[,] Neighbours4 = new int[,] { { +1, 0 }, { 0, +1 }, { -1, 0 }, { 0, -1 } };

	// this is a helper so that it is easier to see what my code does. the enum values
	// each relates to an index into the Neighbours4 array above
	private enum Direction
	{
		Right = 0,
		Up = 1,
		Left = 2,
		Down = 3,
	}

	// ------------------------------------------------------------------------------------------------------------
	#region system

	protected void Start()
	{
		// create the unit
		GameObject go = Instantiate(unitFab);
		unitTr = go.transform;

		// position the unit at the middle tile
		int x = map.mapHorizontalSize / 2;
		int y = map.mapVerticalSize / 2;
		node = map.NodeAt<MapNavNode>(x, y);
		unitTr.position = node.position;
	}

	protected void Update()
	{
		// check what key goes down and act on it
		if (Input.GetKeyDown(KeyCode.UpArrow)) MoveUnit((int)Direction.Up);
		else if (Input.GetKeyDown(KeyCode.DownArrow)) MoveUnit((int)Direction.Down);
		else if (Input.GetKeyDown(KeyCode.LeftArrow)) MoveUnit((int)Direction.Left);
		else if (Input.GetKeyDown(KeyCode.RightArrow)) MoveUnit((int)Direction.Right);
	}

	private void MoveUnit(int dir)
	{
		// get the node that is in the specified direction
		// null is returned then there is no valid node there
		// and the unit should not move there

		// the direction to find the node in is from current
		// node + the  value provided by the Neighbours4

		MapNavNode n = map.NodeAt<MapNavNode>(node.q + Neighbours4[dir, 0], node.r + Neighbours4[dir, 1]);
		if (n != null)
		{
			// keep track of the node the unit is on
			node = n;

			// you could of course use some tweening to smoothly move the
			//  unit to the target position but ill just set it directly
			unitTr.position = node.position;
		}
	}

	#endregion
	// ------------------------------------------------------------------------------------------------------------
	#region GUI

	protected void OnGUI()
	{
		// just a simple Window to show some info about the sample and
		// a button to go back to the main menu of samples
		winRect = GUILayout.Window(0, winRect, Window, GUIContent.none);
	}

	private void Window(int id)
	{
		GUILayout.Label("This sample shows how to move a Unit with the keyboard.");

		GUILayout.Space(10);
		if (GUILayout.Button("Back to Menu")) SceneManager.LoadScene("00_menu");

		GUI.DragWindow();
	}

	#endregion
	// ------------------------------------------------------------------------------------------------------------
}
