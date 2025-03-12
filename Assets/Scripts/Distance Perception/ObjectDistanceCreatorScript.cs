using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using static UnityEngine.Rendering.DebugUI;

public class TechniqueData
{
    public string technique;
    public float x;
    public float z;

    public TechniqueData(string technique, float x, float z)
    {
        this.technique = technique;
        this.x = x;
        this.z = z;
    }
}

public class ObjectDistanceCreatorScript : MonoBehaviour
{
    /*
    public bool isGeneratingRandom = true;

    public float xLowInterval = 10;
    public float xHighInterval = 100;

    public float yLowInterval = -5;
    public float yHighInterval = 5;

    public float zLowInterval = -5;
    public float zHighInterval = 5;
    */

    public GameObject timeLoggerObject;
    private TimeLoggerScript timeLogger;

    public GameObject objectPrefab;
    public GameObject viewBlockerPrefab;

    public TextAsset positionsCsv;
    public GameObject techniqueManager;

    private GameObject objectInstance;
    private float xCoordinate;
    private float yCoordinate;
    private float zCoordinate;

    public float offsetHeight = 1;

    private TechniqueData[] coordinates;

    private GameObject viewBlockerInstance;

    private int instanceCount = 0;

    private bool isVisible = false;
    private bool distanceObject_isVisible = true;

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

        viewBlockerInstance = Instantiate(viewBlockerPrefab);
        viewBlockerInstance.SetActive(false);
        techniqueManagerScript.DeactivateAll();
    }

    void Update()
    {
        // TODO: Log time for intervals
        // Time spent in playground, time spent in observation
        // Time between blindfold and click

        // TODO: Show the interaction technique during the countdown and let user make changes after the blindfold

        if (coordinates != null && distanceObject_isVisible)
        {
            if (GameObject.FindGameObjectsWithTag("DistanceObject").Length == 0)
            {
                techniqueManagerScript.DeactivateAll();
                SetBlockerVisibility(false);
                techniqueToActivate = coordinates[instanceCount % coordinates.Length].technique;
                // techniqueManagerScript.ActivateTechnique(coordinates[instanceCount % coordinates.Length].technique);
                Instantiate(objectPrefab, new Vector3(coordinates[instanceCount % coordinates.Length].z, objectPrefab.transform.localScale.y / 2 + offsetHeight, coordinates[instanceCount % coordinates.Length].x), Quaternion.identity);
                
                lastObjectInstantiationTime = Time.time;
                timeLogger.LogTargetObjectCreated(lastObjectInstantiationTime);

                instanceCount++;
            }

            if (viewBlockerInstance.activeSelf && !techniqueManagerScript.IsActiveTechnique(techniqueToActivate))
            {
                techniqueManagerScript.ActivateTechnique(techniqueToActivate);
                techniqueToActivate = "";
            }
        }
    }

    public void ToggleBlockerVisibility()
    {
        isVisible = !isVisible;
        viewBlockerInstance.SetActive(isVisible);
    }

    public void SetBlockerVisibility(bool value)
    {
        isVisible = value;
        viewBlockerInstance.SetActive(isVisible);
    }

    public void ToggleObjectVisibility()
    {
        distanceObject_isVisible = !distanceObject_isVisible;
    }

    public void SetObjectVisibility(bool value)
    {
        distanceObject_isVisible = value;
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

    /*
    void GenerateObjectAtRandomLocation()
    {
        // Create objects and calculate offset distances in a loop

        xCoordinate = Random.Range(xLowInterval, xHighInterval);
        yCoordinate = Random.Range(yLowInterval, yHighInterval);
        zCoordinate = Random.Range(zLowInterval, zHighInterval);

        Vector3 objectInstancePosition = new Vector3(xCoordinate, yCoordinate, zCoordinate);

        objectInstance = Instantiate(objectPrefab, objectInstancePosition, Quaternion.identity);
    }
    */
}