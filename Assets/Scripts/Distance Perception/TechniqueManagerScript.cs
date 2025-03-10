using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TechniqueManagerScript : MonoBehaviour
{
    public GameObject rodCast;
    public GameObject flowerCone;
    public GameObject goGoHand;

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
            goGoHand.SetActive(false);
            rodCast.SetActive(true);
            return true;
        }
        else if (technique.Equals("FlowerCone"))
        {
            rodCast.SetActive(false);
            goGoHand.SetActive(false);
            flowerCone.SetActive(true);
            return true;
        }
        else if (technique.Equals("GoGoHand"))
        {
            rodCast.SetActive(false);
            flowerCone.SetActive(false);
            goGoHand.SetActive(true);
            return true;
        }
        return false;
    }
}
