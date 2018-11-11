// -= MapNav 2 =-
// www.plyoung.com
// by Leslie Young
// ====================================================================================================================

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

namespace MapNavKit
{
	/// <summary>
	///  misc helpers for the MapNav Inspectors
	/// </summary>
	public class MapNavEdUtil
	{
		// ------------------------------------------------------------------------------------------------------------
		#region Editor GUI

		/// <summary>
		/// Draws a layer mask field
		/// </summary>
		public static int GUILayout_LayerMaskField(string label, int mask)
		{
			// Get layers and layer names. This is best I can do for now since
			// caching the names is no use if I can't detect when they are changed
			List<string> names = new List<string>();
			for (int i = 0; i < 32; i++)
			{
				string nm = LayerMask.LayerToName(i);
				if (nm.Length > 0) names.Add(nm);
				else names.Add("Layer " + i);
			}
			
			// show the mask field
			mask = EditorGUILayout.MaskField(label, mask, names.ToArray());

			return mask;
		}

		/// <summary>
		/// Draws a button that has label to the left
		/// </summary>
		public static bool GUILayout_LabelButton(string label, string button)
		{
			bool res = false;
			EditorGUILayout.BeginHorizontal();
			{
				GUILayout.Label(label, GUILayout.Width(EditorGUIUtility.labelWidth));
				res = GUILayout.Button(button);
			}
			EditorGUILayout.EndHorizontal();
			return res;
		}

		#endregion
		// ------------------------------------------------------------------------------------------------------------
	}
}