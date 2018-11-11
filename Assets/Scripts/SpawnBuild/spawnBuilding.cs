using MapNavKit;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class spawnBuilding : MonoBehaviour
{
    protected Camera _camera;

    protected GameObject currentObject = null;
    // Use this for initialization
    void Start()
    {
        _camera = GetComponent<Camera>();
    }
    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
         //   MySquareMap msqm = (MySquareMap)FindObjectOfType(typeof(MySquareMap));

                Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit))
                {
                    if (hit.transform != null)
                    {
                        Debug.Log("TOTO");
                        Vector3 position = hit.transform.position;

                        if (currentObject == null)
                            changeBuildingToBasicHouse();

                        if (true)
                        { // TODO TEST validité cellule !msqm.isNodeAlreadyOccuped()
                            addBuildingToScene(position);
                        }
                        else
                        {
                            //Debug.Log("Cellule déjà utilisée !!!!");
                            ;
                        }

                    }
                }
            

        }
    }

                    

    public void changeBuildingToMediumHouse()
    {
        this.currentObject = FindObjectOfType<All3DObjects>().getNextObject();
        Debug.Log("Basic House Selected ");
    }

    public void changeBuildingToBasicHouse()
    {
        this.currentObject = FindObjectOfType<All3DObjects>().getCurrentObject();
        Debug.Log("Medium House selected !");
    }

    protected void addBuildingToScene(Vector3 position)
    {
        //GameObject currentObject = FindObjectOfType<All3DObjects>().getCurrentObject();

        // Ici ajouter currentObject = batiment selectionné

        if (currentObject != null)
        {
            Debug.Log("OK");
            //Instantiate
            Instantiate(currentObject, position, Quaternion.identity);
        }
    }
}
