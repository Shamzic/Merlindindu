// -= MapNav 2 =-
// www.plyoung.com
// by Leslie Young
// ====================================================================================================================

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace MapNavKit
{
	/// <summary> 
	/// Data for a node of the MapNav grid.
	/// </summary>
	[System.Serializable]
	public class MapNavNode : ScriptableObject
	{
		/// <summary> Index of tile in the grid. Nodes are saved into a one-dimensional array (MapNavBase.grid[]).
		/// If -1 then this node is not on the grid. It is possible that some nodes are only "place holders" so
		/// that nodes are offset correctly in the array they are stored in. This normally only happens with 
		/// the Hexa grids when in axial format. </summary>
		public int idx = -1;

		/// <summary> Position Q (column/x) in 2D grid. </summary>
		public int q = 0;

		/// <summary> Position R (row/z) in 2D grid. </summary>
		public int r = 0;

		/// <summary> Position H (height/y) used to calculate localPosition using MapNavBase.nodeHeightStep. </summary>
		public int h = 0;

		/// <summary> Position of the node in 3D space, calculated and set when the grid was created/ updated by 
		/// one of the grid manipulation functions. </summary>
		public Vector3 localPosition = Vector3.zero;

		/// <summary> Grid object (MapNav) that owns the node. Used when calculation the position of a node in 3D space. 
		/// If NULL then this node is not on the grid. It is possible that some nodes are only "place holders" so that 
		/// nodes are offset correctly in the array they are stored in. </summary>
		public Transform parent = null;

		/// <summary> Use 'isValid' to check if a node is valid. This property, foredInvalid, is used to force a valid node to
		/// be invalid. This is mostly used by the tools, like Height Tool, to indicate that a node might be "off" the terrain
		/// area but you might use it to make a node invalid for whatever reason. Note that invalid nodes are skipped in just
		/// about all functions that processes nodes except those that does something that needs to update this property. </summary>
		public bool foredInvalid = false;

		/// <summary> Returns True if the node is in the grid. If False is returned then this node should be ignored. </summary>
		public bool isValid { get { return (idx > -1 && parent != null && foredInvalid == false); } }

		/// <summary> The position of the node in 3D world space. </summary>
		public Vector3 position { get { if (parent == null) return localPosition; return parent.position + localPosition; } }

		/// <summary> Custom data related to links between this and other nodes, if any </summary>
		public List<MapNavNodeLink> linkData = null;

		// ------------------------------------------------------------------------------------------------------------

		public override string ToString()
		{
			return string.Format("({0},{1}::{2})", q, r,idx);
		}

		/// <summary> Return the link data between this node and the targetNodeIdx or null if none exist. </summary>
		public MapNavNodeLink GetLinkData(int targetNodeIdx)
		{
			if (linkData == null) return null;
			for (int i = 0; i < linkData.Count; i++)
			{
				if (linkData[i].nodeIdx == targetNodeIdx)
				{
					return linkData[i];
				}
			}
			return null;
		}

		/// <summary> This will update the link data with the target node or create the data if 
		/// it does not yet exist. </summary>
		public void UpdateOrCreateLinkData(int targetNodeIdx, int data, MapNavBase map)
		{
			//Debug.Log(ToString() + " <-> " + map.grid[targetNodeIdx].ToString() + " = " + data);
			if (this.idx < 0)
			{
				Debug.LogError("This node is not a valid node. Link data should not be created with it.");
				return;
			}

			if (targetNodeIdx < 0 || targetNodeIdx >= map.grid.Length)
			{
				Debug.LogError("Target Node's index is invalid: " + targetNodeIdx);
				return;
			}

			if (map.grid[targetNodeIdx].idx < 0)
			{
				Debug.LogError("Target node is not a valid node. Link data should not be created with it.");
				return;
			}

			MapNavNodeLink d = GetLinkData(targetNodeIdx);
			if (d == null)
			{
				if (linkData == null) linkData = new List<MapNavNodeLink>();
				d = new MapNavNodeLink() { nodeIdx = targetNodeIdx };
				linkData.Add(d);
			}

			d.data = data;

			// make sure the other node has the same data
			d =	map.grid[targetNodeIdx].GetLinkData(this.idx);
			if (d == null)
			{
				if (map.grid[targetNodeIdx].linkData == null) map.grid[targetNodeIdx].linkData = new List<MapNavNodeLink>();
				d = new MapNavNodeLink() { nodeIdx = this.idx };
				map.grid[targetNodeIdx].linkData.Add(d);
			}

			d.data = data;
		}

		/// <summary> Remove link data with target node. </summary>
		public void RemoveLinkData(int targetNodeIdx, MapNavBase map)
		{
			if (this.idx < 0)
			{
				Debug.LogError("This node is not a valid node. Link data should not be created with it.");
				return;
			}

			if (targetNodeIdx < 0 || targetNodeIdx >= map.grid.Length)
			{
				Debug.LogError("Target Node's index is invalid: " + targetNodeIdx);
				return;
			}

			if (map.grid[targetNodeIdx].idx < 0)
			{
				Debug.LogError("Target node is not a valid node. Link data should not be created with it.");
				return;
			}

			MapNavNodeLink data = GetLinkData(targetNodeIdx);
			if (data != null) linkData.Remove(data);

			data = map.grid[targetNodeIdx].GetLinkData(this.idx);
			if (data != null) map.grid[targetNodeIdx].linkData.Remove(data);
		}

		// ------------------------------------------------------------------------------------------------------------
	}
}