using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class FlowerConeScript : MonoBehaviour
{
    public enum SelectionType
    {
        SelectionTechnique,
        DistancePerception
    }

    public SelectionType selectedOption;

    public InputActionReference holdObjectAction;
    public InputActionReference setRayLengthRollAction;
    public InputActionReference incrementObjectIndexAction;
    public InputActionReference getCenterPositionAction;

    public GameObject conePrefab;
    public float rayLength_static = 10.0f;
    private float rayLength = 10.0f;
    
    private Vector3 startPoint; // The peak (pointy end) of the cone
    private Vector3 endPoint; // The center of the cone's base

    public float minRayLength = 1f; // Minimum length of the ray
    public float maxRayLength = 10f; // Maximum length of the ray
    public float rotationSensitivity = 0.1f;

    public string outlineScriptName = "Outline";

    private GameObject coneInstance;
    private Transform coneTip;
    private Transform coneBottomOrb;

    private Transform endPointIndicator;

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
        ResetVariables();

        // START ACTION BINDING

        // START CONE SETTING BINDING

        setRayLengthRollAction.action.Enable();
        setRayLengthRollAction.action.started += context => isAdjusting_Right = true; // Start adjusting length
        setRayLengthRollAction.action.canceled += context => isAdjusting_Right = false; // Stop adjusting length
        setRayLengthRollAction.action.performed += context =>
        {
            lastRotationZ = transform.rotation.eulerAngles.z; // Reset the last rotation
            lastPositionY = transform.position.y; // Reset the last position
        };

            // END CONE SETTING BINDING

            // START OTHER ACTION BINDING

        if (selectedOption == SelectionType.SelectionTechnique)
        {
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
                    SetHighlightObjectToMove(objectToMove, false); // Unhighlight the object on release
                }
                objectToMove = null; // Clear objectToHold
            };

            incrementObjectIndexAction.action.Enable();
            incrementObjectIndexAction.action.performed += context =>
            {
                indexCounter += 1;
            };
        }

        else if (selectedOption == SelectionType.DistancePerception)
        {
            getCenterPositionAction.action.Enable();
            getCenterPositionAction.action.performed += context =>
            {
                Debug.Log(coneBottomOrb.transform.position);
            };
        }

            // END OTHER ACTION BINDING

        // END ACTION BINDING

        if (conePrefab != null)
        {
            coneInstance = Instantiate(conePrefab, transform);
            coneTip = coneInstance.transform.Find("Top Orb");
            coneBottomOrb = coneInstance.transform.Find("Bottom Orb");
            endPointIndicator = coneInstance.transform.Find("End Point Indicator");
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

        // TODO: Toggle visibility of end point indicator based on if object to move is null

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

            if (objectToMove != null)
            {
                endPointIndicator.gameObject.SetActive(false);
            }
            else
            {
                endPointIndicator.gameObject.SetActive(true);
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
        Vector3 newScale = coneInstance.transform.localScale;
        newScale.x = rayLength;
        newScale.y = rayLength;
        newScale.z = rayLength;

        if (newScale == lastScale)
        {
            return;
        }

        lastScale = newScale;

        Vector3 originalWorldScale = Vector3.one;

        // Only store the world scale if we are holding an object
        if (isHolding && objectToMove != null)
        {
            originalWorldScale = objectToMove.transform.lossyScale;
        }

        // Apply the new scale to the cone
        coneInstance.transform.localScale = newScale;

        // Move the cone so the tip aligns with startPoint after scaling
        Vector3 tipOffset = coneTip.position - coneInstance.transform.position;
        coneInstance.transform.position += (startPoint - coneTip.position);

        // Restore objectToMove's scale only if it's valid
        if (isHolding && objectToMove != null)
        {
            Vector3 newParentLossyScale = objectToMove.transform.parent.lossyScale;
            objectToMove.transform.localScale = new Vector3(
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
        if (selectedOption == SelectionType.SelectionTechnique)
        {
            MonoBehaviour targetScript = (MonoBehaviour)objectToChange.GetComponent("Outline");
            if (targetScript != null)
            {
                targetScript.enabled = isHighlighted;
            }
        }
    }

    public Vector3 GetBottomOrbCenterPoint()
    {
        return coneBottomOrb.transform.position;
    }

    public float GetBottomOrbRadius()
    {
        SphereCollider sphereCollider = coneBottomOrb.GetComponent<SphereCollider>();
        float radius = sphereCollider.radius * coneBottomOrb.transform.lossyScale.x; // x, y, z should all have the same scale
        return radius;
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

    public void ResetVariables()
    {
        rayLength = rayLength_static;
    }

    /*private void OnEnable()
    {
        holdObjectAction.action.Enable();
        setRayLengthRollAction.action.Enable();
        incrementObjectIndexAction.action.Enable();
    }

    private void OnDisable()
    {
        holdObjectAction.action.Disable();
        setRayLengthRollAction.action.Disable();
        incrementObjectIndexAction.action.Disable();
    }*/
}


