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
        string newLine = $"Target Object Created,{time}\n";
        File.AppendAllText(filePath, newLine);
    }

    public void LogStartButtonClicked(float time)
    {
        string newLine = $"Start Button Clicked,{time}\n";
        File.AppendAllText(filePath, newLine);
    }

    public void LogCountdownCompleted(float time)
    {
        string newLine = $"Countdown Completed,{time}\n";
        File.AppendAllText(filePath, newLine);
    }

    public void LogUserDistanceClick(float time)
    {
        string newLine = $"User Distance Estimation Action Performed,{time}\n";
        File.AppendAllText(filePath, newLine);
    }
}
