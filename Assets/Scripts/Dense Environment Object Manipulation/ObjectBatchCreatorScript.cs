using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using static UnityEngine.Rendering.DebugUI;
using RodCast_Lab.Data;

public class ObjectBatchCreatorScript : MonoBehaviour
{
    public GameObject timeLoggerObject; // Might require editing
    private TimeLoggerScript timeLogger; // Might require editing

    public GameObject objectPrefab;
    public GameObject areaHighlighterPrefab;

    public TextAsset positionsCsv;
    public GameObject techniqueManager;

    private GameObject objectInstance;
    private GameObject areaHighlighterInstance;

    public float offsetHeight = 1;

    public TextMeshProUGUI instanceCountText;

    private TechniqueData[] coordinates;

    private int instanceCount = 0;

    private bool isVisible = false;
    private bool batch_isVisible = false;
    private bool isObjectCreationActive = false;

    private TechniqueManagerScript techniqueManagerScript;

    private string techniqueToActivate = "";

    private float lastObjectInstantiationTime;

    // Start is called before the first frame update
    void Start()
    {
        if (timeLoggerObject != null)
        {
            timeLogger = timeLoggerObject.GetComponent<TimeLoggerScript>();
        }

        coordinates = ReadCSV(positionsCsv);

        techniqueManagerScript = techniqueManager.GetComponent<TechniqueManagerScript>();

        areaHighlighterInstance = Instantiate(areaHighlighterPrefab);
        areaHighlighterInstance.SetActive(false);
        techniqueManagerScript.DeactivateAll();
    }

    void Update()
    {
        if (isObjectCreationActive)
        {
            if (coordinates != null)
            {
                // Add new tag 'DenseBatchTargetObject' and implement the same logic as Distance Perception task
                if (GameObject.FindGameObjectsWithTag("AreaHighlighter").Length == 0 && GameObject.FindGameObjectsWithTag("DenseBatchTargetObject").Length == 0)
                {
                    techniqueManagerScript.DeactivateAll();

                    // Area tag: AreaHighlighter
                    // Highlight should be on when no object exists
                    //SetAreaVisibility(true);

                    // TODO: Make a sequential flow to enable the area highlight and dense object back after button click
                    areaHighlighterInstance.SetActive(true);
                    areaHighlighterInstance.transform.position = new Vector3(coordinates[instanceCount % coordinates.Length].z, objectPrefab.transform.localScale.y / 2 + offsetHeight, coordinates[instanceCount % coordinates.Length].x);
                    areaHighlighterInstance.transform.rotation = Quaternion.identity;
                    //Instantiate(objectPrefab, new Vector3(coordinates[instanceCount % coordinates.Length].z, objectPrefab.transform.localScale.y / 2 + offsetHeight, coordinates[instanceCount % coordinates.Length].x), Quaternion.identity);


                    if (GameObject.FindGameObjectsWithTag("AreaHighlighter").Length > 0)
                    {
                        techniqueToActivate = coordinates[instanceCount % coordinates.Length].technique;
                        techniqueManagerScript.ActivateTechnique(coordinates[instanceCount % coordinates.Length].technique);
                    }
                    
                    objectInstance = Instantiate(objectPrefab, new Vector3(coordinates[instanceCount % coordinates.Length].z, objectPrefab.transform.localScale.y / 2 + offsetHeight, coordinates[instanceCount % coordinates.Length].x), Quaternion.identity);
                    objectInstance.SetActive(false);

                    lastObjectInstantiationTime = Time.time;
                    timeLogger.LogTargetObjectCreated(lastObjectInstantiationTime);

                    instanceCount++;
                    instanceCountText.text = "" + instanceCount;
                }

                /*
                else
                {
                    if (areaHighlighterInstance.activeSelf)
                    {
                        areaHighlighterInstance.SetActive(false);
                    }
                }
                */

                // TODO: Add new logic here to highlight area and spawn the batch back to back
                /*
                if (viewBlockerInstance.activeSelf && !techniqueManagerScript.IsActiveTechnique(techniqueToActivate))
                {
                    techniqueManagerScript.ActivateTechnique(techniqueToActivate);
                    techniqueToActivate = "";
                }
                */
            }
        }
    }

    public void ToggleAreaVisibility()
    {
        isVisible = !isVisible;
        areaHighlighterInstance.SetActive(isVisible);
    }

    public void SetAreaVisibility(bool value)
    {
        isVisible = value;
        areaHighlighterInstance.SetActive(isVisible);
    }

    public void ToggleObjectVisibility()
    {
        batch_isVisible = !batch_isVisible;
        objectInstance.SetActive(batch_isVisible);
    }

    public void SetObjectVisibility(bool value)
    {
        batch_isVisible = value;
        objectInstance.SetActive(batch_isVisible);
    }

    TechniqueData[] ReadCSV(TextAsset file)
    {
        string[] lines = file.text.Split('\n'); // Split file into lines
        List<TechniqueData> dataList = new List<TechniqueData>(); // Store technique, x, z

        for (int i = 1; i < lines.Length; i++) // Start from index 1 to skip header
        {
            string line = lines[i].Trim();
            if (string.IsNullOrWhiteSpace(line)) continue; // Skip empty lines

            string[] values = line.Split(','); // Split each line by comma

            if (values.Length >= 3 &&
                !string.IsNullOrWhiteSpace(values[0]) &&
                float.TryParse(values[1], out float x) &&
                float.TryParse(values[2], out float z))
            {
                dataList.Add(new TechniqueData(values[0], x, z)); // Store in list
            }
            else
            {
                Debug.LogError($"Invalid CSV format at line {i + 1}: " + line);
            }
        }
        return dataList.ToArray();
    }

    public float GetLastObjectInstantiationTime()
    {
        return lastObjectInstantiationTime;
    }

    public void StartObjectCreation()
    {
        isObjectCreationActive = true;
    }

    public void StopObjectCreation()
    {
        isObjectCreationActive = false;
    }

    public void ToggleObjectCreation()
    {
        isObjectCreationActive = !isObjectCreationActive;
    }
}