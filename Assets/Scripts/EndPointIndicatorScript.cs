using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndPointIndicatorScript : MonoBehaviour
{
    public GameObject floorIndicatorPrefab; // Reference to the floor indicator object
    private GameObject floorIndicator; // Reference to the floor indicator object
    public float rayDistance = 50f;  // Maximum distance the ray can travel
    public LayerMask ignoreLayer;

    void Start()
    {
        floorIndicator = Instantiate(floorIndicatorPrefab, transform.position, Quaternion.identity);
        // floorIndicator.transform.SetParent(transform);
        floorIndicator.layer = LayerMask.NameToLayer("IgnoreRaycastLayer");

        SetPositionFloorIndicator();
    }

    // Update is called once per frame
    void Update()
    {
        SetPositionFloorIndicator();
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
}
