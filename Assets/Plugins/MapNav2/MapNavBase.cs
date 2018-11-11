// -= MapNav 2 =-
// www.plyoung.com
// by Leslie Young
// ====================================================================================================================

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Priority_Queue;

namespace MapNavKit
{
	/// <summary> 
	/// The base class that contains all the data and functions needed to interact with the grid.
	/// The Hexa and Square grids are derived from this.
	/// </summary>
	public class MapNavBase : MonoBehaviour
	{
		// ------------------------------------------------------------------------------------------------------------
		#region definitions

		/// <summary> Used when a function needs to check if a node is valid. </summary>
		/// <param name="node"> The node to be checked. </param>
		/// <returns> Return True if the node is valid, else False .</returns>
		public delegate bool ValidationCallback(MapNavNode node);

		/// <summary> Used by the path finding functions. It should return 1 for a normal cost, else it 
		/// should return 2+ for higher cost and 0 if can't move to the node. The fromNode and toNode are
		/// neighbouring nodes and you may ignore the fromNode if that is not needed for your design. </summary>
		/// <param name="fromNode"> Cost should be calculated for moving from this node. </param>
		/// <param name="toNode">	to moving to this node. </param>
		/// <returns> Return 0 if can't move onto toNode, else 1+ to indicate cost of moving. </returns>
		public delegate float NodeCostCallback(MapNavNode fromNode, MapNavNode toNode);

		/// <summary> How node height will be determined when creating a new grid. </summary>
		public enum NodeHeightOpt
		{
			/// <summary> Grid is flat, using minNodeHeight. </summary>
			Flat,
			/// <summary> Grid nodes have random height between minNodeHeight and maxNodeHeight. </summary>
			Random,
		}

		#endregion
		// ------------------------------------------------------------------------------------------------------------
		#region properties

		/// <summary> The Horizontal size of the grid (along Q/ X). </summary>
		public int mapHorizontalSize = 5;

		/// <summary> The Vertical size of the grid (along R/ Z). </summary>
		public int mapVerticalSize = 5;

		/// <summary> The 3D size of a node in Unity units. </summary>
		public float nodeSize = 1f;

		/// <summary> The 3D height step of a node. (MapNavNode.h * nodeHeightStep) gives 3D position. </summary>
		public float nodeHeightStep = 0.15f;

		/// <summary> Min Clamp value for MapNavNode.h (Y) </summary>
		public int minNodeHeight = 0;

		/// <summary> Max Clamp value for MapNavNode.h (Y) </summary>
		public int maxNodeHeight = 5;

		/// <summary> Option to use when creating new grid. </summary>
		public NodeHeightOpt nodeHeightOpt = NodeHeightOpt.Flat;

		/// <summary> The saved grid data. </summary>
		[HideInInspector] public MapNavNode[] grid = new MapNavNode[0];

		/// <summary> If True then Debug info/ Gizmos of the Grid will be rendered in the Scene view. </summary>
		[HideInInspector] public bool _dev_show_nodes = false;

		#endregion
		// ------------------------------------------------------------------------------------------------------------
		#region grid manipulation

		/// <summary> Destroy Grid array and creates a new one from properties. </summary>
		/// <typeparam name="T"> You can specify a custom Node class derived from MapNavNode </typeparam>
		public virtual void CreateGrid<T>() where T : MapNavNode
		{
		}

		/// <summary> Destroy Grid array and creates a new one from properties. </summary>
		/// <param name="nodeType"> You can specify a custom Node class derived from MapNavNode </param>
		public virtual void CreateGrid(System.Type nodeType)
		{
		}

		/// <summary> Destroy Grid array and creates a new one from properties. </summary>
		public virtual void CreateGrid()
		{
			CreateGrid<MapNavNode>();
		}

		/// <summary> Update each node by offset height, clamped to minNodeHeight and maxNodeHeight. </summary>
		public virtual void ChangeGridHeight(int offs)
		{
			for (int idx = 0; idx < grid.Length; idx++)
			{
				if (!grid[idx].isValid) continue;
				grid[idx].h += offs;
				if (grid[idx].h < minNodeHeight) grid[idx].h = minNodeHeight;
				if (grid[idx].h > maxNodeHeight) grid[idx].h = maxNodeHeight;
				grid[idx].localPosition.y = nodeHeightStep * grid[idx].h;
			}
			OnGridChanged(false);
		}

		/// <summary> Modify the nodes so that the difference in height between them are not so big. </summary>
		public virtual void SmoothOut()
		{
			for (int idx = 0; idx < grid.Length; idx++)
			{
				if (false == grid[idx].isValid) continue;

				// get all neighbour nodes
				List<int> ids = NodeIndicesAround(idx, false, true, null);

				// check what biggest difference in height is and try reach in that direction
				int direction = 0;
				int lastDiff = 0;
				for (int i = 0; i < ids.Count; i++)
				{
					if (grid[idx].h != grid[ids[i]].h)
					{
						if (Mathf.Abs(grid[idx].h - grid[ids[i]].h) > lastDiff)
						{
							direction = (grid[idx].h < grid[ids[i]].h ? 1 : -1);
							lastDiff = Mathf.Abs(grid[idx].h - grid[ids[i]].h);
						}
					}
				}

				// now lower or raise the node
				if (direction != 0)
				{
					grid[idx].h = Mathf.Clamp(grid[idx].h + direction, minNodeHeight, maxNodeHeight);
					grid[idx].localPosition.y = nodeHeightStep * grid[idx].h;
				}

			}

			OnGridChanged(false);
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
		public virtual void AdjustToColliders(LayerMask mask, float startHeight, float endHeight, bool markInvalids, int extraPrecision)
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
					grid[idx].localPosition.y = startHeight - hit.distance;
					grid[idx].h = Mathf.RoundToInt(grid[idx].localPosition.y / nodeHeightStep);
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
		#region get node

		/// <summary> Return index of node at x and y position on grid. Return -1 on error or if not valid node in that position. </summary>
		public virtual int NodeIdx(int x, int y)
		{
			return -1;
		}

		/// <summary> Return node at x and y position on grid. Null on error or if node position not on grid. </summary>
		public virtual T NodeAt<T>(int x, int y)
			where T : MapNavNode
		{
			return null;
		}

		/// <summary> Return node that is at the given world position. Return -1 on error or if not valid node in that position. </summary>
		public virtual int NodeIdxFromWorldPosition(Vector3 pos)
		{
			return -1;
		}

		/// <summary> Return node that is at the given world position. Return null on error or if not valid node in that position. </summary>
		public virtual T NodeAtWorldPosition<T>(Vector3 pos)
			where T : MapNavNode
		{
			int idx = NodeIdxFromWorldPosition(pos);
			return (idx >= 0 ? (T)grid[idx] : null);
		}

		/// <summary> Return True if the given index is on the grid and is valid </summary>
		public virtual bool ValidIDX(int idx)
		{
			if (idx < 0 || idx >= grid.Length) return false;
			return grid[idx].isValid;
		}

		#endregion
		// ------------------------------------------------------------------------------------------------------------
		#region pub

		/// <summary> Returns list of nodes starting at the "next" node and going anti-clockwise around the central node. </summary>
		/// <param name="node">				The central node around which to get neighbouring nodes.</param>
		/// <param name="includeInvalid">	Should "invalid" nodes be included? If so then a NULL entry will be added to the
		///									returned list for invalid nodes. An invalid node might be one that is stored in 
		///									the grid array but not considered to be in the grid. This normally happens with
		///									Hexa grids. An invalid node might also be one marked as invalid by the callback function. </param>
		/// <param name="includeCentralNode">Should the central node be included? It will be added first in the list if so.</param>
		/// <param name="callback">			An optional callback that can first check if the node is "valid" and return True if 
		///									so, else it should return False. </param>
		/// <returns> Returns null if there was an error. </returns>
		public virtual List<T> NodesAround<T>(MapNavNode node, bool includeInvalid, bool includeCentralNode, ValidationCallback callback)
			where T : MapNavNode
		{
			return null;
		}

		/// <summary> Return list of nodes in a certain range around given node.
		/// The returned list is in no specific order. Excludes invalid nodes.
		/// </summary>
		/// <param name="node">				The central node around which to get neighbouring nodes.</param>
		/// <param name="range">			The radius around the central node. </param>
		/// <param name="includeCentralNode">Should the central node be included? It will be added first in the list if so.</param>
		/// <param name="callback">			An optional callback that can first check if the node is "valid" and return True if 
		///									so, else it should return False. </param>
		/// <returns>Returns null if there was an error. </returns>
		public virtual List<T> NodesAround<T>(MapNavNode node, int range, bool includeCentralNode, ValidationCallback callback)
			where T : MapNavNode
		{
			return null;
		}

		/// <summary> This returns a list of nodes around the given node, using the notion of "movement costs" to determine
		/// if a node should be included in the returned list or not. The callback can be used to specify the "cost" to 
		/// reach the specified node, making this useful to select the nodes that a unit might be able to move to. </summary>
		/// <param name="node"> The central node. </param>
		/// <param name="radius"> The maximum area around the node to select nodes from. </param>
		/// <param name="callback"> An optional callback (pass null to not use it) that can be used to indicate the cost of
		/// moving from one node to another. By default it will "cost" 1 to move from one node to another. By returning 0 in 
		/// this callback you can indicate that the target node can't be moved to (for example when the tile is occupied).
		/// Return a value higher than one (like 2 or 3) if moving to the target node would cost more and potentially
		/// exclude the node from the returned list of nodes (when cost to reach it would be bigger than "radius"). </param>
		/// <returns> Returns a list of nodes that can be used with grid[]. Returns empty list (not null) if there was an error. </returns>
		public virtual List<T> NodesAround<T>(MapNavNode node, int radius, NodeCostCallback callback)
			where T : MapNavNode
		{
			List<int> accepted = NodeIndicesAround(node.idx, radius, callback);
			if (accepted.Count > 0)
			{
				List<T> res = new List<T>();
				for (int i = 0; i < accepted.Count; i++) res.Add((T)grid[accepted[i]]);
				return res;
			}
			return new List<T>(0);
		}

		/// <summary> Return a list of nodes in a specified range around the given node.
		/// The returned list is in no specific order. Excludes invalid nodes. </summary>
		/// <param name="node">		The central node. </param>
		/// <param name="radius">	Radius from central node that ring starts. </param>
		/// <param name="width">	Width of the ring. </param>
		/// <param name="callback">	An optional callback that can first check if the node is "valid" and 
		///							return True if so, else it should return False. </param>
		/// <returns>Returns null if there was an error. </returns>
		public virtual List<T> NodesRing<T>(MapNavNode node, int radius, int width, ValidationCallback callback)
			where T : MapNavNode
		{
			if (radius < 1) radius = 1;
			if (width < 1) width = 1;

			List<T> all = NodesAround<T>(node, radius + (width - 1), false, callback);
			List<T> inner = radius > 1 ? NodesAround<T>(node, radius - 1, false, callback) : new List<T>();
			if (all == null || inner == null) return null;

			for (int i = 0; i < inner.Count; i++)
			{
				if (all.Contains(inner[i])) all.Remove(inner[i]);
			}

			return all;
		}

		/// <summary> Return a list of nodes that are considered to be on the border of a range of nodes that 
		/// where provided. A border node is a node that is present in the list but for which one of its
		/// neighbour nodes are not in the list or is an invalid node.  This is useful when trying to find
		/// the border nodes for a range of valid nodes a unit can move to. </summary>
		/// <param name="nodes">	List of nodes considered to be valid and from which the border nodes must be determined. </param>
		/// <param name="callback">	An optional callback that is used to determine if a neighbour node is valid. </param>
		/// <returns>Returns null if there was an error. </returns>
		public virtual List<T> GetBorderNodes<T>(List<T> nodes, ValidationCallback callback)
			where T : MapNavNode
		{
			List<T> res = new List<T>();
			for (int i = 0; i < nodes.Count; i++)
			{
				List<T> nn = NodesAround<T>(nodes[i], true, false, callback);
				for (int j = 0; j < nn.Count; j++)
				{	
					// is a border when there is an invalid tile next to it
					if (nn[j] == null || false == nodes.Contains(nn[j]))
					{	
						res.Add(nodes[i]);
						break;
					}
				}
			}
			return res;
		}

		// ------------------------------------------------------------------------------------------------------------

		/// <summary> Returns list of nodes indices starting at the "next" node and going anti-clockwise around the central node. </summary>
		/// <param name="idx">				Index of the central node around which to get neighbouring nodes.</param>
		/// <param name="includeInvalid">	Should "invalid" nodes be included? If so then a -1 entry will be added to the
		///									returned list for invalid nodes. An invalid node might be one that is stored in 
		///									the grid array but not considered to be in the grid. This normally happens with
		///									Hexa grids. An invalid node might also be one marked as invalid by the callback function. </param>
		/// <param name="includeCentralNode">Should the central node be included? It will be added first in the list if so.</param>
		/// <param name="callback">			An optional callback that can first check if the node is "valid" and return True if 
		///									so, else it should return False. </param>
		/// <returns>Returns null if there was an error. </returns>
		public virtual List<int> NodeIndicesAround(int idx, bool includeInvalid, bool includeCentralNode, ValidationCallback callback)
		{
			return null;
		}

		/// <summary> Return list of indices for nodes in a certain range around given node.
		/// The returned list is in no specific order. Excludes invalid nodes. </summary>
		/// <param name="idx">				Index of the central node around which to get neighbouring nodes.</param>
		/// <param name="range">			The radius around the central node. </param>
		/// <param name="includeCentralNode">Should the central node be included? It will be added first in the list if so.</param>
		/// <param name="callback">			An optional callback that can first check if the node is "valid" and return True if 
		///									so, else it should return False. </param>
		/// <returns>Returns null if there was an error. </returns>
		public virtual List<int> NodeIndicesAround(int idx, int range, bool includeCentralNode, ValidationCallback callback)
		{
			return null;
		}

		/// <summary> This returns a list of nodes around the given node, using the notion of "movement costs" to determine
		/// if a node should be included in the returned list or not. The callback can be used to specify the "cost" to 
		/// reach the specified node, making this useful to select the nodes that a unit might be able to move to. </summary>
		/// <param name="nodeIdx"> The central node's index. </param>
		/// <param name="radius"> The maximum area around the node to select nodes from. </param>
		/// <param name="callback"> An optional callback (pass null to not use it) that can be used to indicate the cost of
		/// moving from one node to another. By default it will "cost" 1 to move from one node to another. By returning 0 in 
		/// this callback you can indicate that the target node can't be moved to (for example when the tile is occupied).
		/// Return a value higher than one (like 2 or 3) if moving to the target node would cost more and potentially
		/// exclude the node from the returned list of nodes (when cost to reach it would be bigger than "radius"). </param>
		/// <returns> Returns a list of node indices that can be used with grid[]. Returns empty list (not null) if there was an error. </returns>
		public virtual List<int> NodeIndicesAround(int nodeIdx, int radius, NodeCostCallback callback)
		{
			List<int> accepted = new List<int>(); // accepted nodes
			Dictionary<int, float> costs = new Dictionary<int, float>(); // <idx, cost> - used to track which nodes have been checked
			CheckNodesRecursive(nodeIdx, radius, callback, -1, 0, ref accepted, ref costs);
			return accepted;
		}

		/// <summary> Return a list of node indices in a specified range around the given node.
		/// The returned list is in no specific order. Excludes invalid nodes. </summary>
		/// <param name="idx">		The central node index. </param>
		/// <param name="radius">	Radius from central node that ring starts. </param>
		/// <param name="width">	Width of the ring. </param>
		/// <param name="callback">	An optional callback that can first check if the node is "valid" and 
		///							return True if so, else it should return False. </param>
		/// <returns>Returns null if there was an error. </returns>
		public virtual List<int> NodeIndicesRing(int idx, int radius, int width, ValidationCallback callback)
		{
			if (radius < 1) radius = 1;
			if (width < 1) width = 1;

			List<int> all = NodeIndicesAround(idx, radius + (width - 1), false, callback);
			List<int> inner = radius > 1 ? NodeIndicesAround(idx, radius - 1, false, callback) : new List<int>();
			if (all == null || inner == null) return null;

			for (int i = 0; i < inner.Count; i++)
			{
				if (all.Contains(inner[i])) all.Remove(inner[i]);
			}

			return all;
		}

		// ------------------------------------------------------------------------------------------------------------

		/// <summary> Returns a list of node indices that represents a path from one node to another. An A* algorithm
		/// is used to calculate the path. Return an empty list on error or if the destination node can't be reached. </summary>
		/// <param name="startIdx">	The node where the path should start. </param>
		/// <param name="endIdx">	The node to reach. </param>
		/// <param name="callback"> An optional callback that can return an integer value to indicate the 
		///							cost of moving onto the specified node. This callback should return 1
		///							for normal nodes and 2+ for higher cost to move onto the node and 0
		///							if the node can't be moved onto; for example when the node is occupied. </param>
		/// <returns> Return an empty list on error or if the destination node can't be reached. </returns>
		public virtual List<int> Path(int startIdx, int endIdx, NodeCostCallback callback)
		{
			if (!ValidIDX(startIdx) || !ValidIDX(endIdx)) return new List<int>(0);
			if (startIdx == endIdx) return new List<int>(0);

			List<int> path = new List<int>();
			int current = -1;
			int next = -1;
			float new_cost = 0;
			float next_cost = 0;
			double priority = 0;

			// first check if not direct neighbour and get out early
			List<int> neighbors = PathNodeIndicesAround(startIdx);
			if (neighbors != null)
			{
				if (neighbors.Contains(endIdx))
				{
					if (callback != null)
					{
						next_cost = callback(grid[startIdx], grid[endIdx]);
						if (next_cost >= 1) path.Add(endIdx);
					}
					return path;
				}
			}

			HeapPriorityQueue<PriorityQueueNode> frontier = new HeapPriorityQueue<PriorityQueueNode>(grid.Length);
			frontier.Enqueue(new PriorityQueueNode() { idx = startIdx }, 0);
			Dictionary<int, int> came_from = new Dictionary<int, int>(); // <for_idx, came_from_idx>
			Dictionary<int, float> cost_so_far = new Dictionary<int, float>();	// <idx, cost>
			came_from.Add(startIdx, -1);
			cost_so_far.Add(startIdx, 0);

			while (frontier.Count > 0)
			{
				current = frontier.Dequeue().idx;
				if (current == endIdx) break;

				neighbors = PathNodeIndicesAround(current); // NodeIndicesAround(current, false, false, null);
				if (neighbors != null)
				{
					for (int i = 0; i < neighbors.Count; i++)
					{
						next = neighbors[i];
						if (callback != null) next_cost = callback(grid[current], grid[next]);
						if (next_cost <= 0.0f) continue;

						new_cost = cost_so_far[current] + next_cost;
						if (false == cost_so_far.ContainsKey(next)) cost_so_far.Add(next, new_cost + 1);
						if (new_cost < cost_so_far[next])
						{
							cost_so_far[next] = new_cost;
							priority = new_cost + Heuristic(startIdx, endIdx, next);
							frontier.Enqueue(new PriorityQueueNode() { idx = next }, priority);
							if (false == came_from.ContainsKey(next)) came_from.Add(next, current);
							else came_from[next] = current;
						}
					}
				}
			}

			// build path
			next = endIdx;
			while (came_from.ContainsKey(next))
			{
				if (came_from[next] == -1) break;
				if (came_from[next] == startIdx) break;
				path.Add(came_from[next]);
				next = came_from[next];
			}

			if (path.Count > 0)
			{
				path.Reverse();
				path.Add(endIdx);
			}

			return path;
		}

		/// <summary> Returns a list of nodes that represents a path from one node to another. An A* algorithm is used
		/// to calculate the path. Return an empty list on error or if the destination node can't be reached. </summary>
		/// <param name="start">	The node where the path should start. </param>
		/// <param name="end">		The node to reach. </param>
		/// <param name="callback"> An optional callback that can return an integer value to indicate the 
		///							cost of moving onto the specified node. This callback should return 1
		///							for normal nodes and 2+ for higher cost to move onto the node and 0
		///							if the node can't be moved onto; for example when the node is occupied. </param>
		/// <returns> Return an empty list on error or if the destination node can't be reached. </returns>
		public virtual List<T> Path<T>(MapNavNode start, MapNavNode end, NodeCostCallback callback)
			where T : MapNavNode
		{
			if (start == null || end == null) return new List<T>(0);
			if (start.idx == end.idx) return new List<T>(0);

			List<T> path = new List<T>();
			int current = -1;
			int next = -1;
			float new_cost = 0;
			float next_cost = 0;
			double priority = 0;

			// first check if not direct neighbour and get out early
			List<int> neighbors = PathNodeIndicesAround(start.idx); // NodeIndicesAround(start.idx, false, false, null);
			if (neighbors != null)
			{
				if (neighbors.Contains(end.idx))
				{
					if (callback != null)
					{
						next_cost = callback(start, end);
						if (next_cost >= 1) path.Add((T)end);
					}
					return path;
				}
			}

			HeapPriorityQueue<PriorityQueueNode> frontier = new HeapPriorityQueue<PriorityQueueNode>(grid.Length);
			frontier.Enqueue(new PriorityQueueNode() { idx = start.idx }, 0);
			Dictionary<int, int> came_from = new Dictionary<int, int>(); // <for_idx, came_from_idx>
			Dictionary<int, float> cost_so_far = new Dictionary<int, float>(); // <idx, cost>
			came_from.Add(start.idx, -1);
			cost_so_far.Add(start.idx, 0);

			while (frontier.Count > 0)
			{
				current = frontier.Dequeue().idx;
				if (current == end.idx) break;

				neighbors = PathNodeIndicesAround(current); //NodeIndicesAround(current, false, false, null);
				if (neighbors != null)
				{
					for (int i = 0; i < neighbors.Count; i++)
					{
						next = neighbors[i];
						if (callback != null) next_cost = callback(grid[current], grid[next]);
						if (next_cost <= 0.0f) continue;

						new_cost = cost_so_far[current] + next_cost;
						if (false == cost_so_far.ContainsKey(next)) cost_so_far.Add(next, new_cost + 1);
						if (new_cost < cost_so_far[next])
						{
							cost_so_far[next] = new_cost;
							priority = new_cost + Heuristic(start.idx, end.idx, next);
							frontier.Enqueue(new PriorityQueueNode() { idx = next }, priority);
							if (false == came_from.ContainsKey(next)) came_from.Add(next, current);
							else came_from[next] = current;
						}
					}
				}
			}

			// build path
			next = end.idx;
			while (came_from.ContainsKey(next))
			{
				if (came_from[next] == -1) break;
				if (came_from[next] == start.idx) break;
				path.Add((T)grid[came_from[next]]);
				next = came_from[next];
			}

			if (path.Count > 0)
			{
				path.Reverse();
				path.Add((T)end);
			}

			return path;
		}

		// ------------------------------------------------------------------------------------------------------------

		/// <summary> Returns the distance from node 1 to node 2. Returns 0 on error. </summary>
		public virtual int Distance(MapNavNode node1, MapNavNode node2)
		{
			if (node1 == null || node2 == null) return 0;
			return Distance(node1.idx, node2.idx);
		}

		/// <summary> Returns the distance from node 1 to node 2. </summary>
		public virtual int Distance(int idx1, int idx2)
		{
			return 0;
		}

		#endregion
		// ------------------------------------------------------------------------------------------------------------
		#region helpers

		protected virtual List<int> PathNodeIndicesAround(int idx)
		{
			return null;
		}

		/// <summary> This function is a helper for the Path function </summary>
		protected virtual float Heuristic(int startIdx, int endIdx, int idx)
		{
			//return Vector2.Distance(new Vector2(grid[idx].q, grid[idx].r), new Vector2(grid[endIdx].q, grid[endIdx].r));

			//return Distance(idx, endIdx);

			// this method seems to be the best for now as it give relatively "straight line" paths
			int dx1 = grid[idx].q - grid[endIdx].q;
			int dy1 = grid[idx].r - grid[endIdx].r;
			int dx2 = grid[startIdx].q - grid[endIdx].q;
			int dy2 = grid[startIdx].r - grid[endIdx].r;
			int cross = Mathf.Abs(dx1 * dy2 - dx2 * dy1);
			return Distance(idx, endIdx) + (cross * 0.001f);
		}

		/// <summary> This is a Helper for NodeIndicesAround(int idx, int radius, bool includeCentralNode, ValidationCallback callback) </summary>
		protected virtual void CheckNodesRecursive(int idx, int radius, NodeCostCallback callback, int cameFrom, float currDepth, ref List<int> accepted, ref Dictionary<int, float> costs)
		{
			List<int> ids = _neighbours(idx);
			for (int i = 0; i < ids.Count; i++)
			{
				// skip if came into this function from this node. no point in testing
				// against the node that caused this one to be called for checking
				if (cameFrom == ids[i]) continue;

				// get cost to move to the node
				float res = callback == null ? 1f : callback(grid[idx], grid[ids[i]]);

				// can move to node?
				if (res <= 0.0f) continue;

				// how much would it cost in total?
				float d = currDepth + res;

				// too much to reach node?
				if (d > radius) continue;

				// this neighbour node can be moved to, add it to the accepted list if not yet present
				if (false == accepted.Contains(ids[i])) accepted.Add(ids[i]);

				// do not bother to check the node's neighbours if already reached the max 
				if (d == radius) continue;

				// check if should look into the neighbours of this node
				if (costs.ContainsKey(ids[i]))
				{
					// if the new total cost is higher than previously checked then skip this neighbour 
					// since testing with the higher costs will not change any results when checking 
					// the neighbours of this neighbour node
					if (costs[ids[i]] <= d) continue;
				}
				else costs.Add(ids[i], d);

				// update the cost to move to this node
				costs[ids[i]] = d;

				// and test its neighbours for possible valid nodes
				CheckNodesRecursive(ids[i], radius, callback, idx, d, ref accepted, ref costs);
			}
		}

		/// <summary> This is a helper for CheckNodesRecursive() </summary>
		protected virtual List<int> _neighbours(int idx)
		{
			return new List<int>(0);
		}

		#endregion
		// ------------------------------------------------------------------------------------------------------------
		#region callbacks

		/// <summary> Override this to respond when the grid was changed via functions like CreateGrid and SmoothOut. </summary>
		/// <param name="created"> Will be True if was CreateGrid, else False for all other. </param>
		public virtual void OnGridChanged(bool created) { }

		/// <summary> Override this for notification while the while a grid is being created. The function is called for 
		/// each Node being added to the node. You could use this to add your own initialization data to a custom node 
		/// being created via CreateGrid[T]() </summary>
		/// <param name="node"> The node that was created. </param>
		public virtual void OnNodeCreated(MapNavNode node) { }

		#endregion
		// ------------------------------------------------------------------------------------------------------------
	}
}