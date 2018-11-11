// -= MapNav 2 =-
// www.plyoung.com
// by Leslie Young
// ====================================================================================================================

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace MapNavKit
{
	/// <summary> Used by MapNavNode.linkData </summary>
	[System.Serializable]
	public class MapNavNodeLink
	{
		/// <summary> The index into the map array of the node that is linked with.
		/// Do not change directly, use the functions provided in MapNavNode. </summary>
		public int nodeIdx = -1;

		/// <summary> The data of this link. Do not change directly, use the
		/// functions provided in MapNavNode. </summary>
		public int data = 0;

		// ------------------------------------------------------------------------------------------------------------
	}
}