
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MapNavKit;


public class Sample4Unit : MonoBehaviour 
{
	public float moveSpeed = 1f;

	public Sample4Tile tile { get; set; }	// the tile that this unit is on
	public int movesLeft { get; set; }		// each unit can move up to 3 tiles per turn

	public delegate void OnMoveCompleted();
	private OnMoveCompleted onMoveCompleted = null; // callback to call when this unit is done moving

	private bool moving = false;
	private List<MapNavNode> path = null;
	private int nextPathIdx = 0;
	private Vector3 targetPos = Vector3.zero;
	private float journeyLength;
	private float startTime;

	// ------------------------------------------------------------------------------------------------------------

	/// <summary>
	/// Call this at "end of turn" to reset the unit, like how many moves it got left
	/// </summary>
	public void Resetunit()
	{
		movesLeft = 5;
	}

	/// <summary>
	/// I use this to link the tile and unit with each other so that the
	/// tile and unit knows which unit is on the tile. I will pass null
	/// to simply unlink the tile and unit.
	/// </summary>
	public void LinkWithTile(Sample4Tile t)
	{
		// first unlink the unit from the tile
		if (tile != null) tile.unit = null;
		tile = t;

		// if t == null then this was simply an unlink and it ends here
		if (tile == null) return;

		// else tell the tile that this unit is on it
		tile.unit = this;
	}

	/// <summary>
	/// Change the colour of the unit when it is selected
	/// </summary>
	public void UnitSelected()
	{
		transform.GetChild(0).GetComponent<Renderer>().material.color = Color.green;
	}

	/// <summary>
	/// Change the colour of the unit when it is de-selected
	/// </summary>
	public void UnitDeSelected()
	{
		transform.GetChild(0).GetComponent<Renderer>().material.color = new Color(0f, 0.36f, 0.51f, 1f);
	}

	/// <summary>
	/// start moving the unit form node to node
	/// </summary>
	public void Move(List<MapNavNode> path, OnMoveCompleted callback)
	{
		if (path.Count == 0)
		{
			callback();
			return;
		}

		this.onMoveCompleted = callback;
		this.path = path;
		this.moving = true;

		// unlink with tile
		LinkWithTile(null);

		// start moving
		targetPos = path[0].position;
		nextPathIdx = 1;
		journeyLength = Vector3.Distance(transform.position, targetPos);
		startTime = Time.time;
		movesLeft--;
	}

	// ------------------------------------------------------------------------------------------------------------

	protected void Update()
	{
		if (moving)
		{
			float distCovered = (Time.time - startTime) * moveSpeed;
			float fracJourney = distCovered / journeyLength;

			transform.position = Vector3.Lerp(transform.position, targetPos, fracJourney);

			if (transform.position == targetPos)
			{
				if (nextPathIdx >= path.Count)
				{
					// reached end of path. link with new tile and tell controller i am done
					moving = false;
					LinkWithTile(path[path.Count - 1] as Sample4Tile);
					onMoveCompleted();
					return;
				}

				// go to next node
				targetPos = path[nextPathIdx].position;
				nextPathIdx++;
				journeyLength = Vector3.Distance(transform.position, targetPos);
				startTime = Time.time;
				movesLeft--;
			}
		}
	}

	// ------------------------------------------------------------------------------------------------------------
}
