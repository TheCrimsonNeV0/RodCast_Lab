using UnityEngine;
using System.Collections.Generic;

public class Sorting_TargetListenerScript : MonoBehaviour
{
    public GameObject timeLoggerObject;
    private TimeLoggerScript timeLogger;

    public GameObject sortingBatchCreatorObject;
    private SortingBatchCreatorScript sortingBatchCreator;

    private readonly string[] targetTags = { "SmallTarget", "MediumTarget", "LargeTarget" };
    private Dictionary<string, Sorting_TargetObjectScript> activeTargets = new Dictionary<string, Sorting_TargetObjectScript>();
    private HashSet<string> completedTargets = new HashSet<string>();

    void Start()
    {
        Debug.Log("Target Listener Initialized");
        if (timeLoggerObject != null)
        {
            timeLogger = timeLoggerObject.GetComponent<TimeLoggerScript>();
            sortingBatchCreator = sortingBatchCreatorObject.GetComponent<SortingBatchCreatorScript>();
        }
    }

    void Update()
    {
        foreach (string tag in targetTags)
        {
            // If not already completed and not already tracking the target or it got destroyed
            if (!completedTargets.Contains(tag) && (!activeTargets.ContainsKey(tag) || activeTargets[tag] == null || activeTargets[tag].gameObject == null))
            {
                GameObject targetObject = GameObject.FindGameObjectWithTag(tag);
                if (targetObject != null)
                {
                    Sorting_TargetObjectScript script = targetObject.GetComponent<Sorting_TargetObjectScript>();
                    if (script != null)
                    {
                        activeTargets[tag] = script;
                        script.OnTargetStay += () => HandleTargetStay(tag);
                        Debug.Log($"Listener attached to new target with tag {tag}.");
                    }
                }
            }
        }
    }

    void HandleTargetStay(string tag)
    {
        if (!completedTargets.Contains(tag))
        {
            completedTargets.Add(tag);
            Debug.Log($"Target {tag} stayed on surface.");

            if (completedTargets.Count == targetTags.Length)
            {
                Debug.Log("All targets completed stay condition.");

                foreach (string t in targetTags)
                    DestroyAllWithTag(t);

                DestroyAllWithTag("DenseBatchDecoyObject");
                DestroyAllWithTag("AreaHighlighter");

                float taskCompletionTime = Time.time;
                timeLogger.LogDenseObjectManipulationTaskCompleted(taskCompletionTime);

                completedTargets.Clear();
                activeTargets.Clear();

                DestroyAllWithTag("SortingBatch");
            }
        }
    }

    void DestroyAllWithTag(string tag)
    {
        GameObject[] objects = GameObject.FindGameObjectsWithTag(tag);
        foreach (GameObject obj in objects)
        {
            Destroy(obj);
        }
    }
}
