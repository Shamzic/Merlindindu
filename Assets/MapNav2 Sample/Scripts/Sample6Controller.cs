
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using MapNavKit;

// Note that I derive this controller from controller 4 since they do basically the same thing.
// Sample 6 takes the node link data into account to determine if there are walls in the way
public class Sample6Controller : Sample4Controller 
{

	/// <summary>
	/// I've kept most of what Sample4Controller.OnNodeCostCallback did (see its comments)
	/// The big difference is that the Node Link Data is looked at to determine if there
	/// is a wall in the way. Basically any link data that exist and is not set to 0
	/// will be considered a wall. I've also added a door which when click, will 
	/// set its associated node data to 0. See Sample6Door.cs
	/// </summary>
	protected override float OnNodeCostCallback(MapNavNode fromNode, MapNavNode toNode)
	{
		// get the link data between fromNode and toNode
		MapNavNodeLink ld = fromNode.GetLinkData(toNode.idx);

		// if "ld" is null then there is no link data for them
		if (ld != null)
		{
			// if the data is not 0 then there is a wall or door in way
			// you can decide what the data (numbers) means in your own game
			// but in this simple example any non-zero value blocks movement
			if (ld.data != 0) return 0f; // return 0, meaning that target node is invalid
		}

		// the rest is same as Sample 4
		return base.OnNodeCostCallback(fromNode, toNode);
	}

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
		GUILayout.Label("This is similar to sample 4 but uses link node data to detect walls.");

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
