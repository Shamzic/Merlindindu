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
	/// A MapNav grid consisting of hexagon nodes.
	/// </summary>
	public class MapNavHexa : MapNavBase
	{
		// ------------------------------------------------------------------------------------------------------------
		#region definitions

		/// <summary> How the grid is laid out. </summary>
		public enum CoordinateSystem
		{
			/// <summary> Square shaped grid with off-offset for each node. </summary>
			OddOffset,
			/// <summary> Square shaped grid with even-offset for each node. </summary>
			EvenOffset,
			/// <summary> Hexagonal/ roundish shaped grid. </summary>
			Axial
		}

		/// <summary> Orientation of the nodes in the grid. </summary>
		public enum GridOrientation
		{
			/// <summary> The nodes are rotated with the flat side towards the "top" of the grid. </summary>
			FlatTop,

			/// <summary> The nodes are rotated with the pointy side towards the "top" of the grid. </summary>
			PointyTop
		}

		private static int[,] AxialNeighbours = new int[,] { { +1, 0 }, { +1, -1 }, { 0, -1 }, { -1, 0 }, { -1, +1 }, { 0, +1 } };
		private static int[][,] PointyOddNeighbours = new int[][,] {	new int[,] { {+1,  0}, { 0, -1}, {-1, -1}, {-1,  0}, {-1, +1}, { 0, +1} }, 
																		new int[,] { {+1,  0}, {+1, -1}, { 0, -1}, {-1,  0}, { 0, +1}, {+1, +1} } };
		private static int[][,] PointyEvenNeighbours = new int[][,] {	new int[,] { {+1,  0}, {+1, -1}, { 0, -1}, {-1,  0}, { 0, +1}, {+1, +1} }, 
																		new int[,] { {+1,  0}, { 0, -1}, {-1, -1}, {-1,  0}, {-1, +1}, { 0, +1} } };
		private static int[][,] FlatOddNeighbours = new int[][,] {		new int[,] { {+1,  0}, {+1, -1}, { 0, -1}, {-1, -1}, {-1,  0}, { 0, +1} }, 
																		new int[,] { {+1, +1}, {+1,  0}, { 0, -1}, {-1,  0}, {-1, +1}, { 0, +1} } };
		private static int[][,] FlatEvenNeighbours = new int[][,] {	new int[,] { {+1, +1}, {+1,  0}, { 0, -1}, {-1,  0}, {-1, +1}, { 0, +1} }, 
																		new int[,] { {+1,  0}, {+1, -1}, { 0, -1}, {-1, -1}, {-1,  0}, { 0, +1} } };
		#endregion
		// ------------------------------------------------------------------------------------------------------------
		#region properties

		public CoordinateSystem coordSystem = CoordinateSystem.Axial;
		public GridOrientation gridOrientation = GridOrientation.FlatTop;

		#endregion
		// ------------------------------------------------------------------------------------------------------------
		#region grid manipulation

		/// <summary> Destroy Grid array and creates a new one from properties. </summary>
		/// <typeparam name="T"> You can specify a custom Node class derived from MapNavNode </typeparam>
		public override void CreateGrid<T>()
		{
			if (coordSystem == CoordinateSystem.Axial)
			{
				if (mapHorizontalSize % 2 == 0)
				{
					mapHorizontalSize--;
					Debug.LogWarning("Map size should be odd numbers in Axial mode, changing mapHorizontalSize to: " + mapHorizontalSize);
				}
				if (mapVerticalSize % 2 == 0)
				{
					mapVerticalSize--;
					Debug.LogWarning("Map size should be odd numbers in Axial mode, changing mapVerticalSize to: " + mapVerticalSize);
				}
			}

			grid = new T[mapHorizontalSize * mapVerticalSize];

			float width, length, hori, vert, offs;
			if (gridOrientation == GridOrientation.FlatTop)
			{
				width = nodeSize * 2f;
				length = Mathf.Sqrt(3f) * 0.5f * width;
				hori = 0.75f * width;
				vert = length;
				offs = vert * 0.5f;
			}
			else
			{
				length = nodeSize * 2f;
				width = Mathf.Sqrt(3f) * 0.5f * length;
				vert = 0.75f * length;
				hori = width;
				offs = hori * 0.5f;
			}

			// Axial
			if (coordSystem == CoordinateSystem.Axial)
			{
				int idx = -1;
				float px, py;
				int startX = -(mapHorizontalSize / 2);
				int endX = mapHorizontalSize + startX;
				int startY = -(mapVerticalSize / 2);
				int endY = mapVerticalSize + startY;

				for (int y = startY; y < endY; y++)
				{
					for (int x = startX; x < endX; x++)
					{
						idx++;
						grid[idx] = ScriptableObject.CreateInstance<T>();

						if ((x + y) >= endX || (x + y) < startX)
						{	// do not set parent, making this node invalid
							grid[idx].idx = -1;
							grid[idx].parent = null;
							continue;
						}

						grid[idx].idx = idx;
						grid[idx].q = x;
						grid[idx].r = y;
						grid[idx].parent = transform;
						grid[idx].h = (nodeHeightOpt == NodeHeightOpt.Flat ? minNodeHeight : Random.Range(minNodeHeight, maxNodeHeight + 1));

						if (gridOrientation == GridOrientation.PointyTop)
						{
							px = nodeSize * Mathf.Sqrt(3) * (x + y * 0.5f);
							py = nodeSize * 1.5f * y;
						}
						else
						{
							px = nodeSize * 1.5f * x;
							py = nodeSize * Mathf.Sqrt(3) * (y + x * 0.5f);
						}

						grid[idx].localPosition = new Vector3(px, nodeHeightStep * grid[idx].h, py);

						OnNodeCreated(grid[idx]);
					}
				}
			}

			// Rectangle
			else
			{

				Vector3 posOffs = new Vector3(-mapHorizontalSize * nodeSize / 2f, 0f, -mapVerticalSize * nodeSize / 2f) + new Vector3(nodeSize / 2f, 0f, nodeSize / 2f);

				if (gridOrientation == GridOrientation.PointyTop)
				{
					posOffs.x = posOffs.x * Mathf.Sqrt(3f);
					posOffs.z = posOffs.z * 1.5f;
				}
				else
				{
					posOffs.x = posOffs.x * 1.5f;
					posOffs.z = posOffs.z * Mathf.Sqrt(3f);
				}

				int idx = -1;
				for (int z = 0; z < mapVerticalSize; z++)
				{
					for (int x = 0; x < mapHorizontalSize; x++)
					{
						idx++;
						grid[idx] = ScriptableObject.CreateInstance<T>();
						grid[idx].idx = idx;
						grid[idx].q = x;
						grid[idx].r = z;
						grid[idx].parent = transform;
						grid[idx].h = (nodeHeightOpt == NodeHeightOpt.Flat ? minNodeHeight : Random.Range(minNodeHeight, maxNodeHeight + 1));

						if (gridOrientation == GridOrientation.FlatTop)
						{
							grid[idx].localPosition = new Vector3(hori * x, nodeHeightStep * grid[idx].h, vert * z +
								(x % 2 == 0 ?
									(coordSystem == CoordinateSystem.EvenOffset ? offs : 0f) :
									(coordSystem == CoordinateSystem.EvenOffset ? 0f : offs))
								);
						}
						else
						{
							grid[idx].localPosition = new Vector3(hori * x +
								(z % 2 != 0 ?
									(coordSystem == CoordinateSystem.EvenOffset ? 0f : offs) :
									(coordSystem == CoordinateSystem.EvenOffset ? offs : 0f)),
								nodeHeightStep * grid[idx].h, vert * z);
						}

						grid[idx].localPosition += posOffs;

						OnNodeCreated(grid[idx]);
					}
				}
			}

			OnGridChanged(true);
		}

		/// <summary> Destroy Grid array and creates a new one from properties. </summary>
		/// <param name="nodeType"> You can specify a custom Node class derived from MapNavNode </param>
		public override void CreateGrid(System.Type nodeType)
		{
			if (coordSystem == CoordinateSystem.Axial)
			{
				if (mapHorizontalSize % 2 == 0)
				{
					mapHorizontalSize--;
					Debug.LogWarning("Map size should be odd numbers in Axial mode, changing mapHorizontalSize to: " + mapHorizontalSize);
				}
				if (mapVerticalSize % 2 == 0)
				{
					mapVerticalSize--;
					Debug.LogWarning("Map size should be odd numbers in Axial mode, changing mapVerticalSize to: " + mapVerticalSize);
				}
			}

			grid = new MapNavNode[mapHorizontalSize * mapVerticalSize];

			float width, length, hori, vert, offs;
			if (gridOrientation == GridOrientation.FlatTop)
			{
				width = nodeSize * 2f;
				length = Mathf.Sqrt(3f) * 0.5f * width;
				hori = 0.75f * width;
				vert = length;
				offs = vert * 0.5f;
			}
			else
			{
				length = nodeSize * 2f;
				width = Mathf.Sqrt(3f) * 0.5f * length;
				vert = 0.75f * length;
				hori = width;
				offs = hori * 0.5f;
			}

			// Axial
			if (coordSystem == CoordinateSystem.Axial)
			{
				int idx = -1;
				float px, py;
				int startX = -(mapHorizontalSize / 2);
				int endX = mapHorizontalSize + startX;
				int startY = -(mapVerticalSize / 2);
				int endY = mapVerticalSize + startY;

				for (int y = startY; y < endY; y++)
				{
					for (int x = startX; x < endX; x++)
					{
						idx++;
						grid[idx] = (MapNavNode)ScriptableObject.CreateInstance(nodeType);

						if ((x + y) >= endX || (x + y) < startX)
						{	// do not set parent, making this node invalid
							grid[idx].idx = -1;
							grid[idx].parent = null;
							continue;
						}

						grid[idx].idx = idx;
						grid[idx].q = x;
						grid[idx].r = y;
						grid[idx].parent = transform;
						grid[idx].h = (nodeHeightOpt == NodeHeightOpt.Flat ? minNodeHeight : Random.Range(minNodeHeight, maxNodeHeight + 1));

						if (gridOrientation == GridOrientation.PointyTop)
						{
							px = nodeSize * Mathf.Sqrt(3) * (x + y * 0.5f);
							py = nodeSize * 1.5f * y;
						}
						else
						{
							px = nodeSize * 1.5f * x;
							py = nodeSize * Mathf.Sqrt(3) * (y + x * 0.5f);
						}

						grid[idx].localPosition = new Vector3(px, nodeHeightStep * grid[idx].h, py);

						OnNodeCreated(grid[idx]);
					}
				}
			}

			// Rectangle
			else
			{

				Vector3 posOffs = new Vector3(-mapHorizontalSize * nodeSize / 2f, 0f, -mapVerticalSize * nodeSize / 2f) + new Vector3(nodeSize / 2f, 0f, nodeSize / 2f);

				if (gridOrientation == GridOrientation.PointyTop)
				{
					posOffs.x = posOffs.x * Mathf.Sqrt(3f);
					posOffs.z = posOffs.z * 1.5f;
				}
				else
				{
					posOffs.x = posOffs.x * 1.5f;
					posOffs.z = posOffs.z * Mathf.Sqrt(3f);
				}

				int idx = -1;
				for (int z = 0; z < mapVerticalSize; z++)
				{
					for (int x = 0; x < mapHorizontalSize; x++)
					{
						idx++;
						grid[idx] = (MapNavNode)ScriptableObject.CreateInstance(nodeType);
						grid[idx].idx = idx;
						grid[idx].q = x;
						grid[idx].r = z;
						grid[idx].parent = transform;
						grid[idx].h = (nodeHeightOpt == NodeHeightOpt.Flat ? minNodeHeight : Random.Range(minNodeHeight, maxNodeHeight + 1));

						if (gridOrientation == GridOrientation.FlatTop)
						{
							grid[idx].localPosition = new Vector3(hori * x, nodeHeightStep * grid[idx].h, vert * z +
								(x % 2 == 0 ?
									(coordSystem == CoordinateSystem.EvenOffset ? offs : 0f) :
									(coordSystem == CoordinateSystem.EvenOffset ? 0f : offs))
								);
						}
						else
						{
							grid[idx].localPosition = new Vector3(hori * x +
								(z % 2 != 0 ?
									(coordSystem == CoordinateSystem.EvenOffset ? 0f : offs) :
									(coordSystem == CoordinateSystem.EvenOffset ? offs : 0f)),
								nodeHeightStep * grid[idx].h, vert * z);
						}

						grid[idx].localPosition += posOffs;

						OnNodeCreated(grid[idx]);
					}
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
						float angle_prec = 2f * Mathf.PI / 6f;
						float[] yValues = new float[6];
						for (int i = 0; i < 6; i++)
						{
							float angle = angle_prec * (gridOrientation == GridOrientation.FlatTop ? (float)i : ((float)i + 0.5f));
							p.x = grid[idx].position.x + nodeSize * Mathf.Cos(angle);
							p.z = grid[idx].position.z + nodeSize * Mathf.Sin(angle);
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
			int idx = -1;
			if (coordSystem == CoordinateSystem.Axial)
			{
				// 0, 0 is offset to be at center of grid
				idx = ((r + (mapVerticalSize / 2)) * mapHorizontalSize + (q + (mapHorizontalSize / 2)));
			}
			else
			{
				idx = (r * mapHorizontalSize + q);
			}

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
			int idx = -1;
			if (coordSystem == CoordinateSystem.Axial)
			{
				// 0, 0 is offset to be at centre of grid
				idx = ((r + (mapVerticalSize / 2)) * mapHorizontalSize + (q + (mapHorizontalSize / 2)));
			}
			else
			{
				idx = (r * mapHorizontalSize + q);
			}

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
			int idx = -1;
			pos = pos - transform.position; // work in local coords for grid

			// calculate in axial coords
			if (coordSystem == CoordinateSystem.Axial)
			{
				float q, r;
				if (gridOrientation == GridOrientation.FlatTop)
				{
					q = 2f / 3f * pos.x / nodeSize;
					r = (-1f / 3f * pos.x + 1f / 3f * Mathf.Sqrt(3f) * pos.z) / nodeSize;
				}
				else
				{
					q = (1f / 3f * Mathf.Sqrt(3f) * pos.x - 1f / 3f * pos.z) / nodeSize;
					r = 2f / 3f * pos.z / nodeSize;
				}

				// hex_round works in cube coords - convert axial to cube
				Vector3 cube = hex_round(q, -q - r, r);
				idx = NodeIdx((int)cube.x, (int)cube.z);
				if (false == ValidIDX(idx)) return -1;
			}

			// odd/ even offset is square layout so I will do a simpler calculation
			// to find and estimated position and then check neighbours for precision
			else
			{
				int q, r;
				Vector3 posOffs = new Vector3(-mapHorizontalSize * nodeSize / 2f, 0f, -mapVerticalSize * nodeSize / 2f) + new Vector3(nodeSize / 2f, 0f, nodeSize / 2f);
				float width, length, hori, vert, offs;
				if (gridOrientation == GridOrientation.FlatTop)
				{
					posOffs.x = posOffs.x * 1.5f;
					posOffs.z = posOffs.z * Mathf.Sqrt(3f);
					width = nodeSize * 2f;
					length = Mathf.Sqrt(3f) * 0.5f * width;
					hori = 0.75f * width;
					vert = length;
					offs = vert * 0.5f;
					Vector3 pos2 = pos - posOffs;
					q = Mathf.RoundToInt(pos2.x / hori);
					pos2.z -= (q % 2 == 0 ? (coordSystem == CoordinateSystem.EvenOffset ? offs : 0f) : (coordSystem == CoordinateSystem.EvenOffset ? 0f : offs));
					r = Mathf.RoundToInt(pos2.z / vert);
				}
				else
				{
					posOffs.x = posOffs.x * Mathf.Sqrt(3f);
					posOffs.z = posOffs.z * 1.5f;
					length = nodeSize * 2f;
					width = Mathf.Sqrt(3f) * 0.5f * length;
					vert = 0.75f * length;
					hori = width;
					offs = hori * 0.5f;
					Vector3 pos2 = pos - posOffs;
					r = Mathf.RoundToInt(pos2.z / vert);
					pos2.x -= (r % 2 != 0 ? (coordSystem == CoordinateSystem.EvenOffset ? 0f : offs) : (coordSystem == CoordinateSystem.EvenOffset ? offs : 0f));
					q = Mathf.RoundToInt(pos2.x / hori);
				}

				idx = NodeIdx(q, r);
				if (false == ValidIDX(idx)) return -1;

				// the following will find all the neighbours of the node and check if one of them
				// are not closer to the point just in case the RoundToInt was not correct
				// Overall this does not seem to be needed so I commented it out
				//List<int> ids = NodeIndicesAround(idx, false, true, null);
				//if (ids.Count > 1)
				//{
				//	float dist = Vector3.Distance(grid[idx].localPosition, pos);
				//	for (int i = 1; i > ids.Count; i++)
				//	{
				//		float d = Vector3.Distance(grid[ids[i]].localPosition, pos);
				//		if (d < dist)
				//		{
				//			idx = ids[i];
				//			dist = d;
				//		}
				//	}
				//}

			}

			return idx;
		}

		/// <summary> Returns list of nodes starting at the "next" node and going anti-clockwise around the central node. </summary>
		/// <param name="node">				The central node around which to get neighbouring nodes.</param>
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

			int[,] neighbours = new int[0, 0];
			if (coordSystem == CoordinateSystem.Axial)
			{
				neighbours = AxialNeighbours;
			}
			else if (coordSystem == CoordinateSystem.OddOffset)
			{
				if (gridOrientation == GridOrientation.PointyTop) neighbours = PointyOddNeighbours[node.r & 1];
				else neighbours = FlatOddNeighbours[node.q & 1];
			}
			else if (coordSystem == CoordinateSystem.EvenOffset)
			{
				if (gridOrientation == GridOrientation.PointyTop) neighbours = PointyEvenNeighbours[node.r & 1];
				else neighbours = FlatEvenNeighbours[node.q & 1];
			}

			for (int dir = 0; dir < neighbours.GetLength(0); dir++)
			{
				int q = node.q + neighbours[dir, 0];
				int r = node.r + neighbours[dir, 1];
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

		/// <summary> Return list of nodes in a certain range around given node. The returned list is in no specific order. Excludes invalid nodes. </summary>
		/// <param name="node">				The central node around which to get neighbouring nodes.</param>
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

			int hH = mapHorizontalSize / 2;
			int hV = mapVerticalSize / 2;
			int q, r, z, id;
			for (int x = -radius; x <= radius; x++)
			{
				for (int y = Mathf.Max(-radius, (-x - radius)); y <= Mathf.Min(radius, -x + radius); y++)
				{
					z = -x - y;
					id = -1;
					if (coordSystem == CoordinateSystem.Axial)
					{
						//# convert cube to axial
						q = grid[node.idx].q + x;
						r = grid[node.idx].r + z;
						if (q < -hH || r < -hV || q > hH || r > hV) continue;
						id = ((r + (hV)) * mapHorizontalSize + (q + (hH)));
					}
					else
					{
						q = -1;
						r = -1;

						if (gridOrientation == GridOrientation.FlatTop)
						{
							int par = (grid[node.idx].q & 1);
							if (coordSystem == CoordinateSystem.EvenOffset) par = par == 1 ? 0 : 1;
							if (par == 1)
							{
								//# convert cube to even-q offset
								q = x;
								r = z + (x + (x & 1)) / 2;
							}
							else
							{
								//# convert cube to odd-q offset
								q = x;
								r = z + (x - (x & 1)) / 2;
							}
						}
						else
						{
							int par = grid[node.idx].r & 1;
							if (coordSystem == CoordinateSystem.EvenOffset) par = (par == 1 ? 0 : 1);
							if (par == 1)
							{
								//# convert cube to even-r offset
								q = x + (z + (z & 1)) / 2;
								r = z;
							}
							else
							{
								//# convert cube to odd-r offset
								q = x + (z - (z & 1)) / 2;
								r = z;
							}
						}

						q = grid[node.idx].q + q;
						r = grid[node.idx].r + r;
						if (q < 0 || r < 0 || q >= mapHorizontalSize || r >= mapVerticalSize) continue;
						id = (r * mapHorizontalSize + q);
					}

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
		public override List<int> NodeIndicesAround(int idx, bool includeInvalid, bool includeCentralNode, ValidationCallback callback)
		{
			List<int> nodes = new List<int>(includeCentralNode ? 7 : 6);
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

			int[,] neighbours = new int[0, 0];
			if (coordSystem == CoordinateSystem.Axial)
			{
				neighbours = AxialNeighbours;
			}
			else if (gridOrientation == GridOrientation.PointyTop)
			{
				if (coordSystem == CoordinateSystem.OddOffset) neighbours = PointyOddNeighbours[grid[idx].r & 1];
				else neighbours = PointyEvenNeighbours[grid[idx].r & 1];
			}
			else
			{
				if (coordSystem == CoordinateSystem.OddOffset) neighbours = FlatOddNeighbours[grid[idx].q & 1];
				else neighbours = FlatEvenNeighbours[grid[idx].q & 1];
			}

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

		/// <summary> Return list of indices for nodes in a certain range around given node. The returned list is in no specific order. Excludes invalid nodes. </summary>
		/// <param name="idx">				Index of the central node around which to get neighbouring nodes.</param>
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

			int hH = mapHorizontalSize / 2;
			int hV = mapVerticalSize / 2;
			for (int x = -radius; x <= radius; x++)
			{
				for (int y = Mathf.Max(-radius, (-x - radius)); y <= Mathf.Min(radius, -x + radius); y++)
				{
					int z = -x - y;
					int id = -1;
					if (coordSystem == CoordinateSystem.Axial)
					{
						//# convert cube to axial
						int q = grid[idx].q + x;
						int r = grid[idx].r + z;
						if (q < -hH || r < -hV || q > hH || r > hV) continue;
						id = ((r + hV) * mapHorizontalSize + (q + hH));
					}
					else
					{
						int q = -1;
						int r = -1;

						if (gridOrientation == GridOrientation.FlatTop)
						{
							int par = (grid[idx].q & 1);
							if (coordSystem == CoordinateSystem.EvenOffset) par = par == 1 ? 0 : 1;
							if (par == 1)
							{
								//# convert cube to even-q offset
								q = x;
								r = z + (x + (x & 1)) / 2;
							}
							else
							{
								//# convert cube to odd-q offset
								q = x;
								r = z + (x - (x & 1)) / 2;
							}
						}
						else
						{
							int par = grid[idx].r & 1;
							if (coordSystem == CoordinateSystem.EvenOffset) par = par == 1 ? 0 : 1;
							if (par == 1)
							{
								//# convert cube to even-r offset
								q = x + (z + (z & 1)) / 2;
								r = z;
							}
							else
							{
								//# convert cube to odd-r offset
								q = x + (z - (z & 1)) / 2;
								r = z;
							}
						}

						q = grid[idx].q + q;
						r = grid[idx].r + r;
						if (q < 0 || r < 0 || q >= mapHorizontalSize || r >= mapVerticalSize) continue;
						id = (r * mapHorizontalSize + q);
					}

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

		/// <summary> Returns the distance from node 1 to node 2. </summary>
		public override int Distance(int idx1, int idx2)
		{
			if (idx1 == idx2) return 0;
			if (coordSystem == CoordinateSystem.Axial)
			{
				return (Mathf.Abs(grid[idx1].q - grid[idx2].q) +
						Mathf.Abs(grid[idx1].r - grid[idx2].r) +
						Mathf.Abs(grid[idx1].q + grid[idx1].r - grid[idx2].q - grid[idx2].r)) / 2;
			}

			int x1 = 0, y1 = 0, z1 = 0;
			int x2 = 0, y2 = 0, z2 = 0;

			if (gridOrientation == GridOrientation.FlatTop)
			{
				if (coordSystem == CoordinateSystem.EvenOffset)
				{
					//# convert even-q offset to cube
					x1 = grid[idx1].q;
					z1 = grid[idx1].r - (grid[idx1].q + (grid[idx1].q & 1)) / 2;
					y1 = -x1 - z1;

					x2 = grid[idx2].q;
					z2 = grid[idx2].r - (grid[idx2].q + (grid[idx2].q & 1)) / 2;
					y2 = -x2 - z2;
				}
				else
				{
					//# convert odd-q offset to cube
					x1 = grid[idx1].q;
					z1 = grid[idx1].r - (grid[idx1].q - (grid[idx1].q & 1)) / 2;
					y1 = -x1 - z1;

					x2 = grid[idx2].q;
					z2 = grid[idx2].r - (grid[idx2].q - (grid[idx2].q & 1)) / 2;
					y2 = -x2 - z2;
				}
			}
			else
			{
				if (coordSystem == CoordinateSystem.EvenOffset)
				{
					//# convert even-r offset to cube
					x1 = grid[idx1].q - (grid[idx1].r + (grid[idx1].r & 1)) / 2;
					z1 = grid[idx1].r;
					y1 = -x1 - z1;

					x2 = grid[idx2].q - (grid[idx2].r + (grid[idx2].r & 1)) / 2;
					z2 = grid[idx2].r;
					y2 = -x2 - z2;
				}
				else
				{
					//# convert odd-r offset to cube
					x1 = grid[idx1].q - (grid[idx1].r - (grid[idx1].r & 1)) / 2;
					z1 = grid[idx1].r;
					y1 = -x1 - z1;

					x2 = grid[idx2].q - (grid[idx2].r - (grid[idx2].r & 1)) / 2;
					z2 = grid[idx2].r;
					y2 = -x2 - z2;
				}
			}

			return Mathf.Max(Mathf.Abs(x1 - x2), Mathf.Abs(y1 - y2), Mathf.Abs(z1 - z2));
		}

		#endregion
		// ------------------------------------------------------------------------------------------------------------
		#region helpers

		protected override List<int> PathNodeIndicesAround(int idx)
		{
			return NodeIndicesAround(idx, false, false, null);
		}

		/// <summary> This is a helper for CheckNodesRecursive() </summary>
		protected override List<int> _neighbours(int idx)
		{
			List<int> nodes = new List<int>();
			int[,] neighbours = new int[0, 0];
			if (coordSystem == CoordinateSystem.Axial)
			{
				neighbours = AxialNeighbours;
			}
			else if (gridOrientation == GridOrientation.PointyTop)
			{
				if (coordSystem == CoordinateSystem.OddOffset) neighbours = PointyOddNeighbours[grid[idx].r & 1];
				else neighbours = PointyEvenNeighbours[grid[idx].r & 1];
			}
			else
			{
				if (coordSystem == CoordinateSystem.OddOffset) neighbours = FlatOddNeighbours[grid[idx].q & 1];
				else neighbours = FlatEvenNeighbours[grid[idx].q & 1];
			}

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

		/// <summary> This is a helper for NodeIdxFromWorldPosition </summary>
		private Vector3 hex_round(float x, float y, float z)
		{
			float rx = Mathf.Round(x);
			float ry = Mathf.Round(y);
			float rz = Mathf.Round(z);

			float x_diff = Mathf.Abs(rx - x);
			float y_diff = Mathf.Abs(ry - y);
			float z_diff = Mathf.Abs(rz - z);

			if (x_diff > y_diff && x_diff > z_diff) rx = -ry - rz;
			else if (y_diff > z_diff) ry = -rx - rz;
			else rz = -rx - ry;

			return new Vector3(rx, ry, rz);
		}

		#endregion
		// ------------------------------------------------------------------------------------------------------------
		#region debug

		protected virtual void OnDrawGizmos()
		{
			if (_dev_show_nodes && grid.Length > 0)
			{
				Vector3 p1, p2;
				int idx = -1;
				float angle_prec = 2f * Mathf.PI / 6f;

				for (int y = 0; y < mapVerticalSize; y++)
				{
					for (int x = 0; x < mapHorizontalSize; x++)
					{
						idx++;
						if (idx >= grid.Length) break;
						if (!grid[idx].isValid && !grid[idx].foredInvalid) continue;
						Gizmos.color = grid[idx].foredInvalid ? Color.grey : Color.cyan;

						p1 = grid[idx].position;
						p2 = grid[idx].position;
						for (int i = 0; i < 7; i++)
						{
							float angle = angle_prec * (gridOrientation == GridOrientation.FlatTop ? (float)i : ((float)i + 0.5f));
							float xi = grid[idx].position.x + nodeSize * Mathf.Cos(angle);
							float zi = grid[idx].position.z + nodeSize * Mathf.Sin(angle);
							if (i == 0)
							{
								p1.x = xi;
								p1.z = zi;
							}
							else
							{
								p2 = new Vector3(xi, p2.y, zi);
								Gizmos.DrawLine(p1, p2);
								p1 = p2;
							}
						}
					}
					if (idx >= grid.Length) break;
				}
			}
		}

		#endregion
		// ------------------------------------------------------------------------------------------------------------
	}
}
