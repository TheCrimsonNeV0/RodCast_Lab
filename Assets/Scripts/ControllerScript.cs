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

    public float arcRotationOffsetSensitivity = 0.1f; // Sensitivity factor for adjusting amplitude

    public Quaternion rotation = Quaternion.Euler(0, 0, 0);
    private Vector3 rotatedDirection = new Vector3(0, 0, 0);

    private List<Vector3> pointsAlongLine = new List<Vector3>(); // To store arc points
    public float travelSpeed = 1.0f; // Speed of object movement along the arc

    private bool isMoving = false;
    private bool isAdjusting_Right = false;
    private bool isAdjusting_Left = false;

    public InputActionReference moveAlongArcAction; // Reference to an Input Action for movement
    public InputActionReference setRayLengthRollAction; // Reference to an Input Action for setting the ray length
    public InputActionReference toggleArcVisibilityAction; // Reference to an Input Action for toggling the arc visibility
    public InputActionReference setRayRotation_Left;

    private GameObject objectToMove; // The object that will be assigned based on the RayCast hit

    // Temporary variables
    private float lastRotationZ = 0f;
    private float rotationOffset = 0f; // Current rotation offset
    private float lastPositionY = 0f; // Initialize with the starting Y position of the controller
    private float positionOffset = 0f; // Current position offset

    private float left_lastPositionX = 0f;
    private float left_positionOffset = 0f;

    void Start()
    {
        lastRotationZ = transform.rotation.eulerAngles.z;
        lastPositionY = transform.position.y;

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
        };

        setRayRotation_Left.action.Enable();
        setRayRotation_Left.action.started += context => isAdjusting_Left = true; // Start adjusting length
        setRayRotation_Left.action.canceled += context => isAdjusting_Left = false; // Stop adjusting length
        setRayRotation_Left.action.performed += context =>
        {
            left_lastPositionX = leftController.transform.position.x;
};
    }

    void Update()
    {
        rotatedDirection = rotation * transform.forward;

        DrawRayCast();

        if (isArcVisible && !isMoving && isAdjusting_Right)
        {
            AdjustRayLength();
            AdjustSineAmplitude();
        }
        else if (isArcVisible && !isMoving && isAdjusting_Left)
        {
            AdjustArcRotation_Left(leftController);
        }

        if (isArcVisible && !isMoving)
        {
            bool isHitObject = RaycastHitObject() && 0 < CheckPointsWithinObject(); // Checks if the move can be started
            DrawArc(isHitObject);
        }
    }

    void ResetRayLengthVariables(InputAction.CallbackContext context)
    {
        lastRotationZ = transform.rotation.eulerAngles.z;
        lastPositionY = transform.position.y;
        left_lastPositionX = leftController.transform.position.x;
    }

    ////////////////////////////////// CONTROLLER POSITION AND ROTATION LOGIC START /////////////////////////////////////////

    void AdjustRayLength()
    {
        rotationOffset = ParseRotationOffset(transform.rotation.eulerAngles.z);
        if (rotationOffset != 0)
        {
            Debug.Log(rotationOffset);
            rayLength += rotationSensitivity * rotationOffset;
            rayLength = Mathf.Clamp(rayLength, minRayLength, maxRayLength);
        }
    }

    float ParseRotationOffset(float currentRotationZ)
    {
        // Calculate the difference in rotation
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

        // Update lastRotationY for the next frame
        lastRotationZ = currentRotationZ;

        // Return the offset (positive for clockwise, negative for counterclockwise)
        return -1 * angleDifference; // Negative for rotation correction
    }

    void AdjustSineAmplitude()
    {
        // Calculate the difference in the Y position
        positionOffset = ParsePositionOffset(transform.position.y);

        // Adjust the sine amplitude based on the position offset
        if (positionOffset != 0)
        {
            Debug.Log(positionOffset);
            sineAmplitude += positionSensitivity * positionOffset;
            sineAmplitude = Mathf.Clamp(sineAmplitude, minSineAmplitude, maxSineAmplitude);
        }
    }

    float ParsePositionOffset(float currentPositionY)
    {
        // Calculate the difference in position
        float positionDifference = currentPositionY - lastPositionY;

        // Update lastPositionY for the next frame
        lastPositionY = currentPositionY;

        // Return the position offset (positive for upward, negative for downward)
        return positionDifference;
    }

    void AdjustArcRotation_Left(GameObject controller)
    {
        left_positionOffset = ParsePositionOffset_Left(controller.transform.position.x);

        if (left_positionOffset != 0)
        {
            Debug.Log(left_positionOffset);
            rotation = new Quaternion(rotation.x, rotation.y + arcRotationOffsetSensitivity * left_positionOffset, rotation.z, rotation.w);
            sineAmplitude = Mathf.Clamp(sineAmplitude, minSineAmplitude, maxSineAmplitude);
        }
    }

    float ParsePositionOffset_Left(float currentPositionX)
    {
        float positionDifference = currentPositionX - left_lastPositionX;
        left_lastPositionX = currentPositionX;
        return positionDifference;
    }

    ////////////////////////////////// CONTROLLER POSITION AND ROTATION LOGIC END /////////////////////////////////////////

    void DrawRayCast()
    {
        // Ensure we are setting only 2 points for the RayCast
        lineRenderer.positionCount = 2;

        // Define the start point of the RayCast as the position of the castingObject
        startPoint = transform.position;

        // Calculate the endPoint using the rotated direction
        Vector3 endPoint = startPoint + rotatedDirection * rayLength;

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
        RaycastHit[] hits = Physics.RaycastAll(startPoint, rotatedDirection, rayLength);
        if (hits.Length > 0)
        {
            float maxDistance = 0;
            RaycastHit farthestHit = hits[0];

            foreach (var hit in hits)
            {
                if (hit.collider.CompareTag(NonInteractableMaterial))
                {
                    return false;
                }

                float distance = Vector3.Distance(startPoint, hit.point);
                if (distance > maxDistance)
                {
                    maxDistance = distance;
                    farthestHit = hit;
                }
            }

            objectToMove = farthestHit.collider.gameObject; // Get the farthest object
            return true;
        }
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

                // Calculate the number of points in the first quarter
                int quarterCount = reversedPoints.Count / 4;

                // Loop through the first quarter of the reversed points
                for (int i = 0; i < quarterCount; i++)
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

        // Disable the LineRenderer when starting the movement
        lineRenderer.enabled = false;

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

        // Re-enable gravity and LineRenderer after movement
        if (rb != null)
        {
            rb.useGravity = true;
            rb.isKinematic = false;
        }

        lineRenderer.enabled = true; // Re-enable the LineRenderer

        isMoving = false;
    }

    private void OnEnable()
    {
        moveAlongArcAction.action.Enable();
        moveAlongArcAction.action.performed += OnMoveAlongArcPerformed;
        setRayLengthRollAction.action.Enable();
        toggleArcVisibilityAction.action.Enable();
    }

    private void OnDisable()
    {
        moveAlongArcAction.action.Disable();
        moveAlongArcAction.action.performed -= OnMoveAlongArcPerformed;
        setRayLengthRollAction.action.Disable();
        toggleArcVisibilityAction.action.Disable();
    }
}