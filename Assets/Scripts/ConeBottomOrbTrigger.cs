using UnityEngine;

public class ConeBottomOrbTrigger : MonoBehaviour
{
    private FlowerConeScript parentScript;

    private void Start()
    {
        // Get the parent script
        parentScript = GetComponentInParent<FlowerConeScript>();
        if (parentScript == null)
        {
            Debug.LogError("SphereCollisionDetector script not found on parent.");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Notify the parent when an object enters the trigger
        parentScript?.AddCollidingObject(other.gameObject);
    }

    private void OnTriggerExit(Collider other)
    {
        // Notify the parent when an object exits the trigger
        parentScript?.RemoveCollidingObject(other.gameObject);
    }
}