using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// UI for the samples' main menu
/// </summary>
public class MenuGUI : MonoBehaviour 
{

	protected void OnGUI()
	{
		GUILayout.BeginArea(new Rect(Screen.width / 2 - 100, Screen.height / 2 - 150, 200, 300));
		{
			if (GUILayout.Button("Hexa Grid")) SceneManager.LoadScene("01_hexa_tests");
			if (GUILayout.Button("Square Grid")) SceneManager.LoadScene("02_square_test");
			if (GUILayout.Button("Terrain")) SceneManager.LoadScene("03_terrain");
			if (GUILayout.Button("Moving Units")) SceneManager.LoadScene("04_moving_units");
			if (GUILayout.Button("Borders")) SceneManager.LoadScene("05_borders");
			if (GUILayout.Button("Walls and Doors")) SceneManager.LoadScene("06_nodelink_tool");
			if (GUILayout.Button("Move Units Keyboard")) SceneManager.LoadScene("07_move_units");
			
		}
		GUILayout.EndArea();
	}
	

	// ------------------------------------------------------------------------------------------------------------
}
