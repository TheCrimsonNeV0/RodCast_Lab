using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System.Collections;

public class DataManagerScript : MonoBehaviour
{
    public InputActionReference getDistanceBetweenDistanceObjectAction;
    public GameObject rodCastInteraction;
    public GameObject flowerCone;
    public GameObject csvWriter;

    private RodCastScript rodCastScript;
    private CsvWriterScript csvWriterScript;

    // Start is called before the first frame update
    void Start()
    {
        if (rodCastInteraction != null)
        {
            rodCastScript = rodCastInteraction.GetComponent<RodCastScript>();
        }
        if (csvWriter != null)
        {
            csvWriterScript = csvWriter.GetComponent<CsvWriterScript>();
        }

        getDistanceBetweenDistanceObjectAction.action.Enable();
        getDistanceBetweenDistanceObjectAction.action.performed += context => {
            if (rodCastScript != null)
            {
                GameObject objectToCompare = GameObject.FindGameObjectsWithTag("DistanceObject")[0];
                Debug.Log("Distance Object Position: " + objectToCompare.transform.position);
                Vector3 endPointCoordinate = rodCastScript.GetEndPointPosition();
                float distance = Vector3.Distance(endPointCoordinate, objectToCompare.transform.position);
                Destroy(objectToCompare); // Destroy the Distance Object

                string activeTechnique = GetActiveTechnique();

                if (csvWriterScript != null)
                {
                    csvWriterScript.RecordData(activeTechnique, objectToCompare.transform.position, endPointCoordinate, distance);
                }
            }
        };
    }

    // Update is called once per frame
    void Update()
    {

    }

    string GetActiveTechnique()
    {
        if (rodCastInteraction.activeSelf)
        {
            return "RodCast";
        }
        else if (flowerCone.activeSelf)
        {
            return "FlowerCone";

        }
        else
        {
            return "";
        }
    }
}
