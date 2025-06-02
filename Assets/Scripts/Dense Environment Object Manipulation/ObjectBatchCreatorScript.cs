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
    private bool batch_isVisible = true;
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
            if (coordinates != null && batch_isVisible)
            {
                if (GameObject.FindGameObjectsWithTag("DistanceObject").Length == 0)
                {
                    techniqueManagerScript.DeactivateAll();
                    SetAreaVisibility(false);
                    techniqueToActivate = coordinates[instanceCount % coordinates.Length].technique;
                    // techniqueManagerScript.ActivateTechnique(coordinates[instanceCount % coordinates.Length].technique);
                    Instantiate(objectPrefab, new Vector3(coordinates[instanceCount % coordinates.Length].z, objectPrefab.transform.localScale.y / 2 + offsetHeight, coordinates[instanceCount % coordinates.Length].x), Quaternion.identity);

                    lastObjectInstantiationTime = Time.time;
                    timeLogger.LogTargetObjectCreated(lastObjectInstantiationTime);

                    instanceCount++;
                    instanceCountText.text = "" + instanceCount;
                }

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
    }

    public void SetObjectVisibility(bool value)
    {
        batch_isVisible = value;
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