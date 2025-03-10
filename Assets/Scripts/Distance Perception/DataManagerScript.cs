using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System.Collections;
using System.Drawing;

public class DataManagerScript : MonoBehaviour
{
    public InputActionReference getDistanceBetweenDistanceObjectAction;
    public GameObject rodCastInteraction;
    public GameObject flowerCone;
    public GameObject goGoHand;
    public GameObject csvWriter;

    private RodCastScript rodCastScript;
    private FlowerConeScript flowerConeScript;
    private GoGoHandScript goGoHandScript;
    private CsvWriterScript csvWriterScript;

    public enum Technique
    {
        RodCast,
        FlowerCone,
        GoGoHand
    }

    // Start is called before the first frame update
    void Start()
    {
        if (rodCastInteraction != null)
        {
            rodCastScript = rodCastInteraction.GetComponent<RodCastScript>();
        }
        if (flowerCone != null)
        {
            flowerConeScript = flowerCone.GetComponent<FlowerConeScript>();
        }
        if (goGoHand != null)
        {
            goGoHandScript = goGoHand.GetComponent<GoGoHandScript>();
        }

        if (csvWriter != null)
        {
            csvWriterScript = csvWriter.GetComponent<CsvWriterScript>();
        }

        getDistanceBetweenDistanceObjectAction.action.Enable();
        getDistanceBetweenDistanceObjectAction.action.performed += context => {
            Technique? activeTechnique = GetActiveTechnique();
            if (activeTechnique == Technique.RodCast)
            {
                if (rodCastScript != null)
                {
                    GameObject objectToCompare = GameObject.FindGameObjectsWithTag("DistanceObject")[0];
                    Debug.Log("Distance Object Position: " + objectToCompare.transform.position);
                    Vector3 endPointCoordinate = rodCastScript.GetEndPointPosition();
                    float distance = Vector3.Distance(endPointCoordinate, objectToCompare.transform.position);
                    bool isSuccess = IsSuccess(endPointCoordinate, objectToCompare);
                    Destroy(objectToCompare); // Destroy the Distance Object

                    if (csvWriterScript != null)
                    {
                        csvWriterScript.RecordData("RodCast", objectToCompare.transform.position, endPointCoordinate, 
                            distance, isSuccess);
                    }
                }
            }
            else if (activeTechnique == Technique.FlowerCone)
            {
                if (flowerConeScript != null)
                {
                    GameObject objectToCompare = GameObject.FindGameObjectsWithTag("DistanceObject")[0];
                    Debug.Log("Distance Object Position: " + objectToCompare.transform.position);
                    Vector3 endPointCoordinate = flowerConeScript.GetBottomOrbCenterPoint();
                    float distance = Vector3.Distance(endPointCoordinate, objectToCompare.transform.position);
                    Destroy(objectToCompare); // Destroy the Distance Object

                    if (csvWriterScript != null)
                    {
                        csvWriterScript.RecordData("FlowerCone", objectToCompare.transform.position, endPointCoordinate,
                            distance, IsSuccessFlowerCone(objectToCompare.transform.position, endPointCoordinate, flowerConeScript.GetBottomOrbRadius()));
                    }
                }
            }
            else if (activeTechnique == Technique.GoGoHand)
            {
                if (goGoHandScript != null)
                {
                    GameObject objectToCompare = GameObject.FindGameObjectsWithTag("DistanceObject")[0];
                    Debug.Log("Distance Object Position: " + objectToCompare.transform.position);
                    Vector3 endPointCoordinate = goGoHandScript.GetEndPointPosition();
                    float distance = Vector3.Distance(endPointCoordinate, objectToCompare.transform.position);
                    bool isSuccess = IsSuccess(endPointCoordinate, objectToCompare);
                    Destroy(objectToCompare); // Destroy the Distance Object

                    if (csvWriterScript != null)
                    {
                        csvWriterScript.RecordData("GoGoHand", objectToCompare.transform.position, endPointCoordinate,
                            distance, isSuccess);
                    }
                }
            }
        };
    }

    // Update is called once per frame
    void Update()
    {

    }

    Technique? GetActiveTechnique()
    {
        if (rodCastInteraction.activeSelf)
        {
            return Technique.RodCast;
        }
        else if (flowerCone.activeSelf)
        {
            return Technique.FlowerCone;

        }
        else if (goGoHand.activeSelf)
        {
            return Technique.GoGoHand;
        }
        else
        {
            return null;
        }
    }

    public bool IsSuccess(Vector3 point, GameObject obj)
    {
        if (obj == null)
        {
            Debug.LogError("The provided object is null.");
            return false;
        }

        // Try to get the Collider from the object (any type of collider)
        Collider collider = obj.GetComponent<Collider>();
        if (collider != null)
        {
            // Check if the point is inside the bounds of the collider
            return collider.bounds.Contains(point);
        }

        // If no collider, fall back to checking the Renderer bounds
        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer != null)
        {
            // Check if the point is inside the bounds of the renderer
            return renderer.bounds.Contains(point);
        }

        Debug.LogWarning("No Collider or Renderer found on " + obj.name);
        return false; // If neither collider nor renderer exists
    }

    bool IsSuccessFlowerCone(Vector3 point, Vector3 sphereCenter, float radius) // Function for FlowerCone
    {
        float distanceSquared = (point - sphereCenter).sqrMagnitude;
        return distanceSquared <= radius * radius;
    }
}
