using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using static UnityEngine.Rendering.DebugUI;
using RodCast_Lab.Data;

public class ObjectBatchCreatorScript : MonoBehaviour
{
    public GameObject timeLoggerObject;
    private TimeLoggerScript timeLogger;

    public GameObject objectPrefab;
    public GameObject areaHighlighterPrefab;

    public TextAsset positionsCsv;
    public GameObject techniqueManager;

    private GameObject objectInstance;
    private GameObject areaHighlighterInstance;

    public GameObject dense_csvWriter;
    private Dense_CsvWriterScript dense_csvWriterScript;

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

    private int resetCountInstance = 0;
    private Vector3 coordinateRecord = new Vector3();

    // Start is called before the first frame update
    void Start()
    {
        if (timeLoggerObject != null)
        {
            timeLogger = timeLoggerObject.GetComponent<TimeLoggerScript>();
        }

        if (dense_csvWriter != null)
        {
            dense_csvWriterScript = dense_csvWriter.GetComponent<Dense_CsvWriterScript>();
        }

        coordinates = ReadCSV(positionsCsv);

        techniqueManagerScript = techniqueManager.GetComponent<TechniqueManagerScript>();

        areaHighlighterInstance = Instantiate(areaHighlighterPrefab);
        areaHighlighterInstance.transform.position = new Vector3(0, areaHighlighterPrefab.transform.localScale.y / 2, areaHighlighterPrefab.transform.localScale.z * 3 / 2);
        areaHighlighterInstance.transform.rotation = Quaternion.identity;
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
                    // Get collision count
                    if (instanceCount >= 1)
                    {
                        if (dense_csvWriterScript != null)
                        {
                            // Vector3 coordinateVector = new Vector3(coordinates[(instanceCount - 1) % coordinates.Length].z, objectPrefab.transform.localScale.y / 2 + offsetHeight, coordinates[(instanceCount - 1) % coordinates.Length].x);
                            dense_csvWriterScript.RecordData(techniqueManagerScript.GetActiveTechnique(), coordinateRecord, resetCountInstance);
                        }
                    }
                    resetCountInstance = 0; // Reset before every instance

                    techniqueManagerScript.DeactivateAll();
                    areaHighlighterInstance.SetActive(true);
                    // areaHighlighterInstance.transform.position = new Vector3(coordinates[instanceCount % coordinates.Length].z, objectPrefab.transform.localScale.y / 2 + offsetHeight, coordinates[instanceCount % coordinates.Length].x);
                    // areaHighlighterInstance.transform.rotation = Quaternion.identity;

                    if (GameObject.FindGameObjectsWithTag("AreaHighlighter").Length > 0)
                    {
                        techniqueToActivate = coordinates[instanceCount % coordinates.Length].technique;
                        techniqueManagerScript.ActivateTechnique(coordinates[instanceCount % coordinates.Length].technique);
                    }

                    // objectInstance = Instantiate(objectPrefab, new Vector3(coordinates[instanceCount % coordinates.Length].z, objectPrefab.transform.localScale.y / 2 + offsetHeight, coordinates[instanceCount % coordinates.Length].x), Quaternion.identity);
                    float bufferAreaLength = 1.5f;
                    coordinateRecord = GetRandomPointInCube(areaHighlighterInstance.transform.position, areaHighlighterPrefab.transform.localScale.x - bufferAreaLength);
                    objectInstance = Instantiate(objectPrefab, coordinateRecord, Quaternion.identity);
                    
                    // TODO: Read from CSV and determine density level
                    objectInstance.GetComponent<DenseBatchScript>().ApplyDensityLevel(DenseBatchScript.DensityLevel.Low);
                    
                    objectInstance.SetActive(false);

                    lastObjectInstantiationTime = Time.time;
                    timeLogger.LogAreaDisplayStarted(lastObjectInstantiationTime);

                    instanceCount++;
                    instanceCountText.text = "" + instanceCount;
                }             
            }
        }
    }

    Vector3 GetRandomPointInCube(Vector3 center, float edgeLength)
    {
        float halfEdge = edgeLength / 2f;

        float x = Random.Range(center.x - halfEdge, center.x + halfEdge);
        float y = Random.Range(center.y - halfEdge, center.y + halfEdge);
        float z = Random.Range(center.z - halfEdge, center.z + halfEdge);

        return new Vector3(x, y, z);
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

    public void IncrementResetCount()
    {
        // Detects collision
        resetCountInstance += 1;
        Debug.Log("Collision detected");
    }
}