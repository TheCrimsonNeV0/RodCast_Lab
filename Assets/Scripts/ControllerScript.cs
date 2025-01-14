using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System.Collections;
using static UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics.HapticsUtility;

public class RayCastVisible : MonoBehaviour
{
    public GameObject leftController;

    public float rayLength = 5.0f; // Length of the RayCast
    private LineRenderer lineRenderer;
    public Material lineMaterial;
    public Material lineMaterialNoHit;

    private Vector3 startPoint;
    private Vector3 endPoint;

    public bool isArcVisible = true;

    public string NonInteractableMaterial = "non_interactable";

    public int arcSegments = 20; // Number of segments in the arc for smoothness
    public float sineFrequency = 1.0f; // Frequency coefficient for the sine wave
    public float sineAmplitude = 2.0f; // Peak height of the sine wave

    public float minRayLength = 1f; // Minimum length of the ray
    public float maxRayLength = 10f; // Maximum length of the ray
    public float minSineAmplitude = 0.5f; // Minimum amplitude limit
    public float maxSineAmplitude = 2.0f; // Maximum amplitude limit

    public float rotationSensitivity = 0.1f; // How much the length changes with rotation
    public float positionSensitivity = 0.1f; // Sensitivity factor for adjusting amplitude

    public float positionSensitivity_Left = 1f; // Sensitivity factor for adjusting amplitude

    public float arcRotationOffsetSensitivity = 0.1f; // Sensitivity factor for adjusting amplitude

    public Quaternion rotation = Quaternion.Euler(0, 0, 0);
    private Vector3 rotatedDirection = new Vector3(0, 0, 0);

    private List<Vector3> pointsAlongLine = new List<Vector3>(); // To store arc points
    public float travelSpeed = 1.0f; // Speed of object movement along the arc

    private bool isMoving = false;
    private bool isAdjusting_Right = false;
    private bool isAdjusting_Left = false;
    private bool isHolding = false;
    private bool isHoldingInHand = false;

    public InputActionReference moveAlongArcAction; // Reference to an Input Action for movement
    public InputActionReference setRayLengthRollAction; // Reference to an Input Action for setting the ray length
    public InputActionReference toggleArcVisibilityAction; // Reference to an Input Action for toggling the arc visibility
    public InputActionReference setRayRotation_Left;
    public InputActionReference holdObjectAction;
    public InputActionReference setSineFrequencyAction;

    private GameObject objectToMove; // The object that will be assigned based on the RayCast hit
    private GameObject heldObject;

    // Temporary variables
    private float lastRotationZ = 0f;
    private float rotationOffset = 0f;
    private float lastPositionY = 0f;
    private float positionOffset = 0f;

    private float left_lastPositionX = 0f;
    private float left_lastPositionY = 0f;
    private float left_lastPositionZ = 0f;

    private float left_positionOffsetX = 0f;
    private float left_positionOffsetY = 0f;
    private float left_positionOffsetZ = 0f;

    private Transform originalParent;
    private Transform originalParentForHeld;

    private Vector3 adjustedForward;
    private Vector3 adjustedUp;
    private Vector3 adjustedRight;
    private Vector3 adjustedOrigin;

    public float dashLineWidth = 0.02f;  // Width of the line
    public Material lineMaterialDashed;
    private LineRenderer lineRendererDashed;

    void Start()
    {
        lastRotationZ = transform.rotation.eulerAngles.z;
        lastPositionY = transform.position.y;

        left_lastPositionX = leftController.transform.position.x;
        left_lastPositionY = leftController.transform.position.y;
        left_lastPositionZ = leftController.transform.position.z;

        rotatedDirection = rotation * transform.forward;

        // Get the LineRenderer component from the GameObject
        lineRenderer = GetComponent<LineRenderer>();
        if (lineRenderer == null)
        {
            Debug.LogError("LineRenderer component missing on " + gameObject.name);
            return;
        }

        lineRenderer.material = lineMaterial;

        // Set up LineRenderer properties
        lineRenderer.startColor = Color.green;
        lineRenderer.endColor = Color.green;
        lineRenderer.startWidth = 0.05f;
        lineRenderer.endWidth = 0.05f;

        // Enable the Input Action and bind the method to the performed event
        moveAlongArcAction.action.Enable();
        moveAlongArcAction.action.started += context => isHoldingInHand = true;
        moveAlongArcAction.action.canceled += context => isHoldingInHand = false;
        moveAlongArcAction.action.performed += OnMoveAlongArcPerformed;

        setRayLengthRollAction.action.Enable();
        setRayLengthRollAction.action.started += context => isAdjusting_Right = true; // Start adjusting length
        setRayLengthRollAction.action.canceled += context => isAdjusting_Right = false; // Stop adjusting length
        setRayLengthRollAction.action.performed += context =>
        {
            lastRotationZ = transform.rotation.eulerAngles.z; // Reset the last rotation
            lastPositionY = transform.position.y; // Reset the last position
        };

        toggleArcVisibilityAction.action.Enable();
        toggleArcVisibilityAction.action.performed += context =>
        {
            isArcVisible = !isArcVisible;
            lineRenderer.enabled = isArcVisible;
            lineRendererDashed.enabled = isArcVisible;
        };

        setRayRotation_Left.action.Enable();
        setRayRotation_Left.action.started += context =>
        {
            isAdjusting_Left = true; // Start adjusting length
            adjustedForward = leftController.transform.forward;
            adjustedUp = leftController.transform.up;
            adjustedRight = leftController.transform.right;
            adjustedOrigin = leftController.transform.position;
        };
        setRayRotation_Left.action.canceled += context => isAdjusting_Left = false; // Stop adjusting length
        setRayRotation_Left.action.performed += context =>
        {
            Vector3 currentControllerPosition = leftController.transform.position;
            Vector3 currentRelativePosition = CalculateRelativePosition(currentControllerPosition);

            left_lastPositionX = currentRelativePosition.x;
            left_lastPositionY = currentRelativePosition.y;
            left_lastPositionZ = currentRelativePosition.z;
        };

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

        setSineFrequencyAction.action.Enable();
        setSineFrequencyAction.action.started += context =>
        {
            sineFrequency = (sineFrequency == 2) ? 1 : 2; // Toggles the sine frequency
        };

        CreateDashedLineRenderer();
    }

    void Update()
    {
        rotatedDirection = rotation * transform.forward;

        DrawRayCast();

        if (isArcVisible && !isMoving)
        {
            if (!isHoldingInHand && heldObject != null)
            {
                ReleaseAnchor();
            }
            else if (isHolding && !isHoldingInHand && objectToMove != null)
            {
                objectToMove.transform.position = pointsAlongLine[pointsAlongLine.Count - 1];
            }

            if (isAdjusting_Right)
            {
                AdjustRayLength();
                AdjustSineAmplitude();
            }
            if (isAdjusting_Left)
            {
                AdjustArcRotation_Left(leftController);
                AdjustSineAmplitude_Left(leftController);
                AdjustRayLength_Left(leftController);
            }
        }

        if (isArcVisible && !isMoving)
        {
            bool isHitObject = RaycastHitObject() && 0 < CheckPointsWithinObject(); // Checks if the move can be started
            DrawArc(isHitObject);
            DrawDashedLine();
        }
    }

    void CreateDashedLineRenderer()
    {
        // Create a new GameObject for the dashed line
        GameObject dashedLine = new GameObject("DashedLine");

        dashedLine.transform.SetParent(transform); // Optional: make it a child of the current GameObject
        dashedLine.transform.localPosition = Vector3.zero;

        // Add a LineRenderer component
        lineRendererDashed = dashedLine.AddComponent<LineRenderer>();

        // Configure LineRenderer
        lineRendererDashed.material = lineMaterialDashed;
        lineRendererDashed.startWidth = dashLineWidth;
        lineRendererDashed.endWidth = dashLineWidth;
        lineRendererDashed.useWorldSpace = true; // Ensure positions are in world space
        lineRendererDashed.positionCount = 0; // Start with no points
    }

    void DrawDashedLine()
    {
        lineRendererDashed.positionCount = 2;
        lineRendererDashed.SetPosition(0, startPoint);
        lineRendererDashed.SetPosition(1, pointsAlongLine[pointsAlongLine.Count - 1]);
    }

    void ResetRayLengthVariables(InputAction.CallbackContext context)
    {
        lastRotationZ = transform.rotation.eulerAngles.z;
        lastPositionY = transform.position.y;
        left_lastPositionX = leftController.transform.position.x;
        left_lastPositionY = leftController.transform.position.y;
        left_lastPositionZ = leftController.transform.position.z;
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

    void AdjustSineAmplitude()
    {
        positionOffset = ParsePositionOffset(transform.position.y);

        // Adjust the sine amplitude based on the position offset
        if (positionOffset != 0)
        {
            sineAmplitude += positionSensitivity * positionOffset;
            sineAmplitude = Mathf.Clamp(sineAmplitude, minSineAmplitude, maxSineAmplitude);
        }
    }

    float ParsePositionOffset(float currentPositionY)
    {
        float positionDifference = currentPositionY - lastPositionY;
        lastPositionY = currentPositionY;
        return positionDifference;
    }

    void AdjustArcRotation_Left(GameObject controller)
    {
        left_positionOffsetX = ParsePositionX_Left(controller.transform.position);

        if (left_positionOffsetX != 0)
        {
            float rotationChange = arcRotationOffsetSensitivity * left_positionOffsetX;
            rotation = new Quaternion(rotation.x, rotation.y + rotationChange, rotation.z, rotation.w);
        }
    }

    float ParsePositionX_Left(Vector3 currentPosition)
    {
        Vector3 currentRelativePosition = CalculateRelativePosition(currentPosition);
        Vector3 lastRelativePosition = new Vector3(left_lastPositionX, left_lastPositionY, left_lastPositionZ);

        Vector3 positionDifference = currentRelativePosition - lastRelativePosition;
        Debug.Log(currentRelativePosition + " " + positionDifference);
        left_lastPositionX = currentRelativePosition.x;
        return positionDifference.x;
    }

    void AdjustSineAmplitude_Left(GameObject controller)
    {
        left_positionOffsetY = ParsePositionY_Left(controller.transform.position);
        if (left_positionOffsetY != 0)
        {
            sineAmplitude += positionSensitivity * left_positionOffsetY;
            sineAmplitude = Mathf.Clamp(sineAmplitude, minSineAmplitude, maxSineAmplitude);
        }
    }

    float ParsePositionY_Left(Vector3 currentPosition)
    {
        Vector3 currentRelativePosition = CalculateRelativePosition(currentPosition);
        Vector3 lastRelativePosition = new Vector3(left_lastPositionX, left_lastPositionY, left_lastPositionZ);

        Vector3 positionDifference = currentRelativePosition - lastRelativePosition;
        Debug.Log(currentRelativePosition + " " + positionDifference);
        left_lastPositionY = currentRelativePosition.y;
        return positionDifference.y;
    }

    void AdjustRayLength_Left(GameObject controller)
    {
        left_positionOffsetZ = ParsePositionZ_Left(controller.transform.position);

        if (left_positionOffsetZ != 0)
        {
            rayLength += positionSensitivity_Left * left_positionOffsetZ;
            rayLength = Mathf.Clamp(rayLength, minRayLength, maxRayLength);
        }
    }

    float ParsePositionZ_Left(Vector3 currentPosition)
    {
        Vector3 currentRelativePosition = CalculateRelativePosition(currentPosition);
        Vector3 lastRelativePosition = new Vector3(left_lastPositionX, left_lastPositionY, left_lastPositionZ);

        Vector3 positionDifference = currentRelativePosition - lastRelativePosition;
        Debug.Log(currentRelativePosition + " " + positionDifference);
        left_lastPositionZ = currentRelativePosition.z;
        return positionDifference.z;
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


    ////////////////////////////////// CONTROLLER POSITION AND ROTATION LOGIC END /////////////////////////////////////////

    void DrawRayCast()
    {
        // Ensure we are setting only 2 points for the RayCast
        lineRenderer.positionCount = 2;

        // Define the start point of the RayCast as the position of the castingObject
        startPoint = transform.position;

        // If the arc has points, set the endpoint to the last point of the sine wave
        if (pointsAlongLine.Count > 0)
        {
            endPoint = pointsAlongLine[pointsAlongLine.Count - 1]; // Use the last point in the arc
        }
        else
        {
            // Fallback in case no arc points are available
            endPoint = startPoint + (rotatedDirection * rayLength);
        }

        Debug.DrawLine(startPoint, endPoint, Color.red);

        // Set the positions in the LineRenderer to make the RayCast visible
        lineRenderer.SetPosition(0, startPoint);
        lineRenderer.SetPosition(1, endPoint);
    }

    void DrawArc(bool isRaycasting)
    {
        // Set the material based on the isRaycasting parameter
        lineRenderer.material = isRaycasting ? lineMaterial : lineMaterialNoHit;

        pointsAlongLine.Clear(); // Clear previous points
        lineRenderer.positionCount = arcSegments + 1; // Set the correct number of points

        // Define the rotation offset
        Quaternion arcRotation = Quaternion.Euler(0, rotation.eulerAngles.y, 0);

        // Loop through each segment to calculate the sine wave points
        for (int i = 0; i <= arcSegments; i++)
        {
            float t = i / (float)arcSegments; // Normalized value between 0 and 1 for interpolation
            Vector3 pointAlongLine = Vector3.Lerp(Vector3.zero, Vector3.forward * rayLength, t); // Interpolated point along the Z axis

            // Apply the sine wave offset along the Y axis
            float arcOffset = Mathf.Sin(Mathf.PI * t * sineFrequency) * sineAmplitude;
            Vector3 arcPoint = pointAlongLine + Vector3.up * arcOffset; // Apply sine wave to Y

            // Apply rotation to the arc point
            arcPoint = arcRotation * arcPoint;

            // Transform the point to world space from local space
            arcPoint = transform.TransformPoint(arcPoint);

            // Record this point in the list and LineRenderer
            pointsAlongLine.Add(arcPoint);
            lineRenderer.SetPosition(i, arcPoint); // Set the position for each segment
        }
    }

    bool RaycastHitObject()
    {
        // Calculate the direction from the startPoint to the endPoint
        Vector3 rayDirection = endPoint - startPoint;

        // Perform the raycast using the direction and length of the ray
        RaycastHit[] hits = Physics.RaycastAll(startPoint, rayDirection.normalized, rayDirection.magnitude);
        if (hits.Length > 0)
        {
            float maxDistance = 0;
            RaycastHit farthestHit = hits[0];

            foreach (var hit in hits)
            {
                float distance = Vector3.Distance(startPoint, hit.point);
                if (distance > maxDistance)
                {
                    maxDistance = distance;
                    farthestHit = hit;
                }
            }

            if (farthestHit.collider.CompareTag(NonInteractableMaterial))
            {
                return false;
            }

            objectToMove = farthestHit.collider.gameObject; // Get the farthest object
            return true;
        }
        if (!isHolding && !isHoldingInHand) objectToMove = null;
        return false;
    }

    void OnMoveAlongArcPerformed(InputAction.CallbackContext context)
    {
        Debug.Log("Move Along Arc action performed"); // Debugging check
        // Trigger movement only if not already moving and an object is hit by RayCast
        if (isArcVisible && !isMoving && !isAdjusting_Right && RaycastHitObject())
        {
            int pointsInsideObject = CheckPointsWithinObject();
            Debug.Log("Number of points inside the object: " + pointsInsideObject);
            if (0 < pointsInsideObject)
            {
                StartCoroutine(MoveObjectAlongArc());
            }
        }
    }

    int CheckPointsWithinObject()
    {
        int count = 0;

        if (objectToMove != null)
        {
            Collider objectCollider = objectToMove.GetComponent<Collider>();
            if (objectCollider != null)
            {
                // Reverse the pointsAlongLine list
                List<Vector3> reversedPoints = new List<Vector3>(pointsAlongLine);
                reversedPoints.Reverse();

                // Calculate the number of points in the first half
                int thresholdCount = reversedPoints.Count / 2;

                // Loop through the desired number of the reversed points
                for (int i = 0; i < thresholdCount; i++)
                {
                    if (objectCollider.bounds.Contains(reversedPoints[i]))
                    {
                        count++;
                    }
                }
            }
        }

        return count;
    }

    IEnumerator MoveObjectAlongArc()
    {
        isMoving = true;

        lineRenderer.enabled = false; // Disable the LineRenderer when starting the movement
        lineRendererDashed.enabled = false;

        // Disable gravity if the object has a Rigidbody
        Rigidbody rb = objectToMove.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.useGravity = false;
            rb.isKinematic = true;
        }

        // Use a local copy of the arc points to avoid modification issues
        List<Vector3> arcPointsCopy = new List<Vector3>(pointsAlongLine);
        arcPointsCopy.Reverse();

        // Move along each point in the arc
        foreach (Vector3 point in arcPointsCopy)
        {
            while (Vector3.Distance(objectToMove.transform.position, point) > 0.01f)
            {
                objectToMove.transform.position = Vector3.MoveTowards(objectToMove.transform.position, point, travelSpeed * Time.deltaTime);
                yield return null; // Wait until the next frame to continue moving
            }
        }

        if (rb != null)
        {
            rb.useGravity = true;
            rb.isKinematic = false;
        }

        if (isHoldingInHand) AnchorObject();

        lineRenderer.enabled = true; // Re-enable the LineRenderer
        lineRendererDashed.enabled = true; // Re-enable the LineRenderer

        isMoving = false;
    }

    private void AnchorObject()
    {
        heldObject = objectToMove;
        originalParentForHeld = heldObject.transform.parent;
        heldObject.transform.SetParent(transform);
        Rigidbody temp_rb = heldObject.GetComponent<Rigidbody>();
        if (temp_rb != null)
        {
            temp_rb.isKinematic = true; // Make object static while grabbed
        }
    }

    private void ReleaseAnchor()
    {
        if (heldObject != null)
        {
            Rigidbody temp_rb = heldObject.GetComponent<Rigidbody>();
            if (temp_rb != null)
            {
                temp_rb.isKinematic = false; // Restore physics
            }

            heldObject.transform.SetParent(originalParentForHeld); // Restore parent
            heldObject = null;
        }
    }

    private void OnEnable()
    {
        moveAlongArcAction.action.Enable();
        moveAlongArcAction.action.performed += OnMoveAlongArcPerformed;
        setRayLengthRollAction.action.Enable();
        toggleArcVisibilityAction.action.Enable();
        setRayRotation_Left.action.Enable();
        setSineFrequencyAction.action.Enable();
    }

    private void OnDisable()
    {
        moveAlongArcAction.action.Disable();
        moveAlongArcAction.action.performed -= OnMoveAlongArcPerformed;
        setRayLengthRollAction.action.Disable();
        toggleArcVisibilityAction.action.Disable();
        setRayRotation_Left.action.Disable();
        setSineFrequencyAction.action.Disable();
    }
}