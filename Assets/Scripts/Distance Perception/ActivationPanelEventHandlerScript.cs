using System.Collections;
using System.Collections.Generic;
using Unity.VRTemplate;
using UnityEngine;
using TMPro;

public class ActivationPanelEventHandlerScript : MonoBehaviour
{
    public GameObject objectDistanceCreator;
    private ObjectDistanceCreatorScript objectDistanceCreatorScript;

    public int countdownValueStatic = 10;
    private int countdownValue;
    public TextMeshProUGUI countdownText;

    // Start is called before the first frame update
    void Start()
    {
        if (objectDistanceCreator != null)
        {
            objectDistanceCreatorScript = objectDistanceCreator.GetComponent<ObjectDistanceCreatorScript>();
        }
    }

    public void OnButtonClickReady()
    {
        countdownValue = countdownValueStatic;
        StartCoroutine(StartCountdown());
    }

    public IEnumerator StartCountdown()
    {
        while (countdownValue > 0)
        {
            countdownText.text = countdownValue.ToString();
            yield return new WaitForSeconds(1f); // Wait for 1 second
            countdownValue--; // Reduce value by 1
        }

        countdownText.text = countdownValue.ToString();
        ExecuteFunction();
    }

    void ExecuteFunction()
    {
        objectDistanceCreatorScript.StartObjectCreation();
        Destroy(transform.parent.gameObject);
    }
}
