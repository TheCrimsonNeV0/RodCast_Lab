using UnityEngine;

public class TargetListenerScript : MonoBehaviour
{
    private TargetObjectScript currentTarget;

    void Update()
    {
        if (currentTarget == null)
        {
            GameObject targetObject = GameObject.FindGameObjectWithTag("DenseBatchTargetObject");

            if (targetObject != null)
            {
                TargetObjectScript script = targetObject.GetComponent<TargetObjectScript>();
                if (script != null)
                {
                    currentTarget = script;
                    currentTarget.OnTargetStay += HandleTargetStay;
                    Debug.Log("Listener attached to new target.");
                }
            }
        }
        else if (currentTarget.gameObject == null)
        {
            currentTarget = null; // Target was destroyed
        }
    }

    void HandleTargetStay()
    {
        Debug.Log("Target stayed on surface for 1 second.");
        DestroyAllWithTag("DenseBatchTargetObject");
        DestroyAllWithTag("DenseBatchDecoyObject");
        currentTarget = null;
        // Add your response logic here
    }

    void DestroyAllWithTag(string tag)
    {
        GameObject[] objects = GameObject.FindGameObjectsWithTag(tag);
        foreach (GameObject obj in objects)
        {
            Destroy(obj);
        }
    }

    private void OnEnable()
    {
        if (currentTarget != null)
        {
            currentTarget.OnTargetStay += HandleTargetStay;
        }
    }

    void OnDisable()
    {
        if (currentTarget != null)
        {
            currentTarget.OnTargetStay -= HandleTargetStay;
        }
    }
}
