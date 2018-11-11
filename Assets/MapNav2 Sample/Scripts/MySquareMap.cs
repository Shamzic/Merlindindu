
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MapNavKit;

/// <summary> Used in Sample 2 </summary>
public class MySquareMap : MapNavSquare
{
	public GameObject tileFab;
	public GameObject pathMarkerFab;	// used when marking out calculated path

	// ------------------------------------------------------------------------------------------------------------

	private MapNavNode selectedNode = null;		// keeps track of the selected node
	private MapNavNode altSelectedNode = null;	// keeps track of the second selected node
	private List<GameObject> markedObjects = new List<GameObject>();	// keeps track of nodes that where "marked"
	private List<MapNavNode> invalidNodes = new List<MapNavNode>();		// helper that tracks the nodes marked as "invalid" at runtime
    private List<MapNavNode> alreadyOccupedCells = new List<MapNavNode>();

    protected Camera _camera;

    protected PlayerController _myVillage;

    protected GameObject currentObject = null;
    // ------------------------------------------------------------------------------------------------------------

    protected void Start()
	{
        //FocusCamera();
        _camera = GetComponent<Camera>();
        changeBuildingToBasicHouse();
    }

	/// <summary>
	/// I override this callback so that I can respond on the grid being
	/// changed and place/ update the actual tile objects
	/// </summary>
	public override void OnGridChanged(bool created)
	{
		// The parent object that will hold all the instantiated tile objects
		Transform parent = gameObject.transform;

		// Remove existing tiles and place new ones if map was (re)created
		// since the number of tiles might be different now
		if (created)
		{
			selectedNode = null;
			altSelectedNode = null;
			invalidNodes.Clear();

			for (int i = parent.childCount - 1; i >= 0; i--)
			{
				if (Application.isPlaying)
				{	// was called at runtime
					Object.Destroy(parent.GetChild(i).gameObject);
				}
				else
				{	// was called from editor
					Object.DestroyImmediate(parent.GetChild(i).gameObject);
				}
			}

			// Place tiles according to the generated grid
			for (int idx = 0; idx < grid.Length; idx++)
			{
				// make sure it is a valid node before placing tile here
				if (false == grid[idx].isValid) continue;

				// create a new tile
				GameObject go = (GameObject)Instantiate(tileFab);
				go.name = "Tile " + idx.ToString();
				go.transform.position = grid[idx].position;
				go.transform.parent = parent;
			}

		}

		// else, simply update the position of existing tiles
		else
		{
			for (int idx = 0; idx < grid.Length; idx++)
			{
				// make sure it is a valid node before processing it
				if (false == grid[idx].isValid) continue;

				// Since I gave the tiles proper names I can easily find them by name
				GameObject go = parent.Find("Tile " + idx.ToString()).gameObject;
				go.transform.position = grid[idx].position;
			}
		}

		// focus the camera on the center tile
		// if (Application.isPlaying) FocusCamera();
	}

	// ------------------------------------------------------------------------------------------------------------


	// ------------------------------------------------------------------------------------------------------------

	protected void Update()
	{
      //if ((Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1)) && GUIUtility.hotControl == 0)
        if (Input.GetMouseButtonDown(0))
        {
			// check if clicked on a tile and make it selected
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
			{
				// should be a tile since I have nothing else in scene that has a collider
				// I remove the "Tile " bit from name to get the tile number (node index)
				GameObject go = hit.collider.gameObject;
                string tileIndexName = go.name.Replace("Tile ", "");
				int idx = -1;
				if (int.TryParse(tileIndexName, out idx))
				{
					{
						//if (false == invalidNodes.Contains(grid[idx]))
						{
	/*						if (Input.GetMouseButtonDown(1)) // clic droit
							{
								if (grid[idx] != selectedNode)
								{
									altSelectedNode = grid[idx];
								}
							}
							else // clic gauche*/
							{
								if (grid[idx] != altSelectedNode)
								{
									selectedNode = grid[idx];
                                    if (hit.transform != null )
                                    {
                                        Debug.Log("Click gauche sur la map");
                                        Vector3 position = hit.transform.position;
                                        if (!isNodeAlreadyOccuped())
                                            addBuildingToScene(position);
                                        else
                                            Debug.Log("Cell occuped");
                                    }
                                }
							}
						} 
					}
                    
				}
			}

		}
	}
    

    public bool isNodeAlreadyOccuped()
    {
        bool result = false;
            for (int i = 0; i < alreadyOccupedCells.Count; i++)
            {
                Debug.Log("Recherche Alreadyselected" + i);
                if (selectedNode.Equals(alreadyOccupedCells[i]))
                {
                    result = true;
                }
            }
            return result;
    }


    public void changeBuildingToMediumHouse()
    {
        this.currentObject = FindObjectOfType<All3DObjects>().getNextObject();
        Debug.Log("Medium House Selected ");
    }

    public void changeBuildingToBasicHouse()
    {
        this.currentObject = FindObjectOfType<All3DObjects>().getCurrentObject();
        Debug.Log("Basic House selected !");
    }

    protected void addBuildingToScene(Vector3 position)
    {
        //GameObject currentObject = FindObjectOfType<All3DObjects>().getCurrentObject();

        // Ici ajouter currentObject = batiment selectionné

        if (currentObject != null)
        {
            Debug.Log("OK");
            //Instantiate

            _myVillage = FindObjectOfType<PlayerController>();
            if (_myVillage.getStoneManager()>0 && _myVillage.getWoodManager()>0)
            {
                Debug.Log("wood : " + _myVillage.getWoodManager());
                Debug.Log("stone : " + _myVillage.getStoneManager());
                _myVillage.decreaseStone(2);
                _myVillage.decreaseWood(2);
                Instantiate(currentObject, position, Quaternion.identity);
            }
            else {
                
            }

            
            
        }
    }

    // ------------------------------------------------------------------------------------------------------------
}
