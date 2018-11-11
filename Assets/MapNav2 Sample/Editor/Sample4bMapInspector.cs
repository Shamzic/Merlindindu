
using UnityEngine;
using UnityEditor;
using System.Collections;
using MapNavKit;

[CustomEditor(typeof(Sample4bMap))]
public class Sample4bMapInspector : MapNavSqaureInspector
{
	// I need to do this so that all the tools of MapNav shows
	// up in Inspector since it has a custom Inspector

	protected override void OnEnable()
	{
		// Inform MapNav Inspector to use this custom node type
		NodeType = typeof(Sample4Tile);

		// Be sure to call the base OnEnable()
		base.OnEnable();
	}

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
