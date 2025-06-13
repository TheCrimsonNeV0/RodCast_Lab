using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class SortingBatchScript : MonoBehaviour
{
    public enum DensityLevel
    {
        Low,
        Medium,
        High
    }

    public GameObject targetPrefab;

    public int LOW_DENSITY_LOWER_BOUND = 4;
    public int LOW_DENSITY_UPPER_BOUND = 7;

    public int MEDIUM_DENSITY_LOWER_BOUND = 4;
    public int MEDIUM_DENSITY_UPPER_BOUND = 6;

    public int HIGH_DENSITY_LOWER_BOUND = 6;
    public int HIGH_DENSITY_UPPER_BOUND = 8;

    public DensityLevel densityLevel = DensityLevel.Low;

    private static System.Random random = new System.Random();

    private int[] activeObjects = new int[0];

    // Track the last applied level to detect changes
    private DensityLevel? lastAppliedLevel = null;

    void Start()
    {
        DeactivateAllDecoys();

        /*
        // Always activate target
        Transform target = transform.Find("Target");
        if (target != null)
            target.gameObject.SetActive(true);
        */
    }

    void Update()
    {
        if (lastAppliedLevel != densityLevel)
        {
            Debug.Log($"[DenseBatchScript] Applying density level: {densityLevel}");
            ApplyDensityLevel(densityLevel);
            lastAppliedLevel = densityLevel;
        }
    }

    public void ApplyDensityLevel(DensityLevel level)
    {
        DeactivateAllDecoys();

        int lower = 0, upper = 0;

        switch (level)
        {
            case DensityLevel.Low:
                lower = LOW_DENSITY_LOWER_BOUND;
                upper = LOW_DENSITY_UPPER_BOUND;
                break;
            case DensityLevel.Medium:
                lower = MEDIUM_DENSITY_LOWER_BOUND;
                upper = MEDIUM_DENSITY_UPPER_BOUND;
                break;
            case DensityLevel.High:
                lower = HIGH_DENSITY_LOWER_BOUND;
                upper = HIGH_DENSITY_UPPER_BOUND;
                break;
        }

        int count = GenerateRandomInt(lower, upper);
        activeObjects = GetRandomNumbers(25, count);
        foreach (int i in activeObjects)
        {
            Transform item = transform.Find(i.ToString());
            if (item != null)
                item.gameObject.SetActive(true);
        }

        CreateTargetsFromEnabled();
    }

    void CreateTargetsFromEnabled()
    {
        int[] targetIndices = GetUniqueValues(activeObjects, 3); // Select 3 targets

        foreach (int index in targetIndices)
        {
            Transform original = transform.Find(index.ToString());
            if (original != null)
            {
                Vector3 position = original.position;
                Quaternion rotation = original.rotation;

                original.gameObject.SetActive(false); // Disable the original object
                Instantiate(targetPrefab, position, rotation, transform); // Spawn targetPrefab at the same location
            }
            else
            {
                Debug.LogWarning($"No object found with name {index}");
            }
        }
    }

    void AdjustObjectScale(GameObject obj, float percentage)
    {
        if (percentage <= -100f)
        {
            Debug.LogWarning("Percentage must be greater than -100.");
            return;
        }

        float scaleFactor = 1f + (percentage / 100f);
        obj.transform.localScale = obj.transform.localScale * scaleFactor;
    }

    public int[] GetUniqueValues(int[] inputArray, int count)
    {
        // Get distinct values and take the specified count
        return inputArray.Distinct().Take(count).ToArray();
    }

    public void DeactivateAllDecoys()
    {
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(false);
        }

        for (int i = 1; i <= 6; i++)
        {
            Transform outer = transform.Find("outer" + i);
            if (outer != null)
                outer.gameObject.SetActive(false);
        }
    }

    static int[] GetRandomNumbers(int upperBound, int numberCount)
    {
        if (upperBound < numberCount || upperBound < 1 || numberCount < 1)
            return new int[0];

        System.Random rand = new System.Random();
        List<int> numbers = Enumerable.Range(1, upperBound).ToList();
        return numbers.OrderBy(x => rand.Next()).Take(numberCount).ToArray();
    }

    static int GenerateRandomInt(int lowerBound, int upperBound)
    {
        if (lowerBound > upperBound)
            throw new ArgumentException("Lower bound must be less than or equal to upper bound.");

        return random.Next(lowerBound, upperBound + 1);
    }
}
