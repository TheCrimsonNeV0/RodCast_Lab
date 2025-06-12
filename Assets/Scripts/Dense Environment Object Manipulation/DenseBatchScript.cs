using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class DenseBatchScript : MonoBehaviour
{
    public enum DensityLevel
    {
        Low,
        Medium,
        High
    }

    public int LOW_DENSITY_LOWER_BOUND_INNER = 4;
    public int LOW_DENSITY_UPPER_BOUND_INNER = 7;
    public int LOW_DENSITY_LOWER_BOUND_OUTER = 0;
    public int LOW_DENSITY_UPPER_BOUND_OUTER = 0;

    public int MEDIUM_DENSITY_LOWER_BOUND_INNER = 4;
    public int MEDIUM_DENSITY_UPPER_BOUND_INNER = 6;
    public int MEDIUM_DENSITY_LOWER_BOUND_OUTER = 7;
    public int MEDIUM_DENSITY_UPPER_BOUND_OUTER = 9;

    public int HIGH_DENSITY_LOWER_BOUND_INNER = 6;
    public int HIGH_DENSITY_UPPER_BOUND_INNER = 8;
    public int HIGH_DENSITY_LOWER_BOUND_OUTER = 10;
    public int HIGH_DENSITY_UPPER_BOUND_OUTER = 15;

    public DensityLevel densityLevel = DensityLevel.Low;

    private static System.Random random = new System.Random();
    private bool hasInitialized = false;

    void Start()
    {
        DeactivateAllDecoys();

        // Always activate target
        Transform target = transform.Find("Target");
        if (target != null)
            target.gameObject.SetActive(true);
    }

    void Update()
    {
        if (!hasInitialized)
        {
            ApplyDensityLevel(densityLevel);
            hasInitialized = true;
        }
    }

    public void ApplyDensityLevel(DensityLevel level)
    {
        DeactivateAllDecoys();

        int innerLower = 0, innerUpper = 0, outerLower = 0, outerUpper = 0;

        switch (level)
        {
            case DensityLevel.Low:
                innerLower = LOW_DENSITY_LOWER_BOUND_INNER;
                innerUpper = LOW_DENSITY_UPPER_BOUND_INNER;
                outerLower = LOW_DENSITY_LOWER_BOUND_OUTER;
                outerUpper = LOW_DENSITY_UPPER_BOUND_OUTER;
                break;
            case DensityLevel.Medium:
                innerLower = MEDIUM_DENSITY_LOWER_BOUND_INNER;
                innerUpper = MEDIUM_DENSITY_UPPER_BOUND_INNER;
                outerLower = MEDIUM_DENSITY_LOWER_BOUND_OUTER;
                outerUpper = MEDIUM_DENSITY_UPPER_BOUND_OUTER;
                break;
            case DensityLevel.High:
                innerLower = HIGH_DENSITY_LOWER_BOUND_INNER;
                innerUpper = HIGH_DENSITY_UPPER_BOUND_INNER;
                outerLower = HIGH_DENSITY_LOWER_BOUND_OUTER;
                outerUpper = HIGH_DENSITY_UPPER_BOUND_OUTER;
                break;
        }

        // Enable inner decoys
        int innerCount = GenerateRandomInt(innerLower, innerUpper);
        int[] innerIndices = GetRandomNumbers(16, innerCount);
        foreach (int i in innerIndices)
        {
            Transform item = transform.Find(i.ToString());
            if (item != null)
                item.gameObject.SetActive(true);
        }

        // Enable outer decoys
        int outerCount = GenerateRandomInt(outerLower, outerUpper);
        ActivateChildrenFromPairs(outerCount, 6, 9);

        // Always activate target
        Transform target = transform.Find("Target");
        if (target != null)
            target.gameObject.SetActive(true);
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

    public static List<(int, int)> GenerateUniquePairs(int count, int outerUpperBound, int childUpperBound)
    {
        int totalCombinations = outerUpperBound * childUpperBound;
        if (count > totalCombinations)
            throw new ArgumentException("Not enough unique pairs possible for the given bounds.");

        HashSet<(int, int)> uniquePairs = new HashSet<(int, int)>();

        while (uniquePairs.Count < count)
        {
            int first = random.Next(1, outerUpperBound + 1);
            int second = random.Next(1, childUpperBound + 1);
            uniquePairs.Add((first, second));
        }

        return new List<(int, int)>(uniquePairs);
    }

    public void ActivateChildrenFromPairs(int pairCount, int outerUpperBound, int childUpperBound)
    {
        for (int i = 1; i <= outerUpperBound; i++)
        {
            Transform outer = transform.Find("outer" + i);
            if (outer != null)
            {
                outer.gameObject.SetActive(false);
            }
        }

        var pairs = GenerateUniquePairs(pairCount, outerUpperBound, childUpperBound);

        foreach (var (outerIndex, childIndex) in pairs)
        {
            string parentName = "outer" + outerIndex;
            string childName = "outer_" + childIndex;

            Transform parent = transform.Find(parentName);
            if (parent != null)
            {
                parent.gameObject.SetActive(true);

                foreach (Transform child in parent)
                {
                    child.gameObject.SetActive(false);
                }

                Transform targetChild = parent.Find(childName);
                if (targetChild != null)
                {
                    targetChild.gameObject.SetActive(true);
                }
                else
                {
                    Debug.LogWarning($"Child '{childName}' not found under parent '{parentName}'.");
                }
            }
            else
            {
                Debug.LogWarning($"Parent '{parentName}' not found under '{gameObject.name}'.");
            }
        }
    }
}
