using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TechniqueManagerScript : MonoBehaviour
{
    public GameObject rodCast;
    public GameObject flowerCone;
    public GameObject goGoHand;

    private RodCastScript rodCastScript;
    private FlowerConeScript flowerConeScript;
    private GoGoHandScript goGoHandScript;

    void Start()
    {
        rodCastScript = rodCast.GetComponent<RodCastScript>();
        flowerConeScript = flowerCone.GetComponent<FlowerConeScript>();
        goGoHandScript = goGoHand.GetComponent<GoGoHandScript>();
    }

    void Update()
    {

    }

    public bool ActivateTechnique(string technique)
    {
        if (technique.Equals("RodCast"))
        {
            rodCastScript.ResetVariables();

            flowerCone.SetActive(false);
            goGoHand.SetActive(false);
            rodCast.SetActive(true);
            return true;
        }
        else if (technique.Equals("FlowerCone"))
        {
            flowerConeScript.ResetVariables();

            rodCast.SetActive(false);
            goGoHand.SetActive(false);
            flowerCone.SetActive(true);
            return true;
        }
        else if (technique.Equals("GoGoHand"))
        {
            goGoHandScript.ResetVariables();

            rodCast.SetActive(false);
            flowerCone.SetActive(false);
            goGoHand.SetActive(true);
            return true;
        }
        return false;
    }

    public void DeactivateAll()
    {
        rodCastScript.ResetVariables();
        flowerConeScript.ResetVariables();
        goGoHandScript.ResetVariables();

        rodCast.SetActive(false);
        flowerCone.SetActive(false);
        goGoHand.SetActive(false);
    }

    public bool IsActiveTechnique(string technique)
    {
        if (technique.Equals("RodCast"))
        {
            return rodCast.activeSelf;
        }
        else if (technique.Equals("FlowerCone"))
        {
            return flowerCone.activeSelf;
        }
        else if (technique.Equals("GoGoHand"))
        {
            return goGoHand.activeSelf;
        }
        return false;
    }
}
