using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;

public class GoGoHandScript : MonoBehaviour
{
    public enum SelectionType
    {
        SelectionTechnique,
        DistancePerception
    }

    public SelectionType selectedOption;

    public GameObject headset;

    private Vector3 startPoint;
    private Vector3 endPoint;

    public InputActionReference setRayLengthAction;
    public InputActionReference holdObjectAction;
    public InputActionReference getPositionAction;

    public float minRayLength = 1f;
    public float maxRayLength = 10f;

    public float rayLength = 5.0f;

    public float scalingFactor;

    // public GameObject vrHeadset;
    public GameObject handPrefab;
    private GameObject handObject;

    private GameObject objectToMove;

    private bool isAdjustingLength = false;
    private bool isHolding = false;
    
    private Vector3 adjustedForward;
    private Vector3 adjustedUp;
    private Vector3 adjustedRight;
    private Vector3 adjustedOrigin;

    private float positionOffsetZ = 0.0f;
    public float positionSensitivity = 1.0f;

    private float lastPositionX = 0f;
    private float lastPositionY = 0f;
    private float lastPositionZ = 0f;

    // Start is called before the first frame update
    void Start()
    {
        setRayLengthAction.action.Enable();
        setRayLengthAction.action.started += context =>
        {
            isAdjustingLength = true;
            adjustedForward = transform.forward;
            adjustedUp = transform.up;
            adjustedRight = transform.right;
            adjustedOrigin = headset.transform.position;

        };
        setRayLengthAction.action.canceled += context => isAdjustingLength = false; // Stop adjusting length
        setRayLengthAction.action.performed += context =>
        {
            Vector3 currentControllerPosition = transform.position;
            Vector3 currentRelativePosition = CalculateRelativePosition(currentControllerPosition);

            lastPositionX = currentRelativePosition.x;
            lastPositionY = currentRelativePosition.y;
            lastPositionZ = currentRelativePosition.z;
        };

        if (selectedOption == SelectionType.SelectionTechnique)
        {
            holdObjectAction.action.Enable();
            holdObjectAction.action.started += context =>
            {
                isHolding = true;
                if (objectToMove != null)
                {
                    objectToMove.GetComponent<Rigidbody>().isKinematic = true;
                    objectToMove.transform.SetParent(handObject.transform);
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
        }
        else if (selectedOption == SelectionType.DistancePerception)
        {
            getPositionAction.action.Enable();
            getPositionAction.action.performed += context =>
            {
                Debug.Log(GetEndPointPosition());
            };
        }

        UpdateRayCast();
        handObject = Instantiate(handPrefab, endPoint, Quaternion.identity);
        handObject.transform.SetParent(transform);
    }

    // Update is called once per frame
    void Update()
    {
        UpdateRayCast();
        adjustedOrigin = headset.transform.position;

        if (handObject != null)
        {
            handObject.transform.position = endPoint;
            handObject.transform.rotation = transform.rotation;
        }

        if (isAdjustingLength)
        {
            AdjustRayLength(gameObject);
        }
        if (!(isHolding && objectToMove != null))
        {
            objectToMove = GetFirstCollidingObject();
        }
    }

    void UpdateRayCast()
    {
        // Define the startPoint and endPoint along the transform's forward direction
        startPoint = transform.position;
        endPoint = startPoint + transform.forward * rayLength;
    }

    public GameObject GetFirstCollidingObject()
    {
        if (handObject == null)
        {
            Debug.LogError("handObject is not assigned!");
            return null;
        }

        Collider handCollider = handObject.GetComponent<Collider>();
        if (handCollider == null)
        {
            Debug.LogError("No Collider found on handObject!");
            return null;
        }

        Collider[] hitColliders = Physics.OverlapBox(handCollider.bounds.center, handCollider.bounds.extents, handObject.transform.rotation);

        foreach (Collider col in hitColliders)
        {
            if (col.gameObject != handObject) // Exclude self-collision
            {
                return col.gameObject; // Return the first detected object
            }
        }

        return null; // No collisions found
    }

    Vector3 CalculateRelativePosition(Vector3 position)
    {
        // Ensure the provided vectors are normalized
        Vector3 forward = adjustedForward.normalized;
        Vector3 right = adjustedRight.normalized;
        Vector3 up = adjustedUp.normalized;

        // Calculate the position difference relative to the reference point
        Vector3 positionDifference = position - adjustedOrigin;

        // Project the position difference onto the custom axes
        float relativeForward = Vector3.Dot(positionDifference, forward);
        float relativeRight = Vector3.Dot(positionDifference, right);
        float relativeUp = Vector3.Dot(positionDifference, up);

        // Return the recalculated position in the custom coordinate system
        return new Vector3(relativeRight, relativeUp, relativeForward);
    }

    void AdjustRayLength(GameObject controller)
    {
        positionOffsetZ = ParsePositionZ(controller.transform.position);

        if (positionOffsetZ != 0)
        {
            rayLength += positionSensitivity * positionOffsetZ;
            rayLength = Mathf.Clamp(rayLength, minRayLength, maxRayLength);
        }
    }

    float ParsePositionZ(Vector3 currentPosition)
    {
        Vector3 currentRelativePosition = CalculateRelativePosition(currentPosition);
        Vector3 lastRelativePosition = new Vector3(lastPositionX, lastPositionY, lastPositionZ);

        Vector3 positionDifference = currentRelativePosition - lastRelativePosition;
        lastPositionZ = currentRelativePosition.z;
        return positionDifference.z;
    }

    public Vector3 GetEndPointPosition()
    {
        return handObject.transform.position;
    }

    /*
    void OnDisable()
    {
        if (handObject != null)
        {
            handObject.SetActive(false);
        }
    }

    void OnEnable()
    {
        if (handObject != null)
        {
            handObject.SetActive(true);
        }
    }
    */
}
