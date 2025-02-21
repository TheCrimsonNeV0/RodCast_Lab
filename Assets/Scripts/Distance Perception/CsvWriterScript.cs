using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class CsvWriterScript : MonoBehaviour
{
    public string csvName = "output.csv";
    private string filePath;

    void Start()
    {
        // Define the file path in a persistent location
        filePath = Path.Combine(Application.persistentDataPath, csvName);
        Debug.Log(filePath);

        // Create the CSV file with a header
        CreateCSVFile();
    }

    void CreateCSVFile()
    {
        if (!File.Exists(filePath))
        {
            File.WriteAllText(filePath, "technique,object_x,object_y,object_z,clicked_x,clicked_y,clicked_z,distance\n"); // Headers for the CSV
        }
    }

    public void RecordData(string technique, Vector3 objectPosition, Vector3 clickedPosition, float distance) // Call from the Manager component
    {
        string newLine = $"{technique},{objectPosition.x},{objectPosition.y},{objectPosition.z}," +
                         $"{clickedPosition.x},{clickedPosition.y},{clickedPosition.z},{distance}\n";

        File.AppendAllText(filePath, newLine);
    }
}
