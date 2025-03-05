using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TechniqueManagerScript : MonoBehaviour
{
    public GameObject rodCast;
    public GameObject flowerCone;

    void Start()
    {

    }

    void Update()
    {

    }

    public bool ActivateTechnique(string technique)
    {
        if (technique.Equals("RodCast"))
        {
            flowerCone.SetActive(false);
            rodCast.SetActive(true);
            return true;
        }
        else if (technique.Equals("FlowerCone"))
        {
            rodCast.SetActive(false);
            flowerCone.SetActive(true);
            return true;
        }
        return false;
    }
}
