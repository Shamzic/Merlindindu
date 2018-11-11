using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VillageManager : MonoBehaviour {


    protected int wood;
    protected int stone;

    // Use this for initialization
    void Start () {
    }
	
	// Update is called once per frame
	void Update () {

    }


    //---------------//
    //    SETTERS    //
    //---------------//
    public void setWood(int wd)
    {
        if (wd >= 0) wood = wd;
    }
    public void setStone(int st)
    {
        if (st >= 0) stone = st;
    }


    //---------------//
    //    GETTERS    //
    //---------------//

    public int getWood()
    {
        return wood;
    }
    public int getStone()
    {
        return stone;
    }

}
