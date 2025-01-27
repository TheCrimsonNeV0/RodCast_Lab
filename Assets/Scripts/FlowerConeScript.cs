using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class FlowerConeScript : MonoBehaviour
{
    public InputActionReference holdObjectAction;
    public InputActionReference setRayLengthRollAction;

    public GameObject conePrefab;
    public float rayLength = 10.0f;
    
    private Vector3 startPoint; // The peak (pointy end) of the cone
    private Vector3 endPoint; // The center of the cone's base

    public float minRayLength = 1f; // Minimum length of the ray
    public float maxRayLength = 10f; // Maximum length of the ray
    public float rotationSensitivity = 0.1f;

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
            }
        };
        holdObjectAction.action.canceled += context => {
            isHolding = false;
            if (objectToMove != null)
            {
                objectToMove.GetComponent<Rigidbody>().isKinematic = false;
            }
        };
        holdObjectAction.action.performed += context =>
        {
            GameObject nearest = GetNearestObject();
            if (nearest != null)
            {
                Debug.Log($"Nearest object to the sphere is: {nearest.name}");
            }
        };

        setRayLengthRollAction.action.Enable();
        setRayLengthRollAction.action.started += context => isAdjusting_Right = true; // Start adjusting length
        setRayLengthRollAction.action.canceled += context => isAdjusting_Right = false; // Stop adjusting length
        setRayLengthRollAction.action.performed += context =>
        {
            lastRotationZ = transform.rotation.eulerAngles.z; // Reset the last rotation
            lastPositionY = transform.position.y; // Reset the last position
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

        if (isHolding)
        {
            objectToMove.transform.position = coneBottomOrb.transform.position;
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

    void AlignCone()
    {
        // Calculate the offset from the cone's tip (child) to the cone's pivot
        Vector3 tipOffset = coneTip.position - coneInstance.transform.position;

        // Reposition the cone so that the tip aligns with startPoint
        coneInstance.transform.position = startPoint - tipOffset;

        var scale = coneInstance.transform.localScale;
        scale.y = rayLength;
        coneInstance.transform.localScale = scale;
    }

    // Called by the child component
    public void AddCollidingObject(GameObject obj)
    {
        if (!collidingObjects.Contains(obj))
        {
            collidingObjects.Add(obj);
        }
    }

    // Called by the child component
    public void RemoveCollidingObject(GameObject obj)
    {
        if (collidingObjects.Contains(obj))
        {
            collidingObjects.Remove(obj);
        }
    }

    public GameObject GetNearestObject()
    {
        if (collidingObjects.Count == 0)
        {
            Debug.Log("No objects are colliding with the sphere.");
            return null;
        }

        GameObject nearestObject = null;
        float nearestDistance = Mathf.Infinity;
        Vector3 sphereCenter = transform.position;

        foreach (GameObject obj in collidingObjects)
        {
            float distance = Vector3.Distance(sphereCenter, obj.transform.position);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestObject = obj;
            }
        }

        Debug.Log($"Nearest Object: {nearestObject.name}");
        objectToMove = nearestObject;
        return nearestObject;
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
