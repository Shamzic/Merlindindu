
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using MapNavKit;

/// <summary>
/// This is a simple OnGUI script to show some runtime controls in the sample scene(s)
/// </summary>
public class SampleGUI : MonoBehaviour 
{
	public static SampleGUI Instance;

	public string distance { get; set; }
	public string selRadius { get; set; }
	public bool markInvalids { get; set; }
	public bool neighbourRing { get; set; }

	// ------------------------------------------------------------------------------------------------------------

	private MapNavBase map;
	private Rect rect = new Rect(0, 0, 270, 500);

	private string mapSize;
	private string minHeight;
	private string maxHeight;
	private bool randomHeight = true;

	// ------------------------------------------------------------------------------------------------------------

	protected void Awake()
	{
		Instance = this;
		distance = "0";
		selRadius = "1";
		markInvalids = false;
		neighbourRing = false;
	}

	protected void Start()
	{
		map = FindObjectOfType<MapNavBase>();
		mapSize = map.mapHorizontalSize.ToString();
		minHeight = map.minNodeHeight.ToString();
		maxHeight = map.maxNodeHeight.ToString();
	}
	
	protected void OnGUI()
	{
		rect = GUILayout.Window(0, rect, Window, GUIContent.none);
	}

	private void Window(int id)
	{
		mapSize = TextField("Size", mapSize);
		GUILayout.Label("Note: Too big a map might be slow or crash as the placed tile objects are not combined in this example.");
		minHeight = TextField("Min Height", minHeight);
		maxHeight = TextField("Max Height", maxHeight);
		randomHeight = GUILayout.Toggle(randomHeight, " Random Height");
		GUILayout.Space(10);

		if (GUILayout.Button("Create Map"))
		{
			int.TryParse(mapSize, out map.mapHorizontalSize);
			int.TryParse(minHeight, out map.minNodeHeight);
			int.TryParse(maxHeight, out map.maxNodeHeight);
			map.mapVerticalSize = map.mapHorizontalSize;
			map.nodeHeightOpt = randomHeight ? MapNavBase.NodeHeightOpt.Random : MapNavBase.NodeHeightOpt.Flat;
			map.CreateGrid();
		}

		if (GUILayout.Button("Raise Tiles")) map.ChangeGridHeight(1);
		if (GUILayout.Button("Lower Tiles")) map.ChangeGridHeight(-1);
		if (GUILayout.Button("Smooth Tiles")) map.SmoothOut();

		GUILayout.Space(10);
		GUILayout.Label("Distance to alt-tile: " + distance);

		GUILayout.Space(10);
		selRadius = TextField("Neighbours", selRadius);
		neighbourRing = GUILayout.Toggle(neighbourRing, " as ring");
		markInvalids = GUILayout.Toggle(markInvalids, "Toggle Invalid Tile", GUI.skin.button);

		GUILayout.Space(10);
		GUILayout.Label("Left-click to mark node");
		GUILayout.Label("Right-click to mark alternate");
		GUILayout.Label("Left/Right Arrow to rotate cam");
		
		GUILayout.Space(10);
		if (GUILayout.Button("Back to Menu")) SceneManager.LoadScene("00_menu");

		GUI.DragWindow();
	}

	private string TextField(string label, string val)
	{
		GUILayout.BeginHorizontal();
		{
			GUILayout.Label(label, GUILayout.Width(100));
			val = GUILayout.TextField(val);
		}
		GUILayout.EndHorizontal();
		return val;
	}

	// ------------------------------------------------------------------------------------------------------------
}
