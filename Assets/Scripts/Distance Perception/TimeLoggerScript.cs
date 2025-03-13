using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using static DataManagerScript;

public class TimeLoggerScript : MonoBehaviour
{
    public string csvName = "times.csv";
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
            File.WriteAllText(filePath, "action,time\n"); // Headers for the CSV
        }
    }

    public void LogEvent(string action, float time)
    {
        string newLine = $"{action},{time}\n";
        File.AppendAllText(filePath, newLine);
    }

    public void LogTargetObjectCreated(float time)
    {
        string newLine = $"target_created,{time}\n";
        File.AppendAllText(filePath, newLine);
    }

    public void LogStartButtonClicked(float time)
    {
        string newLine = $"start_clicked,{time}\n";
        File.AppendAllText(filePath, newLine);
    }

    public void LogCountdownCompleted(float time)
    {
        string newLine = $"countdown_completed,{time}\n";
        File.AppendAllText(filePath, newLine);
    }

    public void LogUserDistanceClick(float time)
    {
        string newLine = $"estimation_performed,{time}\n";
        File.AppendAllText(filePath, newLine);
    }
}
