using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndPointIndicatorScript : MonoBehaviour
{
    public GameObject floorIndicatorPrefab; // Reference to the floor indicator object
    private GameObject floorIndicator; // Reference to the floor indicator object
    public float scaleFactor = 1.0f;
    public bool isResizingFromParent = true;
    public float rayDistance = 50f;  // Maximum distance the ray can travel
    public LayerMask ignoreLayer;

    void Start()
    {
        floorIndicator = Instantiate(floorIndicatorPrefab, transform.position, Quaternion.identity);

        Vector3 currentScale = floorIndicator.transform.localScale;
        floorIndicator.transform.localScale = new Vector3(currentScale.x * scaleFactor, currentScale.y, currentScale.z * scaleFactor);
        // floorIndicator.transform.SetParent(transform);
        floorIndicator.layer = LayerMask.NameToLayer("IgnoreRaycastLayer");

        SetPositionFloorIndicator();
    }

    // Update is called once per frame
    void Update()
    {
        SetPositionFloorIndicator();
        if (isResizingFromParent)
        {
            SetSizeFloorIndicator();
        }
        floorIndicator.SetActive((transform.position.y >= 0));
    }

    void SetPositionFloorIndicator()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, rayDistance, ~ignoreLayer))
        {
            // Update the floorIndicator's position to the hit point
            Vector3 newPosition = floorIndicator.transform.position;
            newPosition.x = transform.position.x;
            newPosition.y = hit.point.y;
            newPosition.z = transform.position.z;
            floorIndicator.transform.position = newPosition;
        }
    }

    void SetSizeFloorIndicator()
    {
        // Get the world size of the object using lossyScale
        Vector3 worldSize = new Vector3(transform.lossyScale.x, transform.lossyScale.y, transform.lossyScale.z);

        // Convert world size to local scale relative to floorIndicator's parent
        Vector3 parentScale = floorIndicator.transform.parent ? floorIndicator.transform.parent.lossyScale : Vector3.one;

        floorIndicator.transform.localScale = new Vector3(
            worldSize.x / parentScale.x,
            floorIndicator.transform.localScale.y,
            worldSize.z / parentScale.z
        );
    }


    private void OnDisable()
    {
        if (floorIndicator != null)
        {
            floorIndicator.SetActive(false);
        }
    }

    private void OnEnable()
    {
        if (floorIndicator != null)
        {
            floorIndicator.SetActive(true);
        }
    }
}
