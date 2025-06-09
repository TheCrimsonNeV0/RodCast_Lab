using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Dense_CsvWriterScript : MonoBehaviour
{
    public string csvName = "output_dense.csv";
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
            File.WriteAllText(filePath, "technique,object_x,object_y,object_z,reset_count\n"); // Headers for the CSV
        }
    }

    public void RecordData(string technique, Vector3 objectPosition, int resetCount) // Call from the Manager component
    {
        string newLine = $"{technique},{objectPosition.x},{objectPosition.y},{objectPosition.z}," +
                         $"{resetCount}\n";

        File.AppendAllText(filePath, newLine);
    }
}
