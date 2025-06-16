using UnityEngine;
using System.Collections.Generic;

public class Sorting_TargetListenerScript : MonoBehaviour
{
    public GameObject timeLoggerObject;
    private TimeLoggerScript timeLogger;

    private Sorting_TargetObjectScript currentTarget;

    private readonly string[] targetTags = { "SmallTarget", "MediumTarget", "LargeTarget" };

    void Start()
    {
        if (timeLoggerObject != null)
        {
            timeLogger = timeLoggerObject.GetComponent<TimeLoggerScript>();
        }
    }

    void Update()
    {
        if (currentTarget == null)
        {
            foreach (string tag in targetTags)
            {
                GameObject targetObject = GameObject.FindGameObjectWithTag(tag);
                if (targetObject != null)
                {
                    Sorting_TargetObjectScript script = targetObject.GetComponent<Sorting_TargetObjectScript>();
                    if (script != null)
                    {
                        currentTarget = script;
                        currentTarget.OnTargetStay += HandleTargetStay;
                        Debug.Log($"Listener attached to new target with tag {tag}.");
                        break;
                    }
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

        foreach (string tag in targetTags)
        {
            DestroyAllWithTag(tag);
        }

        DestroyAllWithTag("DenseBatchDecoyObject");
        currentTarget = null;

        float taskCompletionTime = Time.time;
        timeLogger.LogDenseObjectManipulationTaskCompleted(taskCompletionTime);
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
