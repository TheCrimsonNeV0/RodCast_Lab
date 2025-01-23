using UnityEngine;

public class FlowerConeScript : MonoBehaviour
{
    public GameObject conePrefab; // Assign your cone prefab in the inspector
    public float rayLength = 5.0f;
    public Vector3 startPoint;  // The peak (pointy end) of the cone
    public Vector3 endPoint;    // The center of the cone's base

    private GameObject coneInstance;    // Instance of the cone prefab
    private Transform coneTip;          // Reference to the child (tip) GameObject

    // Start is called before the first frame update
    void Start()
    {
        if (conePrefab != null)
        {
            coneInstance = Instantiate(conePrefab, transform);
            coneTip = coneInstance.transform.GetChild(0);
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
    }
}
