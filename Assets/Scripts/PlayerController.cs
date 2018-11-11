using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerController : MonoBehaviour
{

    public float speed = 0.5f;
    public GameObject messagePanel;
    public Camera mainCamera;
    public Camera manageCamera;
    public Canvas UIExploration;
    public UIManager uiManager;
    public VillageManager villageManager;


    private bool canCollect = false;
    private bool manageMode = false;
    private Animator animator;
    private Vector3 villageCenter;
    public float villageSize = 200;

    public int maxGold = 1000;
    public int maxWood = 800;
    public int maxStone = 500;
    public int gold = 150;
    public int wood = 50;
    public int stone = 20;


    void Start()
    {
        villageCenter = GameObject.FindGameObjectWithTag("village").transform.position;
        mainCamera.enabled = true;
        manageCamera.enabled = false;
        animator = gameObject.GetComponent<Animator>();
    }



    void FixedUpdate()
    {
        Transform transform = GetComponent<Transform>();

        float mouveHorizontal = Input.GetAxisRaw("Horizontal");
        float mouveVertical = Input.GetAxisRaw("Vertical");

        if (mouveHorizontal != 0 || mouveVertical != 0) animator.SetBool("Move", true);
        else animator.SetBool("Move", false);

        Vector3 mouvment = new Vector3(mouveHorizontal*speed, 0, mouveVertical*speed);
        //transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(mouvment), 0.15F);
        //transform.Translate(mouvment);
        transform.Translate(mouvment * speed * Time.deltaTime, Space.World);

        if (manageMode)
        {
;            if ((transform.position - villageCenter).magnitude >= villageSize)
            {
                OnManageMode(false);
            }

        }

    }

    void OnTriggerEnter(Collider other)

    {
        if (other.tag == "Collect_gold" || other.tag == "Collect_wood" || other.tag == "Collect_stone")
        {
                Collectable script = (Collectable)other.GetComponent(typeof(Collectable));
                if (script.getIsEmpty()) OpenMessagePanel("-No more ressources-");
                else OpenMessagePanel("-Press P to gather-");
                canCollect = true;
        }
        else if (other.tag == "village" && !manageMode)
        {
            OpenMessagePanel("-Press M to manage-");
        }

    }

    void OnTriggerStay(Collider other)

    {
        if (Input.GetKeyDown(KeyCode.P) && (other.tag == "Collect_gold" || other.tag == "Collect_wood" || other.tag == "Collect_stone"))
        {
            Collectable script = (Collectable)other.GetComponent(typeof(Collectable));
            script.PickRessources();
            if (script.getIsEmpty()) OpenMessagePanel("-No more ressources-");
            else
            {
                CloseMessagePanel();
                OpenMessagePanel("-Press P to gather-");
            }
        }
        else if (Input.GetKeyDown(KeyCode.M) && (other.tag == "village"))
        {
            CloseMessagePanel();
            OnManageMode(true);
        }

    }

    void OnTriggerExit(Collider other)
    {
        if (other.tag == "Collect_gold" || other.tag == "Collect_wood" || other.tag == "Collect_stone")
        {
            CloseMessagePanel();
            canCollect = false;
        }
        else if (other.tag == "village")
        {
            CloseMessagePanel();
        }

    }

    void OpenMessagePanel(string txt)
    {
        messagePanel.SetActive(true);
        if (txt != null) messagePanel.transform.GetChild(0).GetComponent<Text>().text = txt;
    }

    void CloseMessagePanel()
    {
        messagePanel.SetActive(false);
    }

    public void OnManageMode(bool mngMode)
    {
        if (mngMode)
        {
            Debug.Log("ManageMode");
            CloseMessagePanel();

            mainCamera.enabled = false;
            manageCamera.enabled = true;

            UIExploration.transform.GetChild(0).GetComponent<Image>().enabled = false;
            UIExploration.transform.GetChild(0).GetChild(0).GetComponent<Image>().enabled = false;
            UIExploration.transform.GetChild(3).GetComponent<Image>().enabled = false;
            UIExploration.transform.GetChild(3).GetChild(0).GetComponent<RawImage>().enabled = false;


            manageMode = true;
            uiManager.setFreeze(true);

            villageManager.setStone(villageManager.getStone() + stone);
            villageManager.setWood(villageManager.getWood() + wood);
            setStone(0);
            setWood(0);

        }
        else
        {
            Debug.Log("ManageModeExit");
            mainCamera.enabled = true;
            manageCamera.enabled = false;

            UIExploration.transform.GetChild(0).GetComponent<Image>().enabled = true;
            UIExploration.transform.GetChild(0).GetChild(0).GetComponent<Image>().enabled = true;
            UIExploration.transform.GetChild(3).GetComponent<Image>().enabled = true;
            UIExploration.transform.GetChild(3).GetChild(0).GetComponent<RawImage>().enabled = true;

            manageMode = false;
            uiManager.setFreeze(false);
        }


    }

    //---------------//
    //    SETTERS    //
    //---------------//
    public void setMaxGold(int gld)
    {
        if (gld >= 1000) maxGold = gld;
    }
    public void setMaxWood(int wd)
    {
        if (wd >= 800) maxWood = wd;
    }
    public void setMaxStone(int st)
    {
        if (st >= 500) maxStone = st;
    }
    public void setGold(int gld)
    {
        if (gld >= 0 && gld <= maxGold) gold = gld;
    }
    public void setWood(int wd)
    {
        if (wd >= 0 && wd <= maxWood) wood = wd;
    }
    public void setStone(int st)
    {
        if (st >= 0 && st <= maxStone) stone = st;
    }


    //---------------//
    //    GETTERS    //
    //---------------//
    public int getMaxGold()
    {
        return maxGold;
    }
    public int getMaxWood()
    {
        return maxWood;
    }
    public int getMaxStone()
    {
        return maxStone;
    }
    public int getGold()
    {
        return gold;
    }
    public int getWood()
    {
        return wood;
    }
    public int getStone()
    {
        return stone;
    }
    public bool getManageMode()
    {
        return manageMode;
    }


    /*
 *  Update Ressources
 */
    public void decreaseWood(int i)
    {
        villageManager.setWood(villageManager.getWood() - i);
    }

    public void decreaseStone(int i)
    {
        villageManager.setStone(villageManager.getStone() - i);
    }

    public int getWoodManager()
    {
        return villageManager.getWood();
    }
    public int getStoneManager()
    {
        return villageManager.getStone();
    }
}
