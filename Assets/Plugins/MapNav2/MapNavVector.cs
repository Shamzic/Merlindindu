// -= MapNav 2 =-
// www.plyoung.com
// by Leslie Young
// ====================================================================================================================

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace MapNavKit
{
	[System.Serializable]
	public struct MapNavVector
	{
		/// <summary> Position Q (column/x) in 2D grid. </summary>
		public int q;

		/// <summary> Position R (row/z) in 2D grid. </summary>
		public int r;

		/// <summary> Same as Q. </summary>
		public int x
		{
			get { return q; }
			set { q = value; }
		}

		/// <summary> Same as R. </summary>
		public int y
		{
			get { return r; }
			set { r = value; }
		}

		public MapNavVector(int q, int r)
		{
			this.q = q;
			this.r = r;
		}

		// ------------------------------------------------------------------------------------------------------------
	}
}