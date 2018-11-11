
using UnityEngine;
using UnityEditor;
using System.Collections;
using MapNavKit;

[CustomEditor(typeof(MyTerrainMap))]
public class MyTerrainMapInspector : MapNavHexaInspector
{
	// I need to do this so that all the tools of MapNav shows
	// up in Inspector since it has a custom Inspector
	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();
	}

	protected override void OnSceneGUI()
	{
		base.OnSceneGUI();
	}

	// ------------------------------------------------------------------------------------------------------------
}
