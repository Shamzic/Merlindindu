using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour {

    public PlayerController playerScript;
    public VillageManager villageScript;

    public float currentGold;
    public Image current_gold;
    private float maxGold = 100000;
    public int spdCount = 0;
    public int spdTimer = 350;

    private bool freeze = false;
    private bool manageMode = false;

	// Use this for initialization
	void Start () {
        currentGold = playerScript.getGold() * 100;
        maxGold = playerScript.getMaxGold() * 100;
        UpdateHealthBar();
    }
	
	// Update is called once per frame
	void UpdateHealthBar() {

        float ratio = currentGold / maxGold;
        current_gold.rectTransform.localScale = new Vector3(ratio, 1, 1);
	}

    void Update()
    {
        spdCount += 1;

        if (spdCount >= spdTimer && !freeze)
        {
            playerScript.setGold(playerScript.getGold() - 1);
            spdCount = 0;
        }
        else if (freeze) spdCount = 0;


        currentGold = playerScript.getGold() * 100;
        maxGold = playerScript.getMaxGold() * 100;
        UpdateHealthBar();

        manageMode = playerScript.getManageMode();

        if (!manageMode)
        {
            transform.GetChild(2).GetChild(0).GetChild(1).GetComponent<Text>().text = (currentGold / 100).ToString();
            transform.GetChild(2).GetChild(1).GetChild(1).GetComponent<Text>().text = (playerScript.getWood()).ToString();
            transform.GetChild(2).GetChild(2).GetChild(1).GetComponent<Text>().text = (playerScript.getStone()).ToString();
        }
        else
        {
            transform.GetChild(2).GetChild(0).GetChild(1).GetComponent<Text>().text = (currentGold / 100).ToString();
            transform.GetChild(2).GetChild(1).GetChild(1).GetComponent<Text>().text = (villageScript.getWood()).ToString();
            transform.GetChild(2).GetChild(2).GetChild(1).GetComponent<Text>().text = (villageScript.getStone()).ToString();
        }



    }

    public void setFreeze(bool frz)
    {
        freeze = frz;
    }
}
