using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TechniqueSelectionHandlerScript : MonoBehaviour
{
    public GameObject rodcastInteraction;
    public GameObject flowerConeInteraction;

    public void OnButtonClickRodCast()
    {
        flowerConeInteraction.SetActive(false);
        rodcastInteraction.SetActive(true);
    }

    public void OnButtonClickFlowerCone()
    {
        rodcastInteraction.SetActive(false);
        flowerConeInteraction.SetActive(true);
    }
}
