
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MapNavKit;


/// <summary>
/// A custom tile (node) that I can store additional properties for each tile
/// </summary>
public class Sample4Tile : MapNavNode
{
	// see Sample4MapInspector.cs for how I instructed the "Create Grid" tool to use this
	// custom node type rather than the default MapNavNode class. It comes down to calling
	// MapNavBase.CreateGrid<T>() or MapNavBase.CreateGrid(System.Type nodeType) with
	// your custom node type.

	public Sample4Unit unit = null; // this will be set if there is a unit on this tile

	// you can define any number of additional properties and init them when the grid was created
	// by overriding OnGridChanged() in your class derived from MapNavHexa or MapNavSquare
	// Check out Sample4Map.OnGridChanged() to see how I set the following property.
	// It is not used for anything else than to demonstrate how you can init values
	// for tiles/ nodes that where just added to the grid.

	public string tileName = "";

	// This one I will actually use. It is a reference to the places tile GameObject/ art
	// that represents this node/ tile in the scene. See sample4Controller.cs for the 
	// reasons I did this.

	public GameObject tileObj = null;

	// ------------------------------------------------------------------------------------------------------------
}
