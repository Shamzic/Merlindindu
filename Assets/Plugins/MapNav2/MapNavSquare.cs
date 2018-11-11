// -= MapNav 2 =-
// www.plyoung.com
// by Leslie Young
// ====================================================================================================================

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace MapNavKit
{
	public class MapNavSquare : MapNavBase
	{
		// ------------------------------------------------------------------------------------------------------------
		#region definitions

		private static int[,] Neighbours4 = new int[,] { { +1, 0 }, { 0, +1 }, { -1, 0 }, { 0, -1 } };
		private static int[,] Neighbours8 = new int[,] { { +1, 0 }, { +1, +1 }, { 0, +1 }, { -1, +1 }, { -1, 0 }, { -1, -1 }, { 0, -1 }, { +1, -1 } };

		#endregion
		// ------------------------------------------------------------------------------------------------------------
		#region properties

		/// <summary>
		/// Does this grid use the 4 or 8 neighbour system? In a 4-neighbour system the nodes diagonal to another 
		/// are not considered its neighbours and units can't move directly to them from a specified node.
		/// </summary>
		public bool diagonalNeighbours = true;

		#endregion
		// ------------------------------------------------------------------------------------------------------------
		#region grid manipulation

		/// <summary> Destroy Grid array and creates a new one from properties. </summary>
		/// <typeparam name="T"> You can specify a custom Node class derived from MapNavNode </typeparam>
		public override void CreateGrid<T>()
		{
			grid = new T[mapHorizontalSize * mapVerticalSize];

			Vector3 posOffs = new Vector3(-mapHorizontalSize * nodeSize / 2f, 0f, -mapVerticalSize * nodeSize / 2f) + new Vector3(nodeSize / 2f, 0f, nodeSize / 2f);
			int idx = -1;
			for (int y = 0; y < mapVerticalSize; y++)
			{
				for (int x = 0; x < mapHorizontalSize; x++)
				{
					idx++;
					grid[idx] = ScriptableObject.CreateInstance<T>();
					grid[idx].idx = idx;
					grid[idx].q = x;
					grid[idx].r = y;
					grid[idx].parent = transform;
					grid[idx].h = (nodeHeightOpt == NodeHeightOpt.Flat ? minNodeHeight : Random.Range(minNodeHeight, maxNodeHeight + 1));

					grid[idx].localPosition = new Vector3(nodeSize * x, nodeHeightStep * grid[idx].h, nodeSize * y) + posOffs;

					OnNodeCreated(grid[idx]);
				}
			}

			OnGridChanged(true);
		}

		/// <summary> Destroy Grid array and creates a new one from properties. </summary>
		/// <param name="nodeType"> You can specify a custom Node class derived from MapNavNode </param>
		public override void CreateGrid(System.Type nodeType)
		{
			grid = new MapNavNode[mapHorizontalSize * mapVerticalSize];

			Vector3 posOffs = new Vector3(-mapHorizontalSize * nodeSize / 2f, 0f, -mapVerticalSize * nodeSize / 2f) + new Vector3(nodeSize / 2f, 0f, nodeSize / 2f);
			int idx = -1;
			for (int y = 0; y < mapVerticalSize; y++)
			{
				for (int x = 0; x < mapHorizontalSize; x++)
				{
					idx++;
					grid[idx] = (MapNavNode)ScriptableObject.CreateInstance(nodeType);
					grid[idx].idx = idx;
					grid[idx].q = x;
					grid[idx].r = y;
					grid[idx].parent = transform;
					grid[idx].h = (nodeHeightOpt == NodeHeightOpt.Flat ? minNodeHeight : Random.Range(minNodeHeight, maxNodeHeight + 1));

					grid[idx].localPosition = new Vector3(nodeSize * x, nodeHeightStep * grid[idx].h, nodeSize * y) + posOffs;

					OnNodeCreated(grid[idx]);
				}
			}

			OnGridChanged(true);
		}

		/// <summary> This will update the grid nodes by adjusting the height according to colliders that
		/// where hit while casting a ray from start height down to end height. The node's H value will
		/// be updated to be an approximation of the height in the grid while the localPosition will be
		/// updated to be exactly at the height of where the collider was hit. If no collider was hit 
		/// along the way then the node's H position will be set to minNodeHeight and the localPosition
		/// updated to reflect this position. If markInvalids is set then the nodes that was not over
		/// colliders will be set as invalid.
		/// <param name="mask"> Layer mask to test raycast against. </param>
		/// <param name="startHeight"> Height to start from. </param>
		/// <param name="endHeight"> Lowest point to test to. </param>
		/// <param name="markInvalids"> mark invalid nodes? </param>
		/// <param name="extraPrecision"> Use extra precision? In this mode not only the center of nodes are 
		///									checked but also all its corners (4 for square and 6 for hexa).
		///									If not all points hit the collider then the node will be considered
		///									to not be valid. An average height will be calculated from all the 
		///									points.
		///									0: Do not use this feature
		///									1: Only check for invalid nodes but use node center to calculate the node height
		///									2: Also update the node height with average of all points. </param>
		public override void AdjustToColliders(LayerMask mask, float startHeight, float endHeight, bool markInvalids, int extraPrecision)
		{
			if (endHeight >= startHeight)
			{
				Debug.LogError("The end height must be smaller than start height since the ray is casted downward from start height.");
				return;
			}

			float distance = (startHeight - endHeight);

			for (int idx = 0; idx < grid.Length; idx++)
			{
				grid[idx].foredInvalid = false;
				if (grid[idx].parent == null) continue; // not a valid node

				RaycastHit hit;
				if (Physics.Raycast(new Vector3(grid[idx].position.x, startHeight, grid[idx].position.z), -Vector3.up, out hit, distance, mask))
				{
					if (extraPrecision > 0)
					{	// need to check the corners of the node too
						grid[idx].localPosition.y = startHeight - hit.distance;
						Vector3 p = new Vector3(0f, startHeight, 0f);
						float[] yValues = new float[4];
						Vector2[] corners = new Vector2[] 
						{
							new Vector2(+nodeSize, +nodeSize),
							new Vector2(+nodeSize, -nodeSize),
							new Vector2(-nodeSize, -nodeSize),
							new Vector2(-nodeSize, +nodeSize),
						};

						for (int i = 0; i < 4; i++)
						{
							p.x = grid[idx].position.x + corners[i].x;
							p.z = grid[idx].position.z + corners[i].y;
							if (Physics.Raycast(p, -Vector3.up, out hit, distance, mask))
							{
								yValues[i] = startHeight - hit.distance;
							}
							else
							{
								grid[idx].h = minNodeHeight;
								grid[idx].localPosition.y = nodeHeightStep * grid[idx].h;
								if (markInvalids) grid[idx].foredInvalid = true;
								continue; // no need to continue since one of the corners was not on the valid area
							}
						}

						// if reached here then all corners where on valid area and needs to be averaged to get correct Y
						if (extraPrecision == 2)
						{
							for (int i = 0; i < yValues.Length; i++) grid[idx].localPosition.y += yValues[i];
							grid[idx].localPosition.y /= yValues.Length;
						}

						grid[idx].h = Mathf.RoundToInt(grid[idx].localPosition.y / nodeHeightStep);
					}
					else
					{
						grid[idx].localPosition.y = startHeight - hit.distance;
						grid[idx].h = Mathf.RoundToInt(grid[idx].localPosition.y / nodeHeightStep);
					}
				}
				else
				{
					grid[idx].h = minNodeHeight;
					grid[idx].localPosition.y = nodeHeightStep * grid[idx].h;
					if (markInvalids) grid[idx].foredInvalid = true;
				}
			}

			OnGridChanged(false);
		}

		#endregion
		// ------------------------------------------------------------------------------------------------------------
		#region pub

		/// <summary> Return index of node at q (column) and r (row) position on grid. Return -1 on error. </summary>
		public override int NodeIdx(int q, int r)
		{
			int idx = (r * mapHorizontalSize + q);
			if (idx < 0 || idx >= grid.Length) return -1;

			// make sure it is correct, else use lookup to find it
			if (grid[idx].q == q && grid[idx].r == r)
			{
				return grid[idx].idx;
			}

			for (idx = 0; idx < grid.Length; idx++)
			{
				if (grid[idx].q == q && grid[idx].r == r) return idx;
			}

			return -1;
		}

		/// <summary> Return node at q (column) and r (row) position on grid. Null on error or if node position not on grid. </summary>
		public override T NodeAt<T>(int q, int r)
		{
			int idx = (r * mapHorizontalSize + q);
			if (idx < 0 || idx >= grid.Length) return null;

			// make sure it is correct, else use lookup to find it
			if (grid[idx].q == q && grid[idx].r == r)
			{
				return (T)grid[idx];
			}

			for (idx = 0; idx < grid.Length; idx++)
			{
				if (grid[idx].q == q && grid[idx].r == r) return (grid[idx].isValid ? (T)grid[idx] : null);
			}

			return null;
		}

		/// <summary> Return node that is at the given world position. Return -1 on error or if not valid node in that position. </summary>
		public override int NodeIdxFromWorldPosition(Vector3 pos)
		{
			// work in local coords of grid
			pos = pos - transform.position;

			// the grid is as an offset from 0x0x0
			pos -= (new Vector3(-mapHorizontalSize * nodeSize / 2f, 0f, -mapVerticalSize * nodeSize / 2f) + new Vector3(nodeSize / 2f, 0f, nodeSize / 2f));

			// return node idx
			int idx = NodeIdx(Mathf.RoundToInt(pos.x / nodeSize), Mathf.RoundToInt(pos.z / nodeSize));
			if (false == ValidIDX(idx)) return -1;
			return idx;
		}

		/// <summary> 
		/// Returns list of nodes starting at the "next" node and going anti-clockwise around the central node.
		/// </summary>
		/// <param name="node">				The central node around which to get neighboring nodes.</param>
		/// <param name="includeInvalid">	Should "invalid" nodes be included? If so then a NULL entry will be added to the
		///									returned list for invalid nodes. An invalid node might be one that is stored in 
		///									the grid array but not considered to be in the grid. This normally happens with
		///									Hexa grids. An invalid node might also be one marked as invalid by the callback function. </param>
		/// <param name="includeCentralNode">Should the central node be included? It will be added first in the list if so.</param>
		/// <param name="callback">			An optional callback that can first check if the node is "valid" and return True if 
		///									so, else it should return False. </param>
		/// <returns>Returns null if there was an error. </returns>
		public override List<T> NodesAround<T>(MapNavNode node, bool includeInvalid, bool includeCentralNode, ValidationCallback callback)
		{
			if (node == null) return null;
			List<T> nodes = new List<T>(includeCentralNode ? 7 : 6);
			if (node.idx < 0 || node.idx >= grid.Length) return null;

			if (includeCentralNode)
			{
				if (callback != null)
				{
					if (false == node.isValid || false == callback(node))
					{
						if (includeInvalid) nodes.Add(null);
					}
					else nodes.Add((T)node);
				}
				else
				{
					if (node.isValid) nodes.Add((T)node);
					else if (includeInvalid) nodes.Add(null);
				}
			}

			//int[,] neighbours = diagonalNeighbours ? Neighbours8 : Neighbours4;
			int[,] neighbours = Neighbours8;	// always use 8-neighbour for this so that a ring around can be selected.
			// the special select function witch takes "cost" into account will 
			// take 4-neighbour type selection into account

			for (int dir = 0; dir < neighbours.GetLength(0); dir++)
			{
				int q = grid[node.idx].q + neighbours[dir, 0];
				int r = grid[node.idx].r + neighbours[dir, 1];
				MapNavNode n = NodeAt<MapNavNode>(q, r);

				if (n == null)
				{
					if (includeInvalid) nodes.Add(null);
					continue;
				}

				if (false == n.isValid)
				{
					if (includeInvalid) nodes.Add(null);
					continue;
				}

				if (callback != null)
				{
					if (false == callback(n))
					{
						if (includeInvalid) nodes.Add(null);
						continue;
					}
				}

				nodes.Add((T)n);
			}

			return nodes;
		}

		/// <summary> 
		/// Return list of nodes in a certain range around given node.
		/// The returned list is in no specific order. Excludes invalid nodes.
		/// </summary>
		/// <param name="node">				The central node around which to get neighboring nodes.</param>
		/// <param name="radius">			The radius around the central node. </param>
		/// <param name="includeCentralNode">Should the central node be included? It will be added first in the list if so.</param>
		/// <param name="callback">			An optional callback that can first check if the node is "valid" and return True if 
		///									so, else it should return False. </param>
		/// <returns>Returns null if there was an error. </returns>
		public override List<T> NodesAround<T>(MapNavNode node, int radius, bool includeCentralNode, ValidationCallback callback)
		{
			if (radius < 1) radius = 1;
			if (node == null) return null;
			if (radius == 1) return NodesAround<T>(node, false, includeCentralNode, callback);
			List<T> nodes = new List<T>(0);
			if (node.idx < 0 || node.idx >= grid.Length) return null;

			if (includeCentralNode)
			{
				if (node.isValid)
				{
					if (callback != null)
					{
						if (true == callback(node)) nodes.Add((T)node);
					}
					else nodes.Add((T)node);
				}
			}

			for (int x = -radius; x <= radius; x++)
			{
				for (int y = -radius; y <= radius; y++)
				{
					//if (Mathf.Abs(x + y) > radius) continue;
					int q = node.q + x;
					int r = node.r + y;
					if (q < 0 || r < 0 || q >= mapHorizontalSize || r >= mapVerticalSize) continue;
					int id = (r * mapHorizontalSize + q);

					if (id == node.idx) continue;
					if (id >= 0 && id < grid.Length)
					{
						if (grid[id].isValid)
						{
							if (callback != null)
							{
								if (false == callback(grid[id])) continue;
							}

							if (false == nodes.Contains((T)grid[id]))
							{
								nodes.Add((T)grid[id]);
							}
						}
					}
				}
			}

			return nodes;
		}

		/// <summary> 
		/// Returns list of nodes indices starting at the "next" node and going anti-clockwise around the central node.
		/// </summary>
		/// <param name="idx">				Index of the central node around which to get neighboring nodes.</param>
		/// <param name="includeInvalid">	Should "invalid" nodes be included? If so then a -1 entry will be added to the
		///									returned list for invalid nodes. An invalid node might be one that is stored in 
		///									the grid array but not considered to be in the grid. This normally happens with
		///									Hexa grids. An invalid node might also be one marked as invalid by the callback function. </param>
		/// <param name="includeCentralNode">Should the central node be included? It will be added first in the list if so.</param>
		/// <param name="callback">			An optional callback that can first check if the node is "valid" and return True if 
		///									so, else it should return False. </param>
		/// <returns>Returns null if there was an error. </returns>
		public override List<int> NodeIndicesAround(int idx, bool includeInvalid, bool includeCentralNode, ValidationCallback callback)
		{
			List<int> nodes = new List<int>();
			if (idx < 0 || idx >= grid.Length) return null;

			if (includeCentralNode)
			{
				if (grid[idx] != null)
				{
					if (callback != null)
					{
						if (false == grid[idx].isValid || false == callback(grid[idx]))
						{
							if (includeInvalid) nodes.Add(-1);
						}
						else nodes.Add(idx);
					}
					else
					{
						if (grid[idx].isValid) nodes.Add(idx);
						else if (includeInvalid) nodes.Add(-1);
					}
				}
				else if (includeInvalid) nodes.Add(-1);
			}

			//int[,] neighbours = diagonalNeighbours ? Neighbours8 : Neighbours4;
			int[,] neighbours = Neighbours8;	// always use 8-neighbour for this so that a ring around can be selected.
			// the special select function witch takes "cost" into account will 
			// take 4-neighbour type selection into account

			for (int i = 0; i < neighbours.GetLength(0); i++)
			{
				int x = grid[idx].q + neighbours[i, 0];
				int y = grid[idx].r + neighbours[i, 1];
				int id = NodeIdx(x, y);

				if (id < 0)
				{
					if (includeInvalid) nodes.Add(-1);
					continue;
				}

				if (grid[id] == null)
				{
					if (includeInvalid) nodes.Add(-1);
					continue;
				}

				if (false == grid[id].isValid)
				{
					if (includeInvalid) nodes.Add(-1);
					continue;
				}

				if (callback != null)
				{
					if (false == callback(grid[id]))
					{
						if (includeInvalid) nodes.Add(-1);
						continue;
					}
				}

				nodes.Add(id);
			}

			return nodes;
		}

		/// <summary> 
		/// Return list of indices for nodes in a certain range around given node.
		/// The returned list is in no specific order. Excludes invalid nodes.
		/// </summary>
		/// <param name="idx">				Index of the central node around which to get neighboring nodes.</param>
		/// <param name="radius">			The radius around the central node. </param>
		/// <param name="includeCentralNode">Should the central node be included? It will be added first in the list if so.</param>
		/// <param name="callback">			An optional callback that can first check if the node is "valid" and return True if 
		///									so, else it should return False. </param>
		/// <returns>Returns null if there was an error. </returns>
		public override List<int> NodeIndicesAround(int idx, int radius, bool includeCentralNode, ValidationCallback callback)
		{
			if (radius < 1) radius = 1;
			if (radius == 1) return NodeIndicesAround(idx, false, includeCentralNode, callback);
			List<int> nodes = new List<int>(0);
			if (idx < 0 || idx >= grid.Length) return null;

			if (includeCentralNode)
			{
				if (grid[idx] != null)
				{
					if (grid[idx].isValid)
					{
						if (callback != null)
						{
							if (true == callback(grid[idx])) nodes.Add(idx);
						}
						else nodes.Add(idx);
					}
				}
			}

			for (int x = -radius; x <= radius; x++)
			{
				for (int y = -radius; y <= radius; y++)
				{
					int q = grid[idx].q + x;
					int r = grid[idx].r + y;
					if (q < 0 || r < 0 || q >= mapHorizontalSize || r >= mapVerticalSize) continue;
					int id = (r * mapHorizontalSize + q);

					if (id == idx) continue;
					if (id >= 0 && id < grid.Length)
					{
						if (grid[id].isValid)
						{
							if (callback != null)
							{
								if (false == callback(grid[id])) continue;
							}

							if (false == nodes.Contains(id))
							{
								nodes.Add(id);
							}
						}
					}
				}
			}

			return nodes;
		}

		/// <summary>
		/// Returns the distance from node 1 to node 2.
		/// </summary>
		public override int Distance(int idx1, int idx2)
		{
			if (idx1 == idx2) return 0;
			if (diagonalNeighbours) return Mathf.Max(Mathf.Abs(grid[idx1].q - grid[idx2].q), Mathf.Abs(grid[idx1].r - grid[idx2].r));
			return (Mathf.Abs(grid[idx1].q - grid[idx2].q) + Mathf.Abs(grid[idx1].r - grid[idx2].r));
		}

		#endregion
		// ------------------------------------------------------------------------------------------------------------
		#region helpers

		protected override List<int> PathNodeIndicesAround(int idx)
		{
			if (idx < 0 || idx >= grid.Length) return null;
			List<int> nodes = new List<int>();

			int[,] neighbours = diagonalNeighbours ? Neighbours8 : Neighbours4;

			for (int i = 0; i < neighbours.GetLength(0); i++)
			{
				int x = grid[idx].q + neighbours[i, 0];
				int y = grid[idx].r + neighbours[i, 1];
				int id = NodeIdx(x, y);

				if (id < 0) continue;
				if (grid[id] == null) continue;
				if (false == grid[id].isValid) continue;

				nodes.Add(id);
			}

			return nodes;
		}

		/// <summary> This is a helper for CheckNodesRecursive() </summary>
		protected override List<int> _neighbours(int idx)
		{
			List<int> nodes = new List<int>();
			int[,] neighbours = diagonalNeighbours ? Neighbours8 : Neighbours4;
			for (int dir = 0; dir < neighbours.GetLength(0); dir++)
			{
				int q = grid[idx].q + neighbours[dir, 0];
				int r = grid[idx].r + neighbours[dir, 1];
				int id = NodeIdx(q, r);

				if (id < 0) continue;
				if (grid[id] == null) continue;
				if (false == grid[id].isValid) continue;

				nodes.Add(id);
			}
			return nodes;
		}

		#endregion
		// ------------------------------------------------------------------------------------------------------------
		#region debug

		protected virtual void OnDrawGizmos()
		{
			if (_dev_show_nodes && grid.Length > 0)
			{
				Vector3 sz = new Vector3(nodeSize, 0f, nodeSize);
				for (int idx = 0; idx < grid.Length; idx++)
				{
					if (!grid[idx].isValid && !grid[idx].foredInvalid) continue;
					Gizmos.color = grid[idx].foredInvalid ? Color.grey : Color.cyan;
					Gizmos.DrawWireCube(grid[idx].position, sz);
				}
			}
		}

		#endregion
		// ------------------------------------------------------------------------------------------------------------
	}
}
