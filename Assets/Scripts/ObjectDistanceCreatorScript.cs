using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using static UnityEngine.Rendering.DebugUI;

public class ObjectDistanceCreatorScript : MonoBehaviour
{
    public bool isGeneratingRandom = true;

    public float xLowInterval = 10;
    public float xHighInterval = 100;

    public float yLowInterval = -5;
    public float yHighInterval = 5;

    public float zLowInterval = -5;
    public float zHighInterval = 5;

    public GameObject objectPrefab;
    public GameObject viewBlockerPrefab;

    public TextAsset positionsCsv;

    private GameObject objectInstance;
    private float xCoordinate;
    private float yCoordinate;
    private float zCoordinate;

    private Vector2[] coordinates;

    public TextAsset csvFile; // Assign in the Unity Inspector

    private GameObject viewBlockerInstance;

    // Start is called before the first frame update
    void Start()
    {
        if (isGeneratingRandom)
        {
            GenerateObjectAtRandomLocation();
        }
        else
        {
            coordinates = ReadCSV(positionsCsv);
        }

        viewBlockerInstance = Instantiate(viewBlockerPrefab);
        viewBlockerInstance.SetActive(false);
    }

    public void ToggleBlockerVisibility()
    {
        viewBlockerInstance.SetActive(!viewBlockerInstance.activeSelf);
    }

    public void SetBlockerVisibility(bool value)
    {
        viewBlockerInstance.SetActive(value);
    }

    Vector2[] ReadCSV(TextAsset file)
    {
        string[] lines = file.text.Split('\n'); // Split file into lines
        List<Vector2> coordinates = new List<Vector2>(); // Store (x, z) coordinates

        foreach (string line in lines)
        {
            if (string.IsNullOrWhiteSpace(line)) continue; // Skip empty lines

            string[] values = line.Split(','); // Split each line by comma

            if (values.Length == 2 && float.TryParse(values[0], out float x) && float.TryParse(values[1], out float z))
            {
                coordinates.Add(new Vector2(x, z)); // Store as Vector2 (x, z)
            }
            else
            {
                Debug.LogError("Invalid CSV format: " + line);
            }
        }
        return coordinates.ToArray();
    }

    void GenerateObjectAtRandomLocation()
    {
        // Create objects and calculate offset distances in a loop

        xCoordinate = Random.Range(xLowInterval, xHighInterval);
        yCoordinate = Random.Range(yLowInterval, yHighInterval);
        zCoordinate = Random.Range(zLowInterval, zHighInterval);

        Vector3 objectInstancePosition = new Vector3(xCoordinate, yCoordinate, zCoordinate);

        objectInstance = Instantiate(objectPrefab, objectInstancePosition, Quaternion.identity);
    }
}