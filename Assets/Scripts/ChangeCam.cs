
using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ChangeCam : MonoBehaviour
{
    public Camera main_cam;
    public Camera other_cam;

    public Image panel;
    private Color colorFadeOut;
    private Color colorFadeIn;
   

    void Start()
    {
        main_cam.enabled = true;
        other_cam.enabled = false;
        colorFadeIn = new Color(1f, 1f, 1f, 1f);
        colorFadeOut = new Color(1f, 1f, 1f, 0f);
        if (panel != null) panel.CrossFadeColor(colorFadeOut, 0.01f, true, true);
    }

    private Transform target = null;

    void OnTriggerEnter(Collider other)

    {
        if (other.tag == "Player") target = other.transform;
        if (panel != null)  panel.CrossFadeColor(colorFadeIn, 3f, true, true);


    }

    void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player") target = null;
        //panel.CrossFadeColor(colorFadeIn, 3f, true, true);
        if (panel != null) panel.CrossFadeColor(colorFadeOut, 3f, true, true);
    }

    void Update()
    {
        if (target == null)
        {
            main_cam.enabled = true;
            other_cam.enabled = false;
        }

        else
        {
            main_cam.enabled = false;
            other_cam.enabled = true;
        }

    }
}


