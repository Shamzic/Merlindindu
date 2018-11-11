using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using MapNavKit;

/// <summary> Used in Sample 3 </summary>
public class MyTerrainMap : MapNavHexa
{
	public GameObject hexaMarkerFab;					// the projector used to mark the hexa nodes on map

	// ------------------------------------------------------------------------------------------------------------

	private Rect winRect = new Rect(0, 0, 270, 500);	// used by GUI window

	private GameObject[] markers;						// the markers that will show where mouse is over the map

	// ------------------------------------------------------------------------------------------------------------

	protected void Start()
	{
		Camera.main.GetComponent<CameraController>().Refocus(new Vector3(1000, 0, 1000));

		// Create the markers now and hide them. I will need 7 to show
		// the central node and its direct neighbours
		markers = new GameObject[7];
		for (int i = 0; i < 7; i++)
		{
			GameObject go = (GameObject)Instantiate(hexaMarkerFab);
			go.SetActive(false);
			markers[i] = go;
		}
	}

	/// <summary>
	/// I override this callback so that I can respond on the grid being
	/// changed and place/ update the objects related to the grid nodes
	/// </summary>
	public override void OnGridChanged(bool created)
	{
		// I placed this here to show that you do not actually need this :p
		// Only override functions that you need. In this case there is
		// nothing I want to update related to the grid's nodes since in
		// this example I only show projectors as you move the mouse
		// over the terrain and the position of these are updated 
		// in real time.
	}

	// ------------------------------------------------------------------------------------------------------------

	protected void LateUpdate()
	{
		// Update the map markers by checking where the mouse is over the
		// map and calculating which hexa node is around that position

		RaycastHit hit;
		Ray r = Camera.main.ScreenPointToRay(Input.mousePosition);
		if (Physics.Raycast(r, out hit))
		{
			MapNavNode n = NodeAtWorldPosition<MapNavNode>(hit.point);
			ShowMarkersAround(n);
		}

	}

	private void ShowMarkersAround(MapNavNode n)
	{
		HideAllMarkers();
		if (n == null) return;

		List<MapNavNode> nodes = NodesAround<MapNavNode>(n, false, true, null);
		for (int i = 0; i < nodes.Count; i++)
		{
			markers[i].SetActive(true);
			markers[i].transform.position = nodes[i].position + new Vector3(0f, 20f, 0f);
		}
	}

	private void HideAllMarkers()
	{
		for (int i = 0; i < markers.Length; i++)
		{
			markers[i].SetActive(false);
		}
	}

	// ------------------------------------------------------------------------------------------------------------

	protected void OnGUI()
	{
		// just a simple Window to show some info about the sample and
		// a button to go back to the main menu of samples
		winRect = GUILayout.Window(0, winRect, Window, GUIContent.none);
	}

	private void Window(int id)
	{
		GUILayout.Label("This sample's main focus is the tool that is used to position the grid nodes. The runtime side simply updates projectors to visualize the grid as you move the mouse over the terrain.");

		GUILayout.Space(10);
		if (GUILayout.Button("Back to Menu")) SceneManager.LoadScene("00_menu");

		GUI.DragWindow();
	}

	// ------------------------------------------------------------------------------------------------------------
}
