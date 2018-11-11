using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;




public class Object3D : MonoBehaviour {
    //Paramaters attributes:
    [SerializeField]
    protected StringObject buildingEtapes;
    [SerializeField]
    protected int startingIndex = 0;
    
    [SerializeField]
    protected RessourcesDictionary ressourcesNeeded= new RessourcesDictionary();
    protected RessourcesDictionary currentRessoruces= new RessourcesDictionary();



    //Internal attributes:
    protected GameObject current_obj;
    protected ParticleSystem particule_system;
    protected float T=0;


    void Start () {
        List<string> all_ressources = Ressources.getAllRessources();
        
        for(int i=0; i<all_ressources.Count; i++)
        {
            ressourcesNeeded.Add(all_ressources[i], 0);
        }

        particule_system = GetComponentInChildren<ParticleSystem>();
        current_obj = Instantiate(buildingEtapes[startingIndex.ToString()], transform.position, Quaternion.identity);
    }
	
	void Update () {
        //TODO:
        //  Ajouter les conditions de constructions (ressources, ouvrier, ect ...).
        //  Ajouter une construction sous forme de %.
        //  Fixe particules.
        //  Fixe resources nécéssaire.

        T += Time.deltaTime;
        if (T > 2.5f)
        {
            T = 0;
            upgradeModel();
        }
	}
    
    void upgradeModel()
    {
        //start the particules
        particule_system.Play();

        //upgrade building models:
        startingIndex++;
        if (startingIndex < buildingEtapes.Count)
        {
            Destroy(current_obj);
            current_obj = Instantiate(buildingEtapes[startingIndex.ToString()], transform.position, Quaternion.identity);
        }
        else
        {
            startingIndex = buildingEtapes.Count - 1;
        }
       
    }
    void downgradeModel()
    {
        startingIndex--;
        if (startingIndex < buildingEtapes.Count)
        {
            Destroy(current_obj);
            current_obj = Instantiate(buildingEtapes[startingIndex.ToString()], transform.position, Quaternion.identity);
        }
        else
        {
            startingIndex = 0;
        }
    }
    
}
