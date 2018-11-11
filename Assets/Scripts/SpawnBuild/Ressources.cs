using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class Ressources {
    public static List<string> getAllRessources()
    {
        List<string> _ressources = new List<string>();
        _ressources.Add("Bois");
        _ressources.Add("Pierre");
        _ressources.Add("Or");
        _ressources.Add("Fer");
        _ressources.Add("Charbon");
        _ressources.Add("Blé");

        return _ressources;
    }
}
