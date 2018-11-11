// -= MapNav 2 =-
// www.plyoung.com
// by Leslie Young
// ====================================================================================================================

#pragma warning disable 618

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

namespace MapNavKit
{
	[CustomEditor(typeof(MapNavBase))]
	public class MapNavBaseInspector : Editor
	{
		/// <summary> Set this to a custom type in OnEnable() so that "Generate Grid" 
		/// will create a grid based on your custom node/ tile. type </summary>		
		protected static System.Type NodeType = typeof(MapNavNode);

		protected MapNavBase mapnav;

		private static bool[] tool = new bool[] { false, false };

		private static bool debug_ShowLabels = true;

		private static GUIStyle Toolbox_Style;

		private enum heightTool_extraPrecision_type { Off=0, CheckValid=1, AlsoAverageHeight=2 }
		private static LayerMask heightTool_LayerMask = -1;
		private static float heightTool_startHeight = 1000f;
		private static float heightTool_endHeight = -1000f;
		private static bool heightTool_markInvalids = true;
		private static heightTool_extraPrecision_type heightTool_extraPrecision = heightTool_extraPrecision_type.Off;
		
		private Plane plane = new Plane(Vector3.up, Vector3.zero);
		private int lastMarked = -1;
		private bool linkTool_MarkMode = false;
		private bool linkTool_UnlinkMode = false;
		private static bool linkTool_DrawLinks = true;
		private static List<int>[] linkTool_Nodes = { new List<int>(), new List<int>() };
		private static int linkTool_Data = 0;
		private static int nextIdent = 1;

		private struct LinkCacheEntry
		{
			public int ident;
			public int idx1;
			public int idx2;
			public int data;
		}

		private static Dictionary<int, Color> col = new Dictionary<int, Color>();
		private static Dictionary<int, List<LinkCacheEntry>> linkCache = new Dictionary<int, List<LinkCacheEntry>>();
		private int selLinkIdx = -1;
		private List<LinkCacheEntry> unlinkSelection = new List<LinkCacheEntry>();

		// ------------------------------------------------------------------------------------------------------------

		protected virtual void OnEnable()
		{
			mapnav = target as MapNavBase;
			UpdateLinkCache();
		}

		private bool LinkCacheContains(List<LinkCacheEntry> lst, int a, int b)
		{
			foreach (LinkCacheEntry n in lst)
			{
				if (n.idx1 == a && n.idx2 == b) return true;
				if (n.idx1 == b && n.idx2 == a) return true;
			}
			return false;
		}

		private void UpdateLinkCache()
		{
			linkCache.Clear();			
			col.Clear();
			nextIdent = 1;
			foreach (MapNavNode n in mapnav.grid)
			{
				if (n.linkData == null) continue;
				foreach (MapNavNodeLink nl in n.linkData)
				{
					if (!linkCache.ContainsKey(nl.data))
					{
						linkCache.Add(nl.data, new List<LinkCacheEntry>());
					}

					List<LinkCacheEntry> lst = linkCache[nl.data];
					if (!LinkCacheContains(lst, n.idx, nl.nodeIdx))
					{
						lst.Add(new LinkCacheEntry() { ident = nextIdent, idx1 = n.idx, idx2 = nl.nodeIdx, data = nl.data });
						nextIdent++;
					}
				}
			}

			if (linkCache.Count > 0)
			{
				List<Color> gened = GeneratedColors(linkCache.Count);
				int cnt = 0;
				foreach (int data in linkCache.Keys)
				{
					col.Add(data, gened[cnt]);
					cnt++; if (cnt >= gened.Count) cnt = gened.Count - 1;
				}
			}
		}

		public List<Color> GeneratedColors(int n)
		{
			List<Color> res = new List<Color>();
			Color[] c = 
			{
				new Color(0f, 1f, 0f, 1f),
				new Color(0f, 0f, 1f, 1f),
				new Color(0f, 1f, 1f, 1f),
				new Color(1f, 0f, 1f, 1f),
				new Color(0f, 0.5f, 1.0f, 1f),
				new Color(0f, 1.0f, 0.5f, 1f),
				new Color(0.5f, 0f, 1.0f, 1f),
				new Color(1.0f, 0f, 0.5f, 1f),
				new Color(1.0f, 0.5f, 0f, 1f),
			};

			int a = 0;
			for (int i = 0; i < n; i++)
			{
				res.Add(c[a]);
				a++; if (a >= c.Length) a = 0;
			}
			return res;
		}

		// ------------------------------------------------------------------------------------------------------------

		private void InitUI()
		{
			if (Toolbox_Style == null)
			{
				Toolbox_Style = new GUIStyle(GUI.skin.box) { padding = new RectOffset(10, 10, 10, 10) };
			}
		}

		public override void OnInspectorGUI()
		{
			DrawDefaultInspector();
			if (GUI.changed)
			{
				EditorUtility.SetDirty(mapnav);
				GUI.changed = false;
			}

			InitUI();

			mapnav = target as MapNavBase;
			if (mapnav == null) return;

			EditorGUILayout.Space();
			GUILayout.Label("Tools");
			if (GUILayout.Button("Generate Grid"))
			{
				for (int i = 0; i < tool.Length; i++) tool[i] = false;
				linkCache.Clear();
				col.Clear();
				linkTool_Nodes[0].Clear();
				linkTool_Nodes[1].Clear();

				mapnav.CreateGrid(NodeType);
				EditorUtility.SetDirty(mapnav);
			}
			if (GUILayout.Button("Lift Grid"))
			{
				mapnav.ChangeGridHeight(+1);
				EditorUtility.SetDirty(mapnav);
			}
			if (GUILayout.Button("Lower Grid"))
			{
				mapnav.ChangeGridHeight(-1);
				EditorUtility.SetDirty(mapnav);
			}
			if (GUILayout.Button("Smooth Out"))
			{
				mapnav.SmoothOut();
				EditorUtility.SetDirty(mapnav);
			}

			tool[0] = GUILayout.Toggle(tool[0], "Height Tool", GUI.skin.button);
			if (tool[0])
			{
				EditorGUILayout.BeginVertical(Toolbox_Style);
				heightTool_LayerMask = MapNavEdUtil.GUILayout_LayerMaskField("Layer Mask", heightTool_LayerMask);
				heightTool_startHeight = EditorGUILayout.FloatField("Start Height", heightTool_startHeight);
				heightTool_endHeight = EditorGUILayout.FloatField("End Height", heightTool_endHeight);
				heightTool_markInvalids = EditorGUILayout.Toggle("Mark Invalid", heightTool_markInvalids);
				heightTool_extraPrecision = (heightTool_extraPrecision_type)EditorGUILayout.EnumPopup("Extra Precision", heightTool_extraPrecision);
				if (MapNavEdUtil.GUILayout_LabelButton("", "Execute"))
				{
					mapnav.AdjustToColliders(heightTool_LayerMask, heightTool_startHeight, linkTool_Data, heightTool_markInvalids, (int)heightTool_extraPrecision);
					EditorUtility.SetDirty(mapnav);
				}
				EditorGUILayout.EndVertical();
			}

			EditorGUI.BeginChangeCheck();
			tool[1] = GUILayout.Toggle(tool[1], "NodeLink Tool", GUI.skin.button);
			if (EditorGUI.EndChangeCheck()) SceneView.RepaintAll();

			if (tool[1])
			{
				EditorGUILayout.BeginVertical(Toolbox_Style);
				DrawLinkTool();
				EditorGUILayout.EndVertical();
			}

			EditorGUILayout.Space();
			GUILayout.Label("Debug");
			mapnav._dev_show_nodes = GUILayout.Toggle(mapnav._dev_show_nodes, "Show Nodes", GUI.skin.button);
			if (mapnav._dev_show_nodes)
			{
				EditorGUI.BeginChangeCheck();
				debug_ShowLabels = EditorGUILayout.Toggle("Show labels", debug_ShowLabels);
				if (EditorGUI.EndChangeCheck()) SceneView.RepaintAll();
			}
		}

		private void DrawLinkTool()
		{
			EditorGUI.BeginChangeCheck();
			linkTool_MarkMode = GUILayout.Toggle(linkTool_MarkMode, "Link Mode", GUI.skin.button);
			if (EditorGUI.EndChangeCheck())
			{
				// reset when mode changes
				if (linkTool_MarkMode) linkTool_UnlinkMode = false;
				unlinkSelection.Clear();
				linkTool_Nodes[0].Clear();
				linkTool_Nodes[1].Clear();
				SceneView.RepaintAll();
			}

			if (linkTool_MarkMode)
			{
				EditorGUILayout.BeginVertical(Toolbox_Style);
				if (GUILayout.Button("Clear Marked"))
				{
					linkTool_Nodes[0].Clear();
					linkTool_Nodes[1].Clear();
					SceneView.RepaintAll();
				}
				GUILayout.Space(10);
				linkTool_Data = EditorGUILayout.IntField("Link Data", linkTool_Data);
				if (MapNavEdUtil.GUILayout_LabelButton("", "Apply"))
				{
					UpdateNodeLinkData();
					linkTool_Nodes[0].Clear();
					linkTool_Nodes[1].Clear();
					SceneView.RepaintAll();
				}

				EditorGUILayout.EndVertical();
				GUILayout.Space(30);
			}

			EditorGUI.BeginChangeCheck();
			linkTool_UnlinkMode = GUILayout.Toggle(linkTool_UnlinkMode, "UnLink Mode", GUI.skin.button);
			if (EditorGUI.EndChangeCheck())
			{
				// reset when mode changes
				if (linkTool_UnlinkMode) linkTool_MarkMode = false;
				unlinkSelection.Clear();
				linkTool_Nodes[0].Clear();
				linkTool_Nodes[1].Clear();
				SceneView.RepaintAll();
			}

			if (linkTool_UnlinkMode)
			{
				EditorGUILayout.BeginVertical(Toolbox_Style);
				if (GUILayout.Button("Remove all Links"))
				{
					if (EditorUtility.DisplayDialog("Warning", "You are about to remove all link data, continue?", "Yes", "Cancel"))
					{
						for (int i = 0; i < mapnav.grid.Length; i++) mapnav.grid[i].linkData = null;
						EditorUtility.SetDirty(mapnav);
						GUI.changed = false;

						col.Clear();
						linkCache.Clear();
						linkTool_Nodes[0].Clear();
						linkTool_Nodes[1].Clear();
						SceneView.RepaintAll();
					}
				}
				GUILayout.Space(10);
				if (unlinkSelection.Count > 0)
				{
					if (GUILayout.Button("Remove Selected"))
					{
						foreach (LinkCacheEntry n in unlinkSelection)
						{
							mapnav.grid[n.idx1].RemoveLinkData(n.idx2, mapnav);
						}

						EditorUtility.SetDirty(mapnav);
						GUI.changed = false;

						unlinkSelection.Clear();
						col.Clear();
						linkCache.Clear();
						linkTool_Nodes[0].Clear();
						linkTool_Nodes[1].Clear();
						UpdateLinkCache();
						SceneView.RepaintAll();
					}

					foreach (LinkCacheEntry n in unlinkSelection)
					{
						//EditorGUILayout.BeginHorizontal();
						GUILayout.Label(mapnav.grid[n.idx1].ToString() + " => " + mapnav.grid[n.idx2].ToString());
						//EditorGUILayout.EndHorizontal();
					}
				}

				EditorGUILayout.EndVertical();
				GUILayout.Space(30);
			}

			GUILayout.Space(15);
			EditorGUI.BeginChangeCheck();
			linkTool_DrawLinks = EditorGUILayout.Toggle("Show NodeLinks", linkTool_DrawLinks);
			if (EditorGUI.EndChangeCheck()) SceneView.RepaintAll();
		}

		private void UpdateNodeLinkData()
		{
			if (linkTool_Nodes[0].Count == 1 && linkTool_Nodes[1].Count == 1)
			{
				mapnav.grid[linkTool_Nodes[0][0]].UpdateOrCreateLinkData(linkTool_Nodes[1][0], linkTool_Data, mapnav);
			}
			else
			{
				for (int i = 0; i < linkTool_Nodes[0].Count; i++)
				{
					// get neighbours of the node and check which of those are in the red set of nodes
					List<int> ids = mapnav.NodeIndicesAround(linkTool_Nodes[0][i], 1, null);
					for (int j = 0; j < ids.Count; j++)
					{
						if (linkTool_Nodes[1].Contains(ids[j]))
						{
							mapnav.grid[linkTool_Nodes[0][i]].UpdateOrCreateLinkData(ids[j], linkTool_Data, mapnav);
						}
					}
				}
			}

			EditorUtility.SetDirty(mapnav);
			GUI.changed = false;

			UpdateLinkCache();
		}

		// ------------------------------------------------------------------------------------------------------------

		protected virtual void OnSceneGUI()
		{
			mapnav = target as MapNavBase;
			if (mapnav == null) return;
			
			if (mapnav._dev_show_nodes && debug_ShowLabels)
			{
				Handles.color = Color.cyan;
				float tz = mapnav.nodeSize * 3f;

				int idx = -1;
				for (int y = 0; y < mapnav.mapVerticalSize; y++)
				{
					for (int x = 0; x < mapnav.mapHorizontalSize; x++)
					{
						idx++;
						if (idx >= mapnav.grid.Length) break;
						if (!mapnav.grid[idx].isValid) continue;

						float sz = HandleUtility.GetHandleSize(mapnav.grid[idx].position);
						if (sz > tz) continue;
						string l = mapnav.grid[idx].q + ", " + mapnav.grid[idx].r;
						Handles.Label(mapnav.grid[idx].position, l);
					}
				}
			}

			if (tool[1])
			{
				Tools.current = Tool.None;

				if (linkTool_MarkMode)
				{
					DrawLinkToolMarkMode();
				}

				if (linkTool_DrawLinks)
				{
					DrawLinks();
				}
			}

			// prevent selection of other objects while the map is active
			if (Event.current.type == EventType.Layout)
			{
				HandleUtility.AddDefaultControl(0);
			}
		}

		private void DrawLinkToolMarkMode()
		{
			// check what node was clicked on
			if (Event.current.button == 0 && (Tools.viewTool == ViewTool.Pan || Tools.viewTool == ViewTool.None))
			{
				int nSet = Event.current.modifiers == EventModifiers.Shift ? 1 : 0;
				if (Event.current.type == EventType.MouseDown || Event.current.type == EventType.MouseDrag)
				{
					float rayDist = 0f;
					Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
					if (plane.Raycast(ray, out rayDist))
					{
						Vector3 rayPoint = ray.GetPoint(rayDist);
						MapNavNode n = mapnav.NodeAtWorldPosition<MapNavNode>(rayPoint);
						if (n != null)
						{
							if (lastMarked != n.idx)
							{
								lastMarked = n.idx;
								if (linkTool_Nodes[nSet].Contains(n.idx)) linkTool_Nodes[nSet].Remove(n.idx);
								else linkTool_Nodes[nSet].Add(n.idx);
							}
						}
					}
					Event.current.Use();
				}
				else if (Event.current.type == EventType.MouseUp)
				{
					lastMarked = -1;
					Event.current.Use();
				}
			}

			float sz = mapnav.nodeSize / 3f;
			Handles.color = Color.blue;
			for (int i = 0; i < linkTool_Nodes[0].Count; i++)
			{
				Handles.DrawSolidDisc(mapnav.grid[linkTool_Nodes[0][i]].position, Vector3.up, sz);
			}
			sz -= 0.05f;
			Handles.color = Color.red;
			for (int i = 0; i < linkTool_Nodes[1].Count; i++)
			{
				Handles.DrawSolidDisc(mapnav.grid[linkTool_Nodes[1][i]].position, Vector3.up, sz);
			}
		}

		private void DrawLinks()
		{

			if (linkTool_UnlinkMode)
			{
				foreach (List<LinkCacheEntry> cache in linkCache.Values)
					foreach (LinkCacheEntry n in cache)
					{
						if (selLinkIdx == n.idx1)
						{
							Handles.color = Color.red;
						}
						else
						{
							Handles.color = col[n.data];
						}

						if (selLinkIdx < 0 || selLinkIdx == n.idx1)
						{
							Handles.DrawLine(mapnav.grid[n.idx1].position, mapnav.grid[n.idx2].position);
							float baseSize = HandleUtility.GetHandleSize(mapnav.grid[n.idx1].position);
							if (Handles.Button(mapnav.grid[n.idx1].position, Quaternion.identity, baseSize * 0.2f, baseSize * 0.1f, DrawNodeLinkButton))
							{
								if (selLinkIdx == n.idx1) selLinkIdx = -1;
								else selLinkIdx = n.idx1;
								SceneView.RepaintAll();
							}

							Handles.color = col[n.data];
							if (selLinkIdx == n.idx1)
							{
								if (Handles.Button(mapnav.grid[n.idx2].position, Quaternion.identity, baseSize * 0.2f, baseSize * 0.1f, DrawNodeLinkButton))
								{
									if (!unlinkSelection.Contains(n)) unlinkSelection.Add(n);
									selLinkIdx = -1;
									Repaint();
									SceneView.RepaintAll();
								}
							}
							else
							{
								Handles.CubeCap(0, mapnav.grid[n.idx2].position, Quaternion.identity, baseSize * 0.1f);
							}
						}
					}
			}
			else
			{
				foreach (List<LinkCacheEntry> cache in linkCache.Values)
					foreach (LinkCacheEntry n in cache)
					{
						Handles.color = col[n.data];
						Handles.DrawLine(mapnav.grid[n.idx1].position, mapnav.grid[n.idx2].position);
						float baseSize = HandleUtility.GetHandleSize(mapnav.grid[n.idx1].position);
						Handles.CubeCap(0, mapnav.grid[n.idx1].position, Quaternion.identity, baseSize * 0.1f);
						Handles.CubeCap(0, mapnav.grid[n.idx2].position, Quaternion.identity, baseSize * 0.1f);
					}
			}
		}

		private void DrawNodeLinkButton(int controlId, Vector3 position, Quaternion rotation, float size)
		{
			Handles.CubeCap(controlId, position, rotation, size);
		}

		// ------------------------------------------------------------------------------------------------------------
	}
}