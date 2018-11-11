// -= MapNav 2 =-
// www.plyoung.com
// by Leslie Young
// ====================================================================================================================

using UnityEngine;
using UnityEditor;
using System.Collections;

namespace MapNavKit
{
	[CustomEditor(typeof(MapNavHexa))]
	public class MapNavHexaInspector : MapNavBaseInspector
	{
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
}