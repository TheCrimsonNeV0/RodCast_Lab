using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class FlowerConeScript : MonoBehaviour
{
    public InputActionReference holdObjectAction;
    public InputActionReference setRayLengthRollAction;
    public InputActionReference incrementObjectIndexAction;

    public GameObject conePrefab;
    public float rayLength = 10.0f;
    
    private Vector3 startPoint; // The peak (pointy end) of the cone
    private Vector3 endPoint; // The center of the cone's base

    public float minRayLength = 1f; // Minimum length of the ray
    public float maxRayLength = 10f; // Maximum length of the ray
    public float rotationSensitivity = 0.1f;

    public string outlineScriptName = "Outline";

    private GameObject coneInstance;
    private Transform coneTip;
    private Transform coneBottomOrb;

    private List<GameObject> collidingObjects = new List<GameObject>();
    private GameObject objectToMove;

    // Temporary Variables
    private bool isHolding;

    private bool isAdjusting_Right;
    private float lastRotationZ = 0f;
    private float lastPositionY = 0f;
    private float rotationOffset = 0f;

    private int indexCounter = 0;
    private GameObject closestObject;

    // Start is called before the first frame update
    void Start()
    {
        // Actions Initialization Start

        holdObjectAction.action.Enable();
        holdObjectAction.action.started += context =>
        {
            isHolding = true;
            if (objectToMove != null)
            {
                objectToMove.GetComponent<Rigidbody>().isKinematic = true;
                objectToMove.transform.SetParent(coneBottomOrb.transform);
            }
        };
        holdObjectAction.action.canceled += context => {
            isHolding = false;
            if (objectToMove != null)
            {
                objectToMove.GetComponent<Rigidbody>().isKinematic = false;
                objectToMove.transform.SetParent(null);
            }
            objectToMove = null; // Clear objectToHold
        };

        setRayLengthRollAction.action.Enable();
        setRayLengthRollAction.action.started += context => isAdjusting_Right = true; // Start adjusting length
        setRayLengthRollAction.action.canceled += context => isAdjusting_Right = false; // Stop adjusting length
        setRayLengthRollAction.action.performed += context =>
        {
            lastRotationZ = transform.rotation.eulerAngles.z; // Reset the last rotation
            lastPositionY = transform.position.y; // Reset the last position
        };

        incrementObjectIndexAction.action.Enable();
        incrementObjectIndexAction.action.performed += context =>
        {
            indexCounter += 1;
        };

        // Actions Initialization End

        if (conePrefab != null)
        {
            coneInstance = Instantiate(conePrefab, transform);
            coneTip = coneInstance.transform.Find("Top Orb");
            coneBottomOrb = coneInstance.transform.Find("Bottom Orb");
        }
        else
        {
            Debug.LogError("Cone prefab is not assigned!");
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Update startPoint and endPoint based on the transform
        UpdateRayCast();
        AlignCone();
        
        if (!(isHolding && objectToMove != null))
        {
            closestObject = GetNearestObject();

            GameObject tempObjectToMove = null;
            if (collidingObjects.Count > 0)
            {
                tempObjectToMove = collidingObjects[indexCounter % collidingObjects.Count];
            }

            // Highlighting object logic
            if (tempObjectToMove != null)
            {
                if (tempObjectToMove != objectToMove)
                {
                    if (objectToMove != null)
                    {
                        SetHighlightObjectToMove(objectToMove, false);
                    }
                    objectToMove = tempObjectToMove;
                    SetHighlightObjectToMove(objectToMove, true);
                }
                else
                {
                    SetHighlightObjectToMove(objectToMove, true);
                }
            }
            else
            {
                if (objectToMove != null)
                {
                    SetHighlightObjectToMove(objectToMove, false);
                    objectToMove = null;
                }
            }
        }

        if (isAdjusting_Right)
        {
            AdjustRayLength();
        }
    }

    void UpdateRayCast()
    {
        // Define the startPoint and endPoint along the transform's forward direction
        startPoint = transform.position;
        endPoint = startPoint + transform.forward * rayLength;
    }

    private Vector3 lastScale; // Store last known scale to compare changes
    void AlignCone()
    {
        // Calculate the new scale
        Vector3 newScale = coneInstance.transform.localScale;
        newScale.y = rayLength;

        // Check if the scale has changed
        if (newScale == lastScale)
        {
            return; // Exit early if there is no change
        }

        // Update the last scale record
        lastScale = newScale;

        // Store original child world scales
        Dictionary<Transform, Vector3> originalChildWorldScales = new Dictionary<Transform, Vector3>();

        foreach (Transform child in coneInstance.transform)
        {
            originalChildWorldScales[child] = child.lossyScale; // Store world scale before scaling the parent
        }

        // Calculate the offset from the cone's tip (child) to the cone's pivot
        Vector3 tipOffset = coneTip.position - coneInstance.transform.position;

        // Reposition the cone so that the tip aligns with startPoint
        coneInstance.transform.position = startPoint - tipOffset;

        // Apply the new scale to the parent
        coneInstance.transform.localScale = newScale;

        // Restore child scales to maintain original world scales
        foreach (var kvp in originalChildWorldScales)
        {
            Transform child = kvp.Key;
            Vector3 originalWorldScale = kvp.Value;

            // Compute new local scale so that world scale remains unchanged
            Vector3 newParentLossyScale = coneInstance.transform.lossyScale;
            child.localScale = new Vector3(
                originalWorldScale.x / newParentLossyScale.x,
                originalWorldScale.y / newParentLossyScale.y,
                originalWorldScale.z / newParentLossyScale.z
            );
        }
    }

    // Called by the child component
    public void AddCollidingObject(GameObject obj)
    {
        if (!collidingObjects.Contains(obj))
        {
            collidingObjects.Add(obj);
        }
        Vector3 sphereCenter = coneBottomOrb.transform.position;
        collidingObjects.Sort((a, b) =>
        {
            float distanceA = Vector3.Distance(sphereCenter, a.transform.position);
            float distanceB = Vector3.Distance(sphereCenter, b.transform.position);
            return distanceA.CompareTo(distanceB); // Sort in ascending order
        });
        if (collidingObjects[0] != closestObject)
        {
            indexCounter = 0;
        }
    }

    // Called by the child component
    public void RemoveCollidingObject(GameObject obj)
    {
        if (collidingObjects.Contains(obj))
        {
            collidingObjects.Remove(obj);
        }
        Vector3 sphereCenter = coneBottomOrb.transform.position;
        collidingObjects.Sort((a, b) =>
        {
            float distanceA = Vector3.Distance(sphereCenter, a.transform.position);
            float distanceB = Vector3.Distance(sphereCenter, b.transform.position);
            return distanceA.CompareTo(distanceB); // Sort in ascending order
        });
        if (collidingObjects.Count > 0)
        {
            if (collidingObjects[0] != closestObject)
            {
                indexCounter = 0;
            }
        }
    }

    public GameObject GetNearestObject()
    {
        if (collidingObjects.Count == 0)
        {
            return null;
        }

        Vector3 sphereCenter = coneBottomOrb.transform.position;

        // Sort the list by distance using List.Sort
        /* collidingObjects.Sort((a, b) =>
        {
            float distanceA = Vector3.Distance(sphereCenter, a.transform.position);
            float distanceB = Vector3.Distance(sphereCenter, b.transform.position);
            return distanceA.CompareTo(distanceB); // Sort in ascending order
        }); */

        // The closest object is now the first one in the sorted list
        GameObject nearestObject = collidingObjects[0];

        return nearestObject;
    }

    public void SetHighlightObjectToMove(GameObject objectToChange, bool isHighlighted)
    {
        MonoBehaviour targetScript = (MonoBehaviour) objectToChange.GetComponent("Outline");
        if (targetScript != null)
        {
            targetScript.enabled = isHighlighted;
        }
    }

    ////////////////////////////////// CONTROLLER POSITION AND ROTATION LOGIC START /////////////////////////////////////////


    void AdjustRayLength()
    {
        rotationOffset = ParseRotationOffset(transform.rotation.eulerAngles.z);
        if (rotationOffset != 0)
        {
            rayLength += rotationSensitivity * rotationOffset;
            rayLength = Mathf.Clamp(rayLength, minRayLength, maxRayLength);
        }
    }

    float ParseRotationOffset(float currentRotationZ)
    {
        float angleDifference = currentRotationZ - lastRotationZ;

        // Detect crossovers at 0 or 360 degrees
        if (angleDifference > 180) // Crossed 360 to 0 counterclockwise
        {
            angleDifference -= 360;
        }
        else if (angleDifference < -180) // Crossed 0 to 360 clockwise
        {
            angleDifference += 360;
        }

        lastRotationZ = currentRotationZ;

        // Return the offset (positive for clockwise, negative for counterclockwise)
        return -1 * angleDifference; // Negative for rotation correction
    }

    ////////////////////////////////// CONTROLLER POSITION AND ROTATION LOGIC END /////////////////////////////////////////
}
