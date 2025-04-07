using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TechniqueSelectionHandlerScript : MonoBehaviour
{
    public GameObject rodcastInteraction;
    public GameObject flowerConeInteraction;
    public GameObject goGoHandInteraction;

    public void OnButtonClickRodCast()
    {
        flowerConeInteraction.SetActive(false);
        goGoHandInteraction.SetActive(false);
        rodcastInteraction.SetActive(true);
    }

    public void OnButtonClickFlowerCone()
    {
        rodcastInteraction.SetActive(false);
        goGoHandInteraction.SetActive(false);
        flowerConeInteraction.SetActive(true);
    }

    public void OnButtonClickGoGoHand()
    {
        rodcastInteraction.SetActive(false);
        flowerConeInteraction.SetActive(false);
        goGoHandInteraction.SetActive(true);
    }

    public void OnButtonClickStartTest()
    {
        SceneManager.LoadScene("ObjectDistanceCognitionScene");
    }
}
