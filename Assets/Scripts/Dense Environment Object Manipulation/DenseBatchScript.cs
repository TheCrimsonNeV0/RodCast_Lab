using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class DenseBatchScript : MonoBehaviour
{
    private static System.Random random = new System.Random();
    void Start()
    {
        int[] randomNumbers = GetRandomNumbers(16, GenerateRandomInt(5, 8));

        // Disable all children
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(false);
        }

        // Enable "Target" child
        Transform target = transform.Find("Target");
        if (target != null)
            target.gameObject.SetActive(true);

        // Enable selected random children
        foreach (int i in randomNumbers)
        {
            Transform item = transform.Find(i.ToString());
            if (item != null)
                item.gameObject.SetActive(true);
        }

        // Enable outer decoys
        ActivateChildrenFromPairs(10, 6, 9);
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

        // Add +1 to make upperBound inclusive
        return random.Next(lowerBound, upperBound + 1);
    }

    public static List<(int, int)> GenerateUniquePairs(int count, int upperBound1, int upperBound2)
    {
        int totalCombinations = upperBound1 * upperBound2;
        if (count > totalCombinations)
            throw new ArgumentException("Not enough unique pairs possible for the given bounds.");

        HashSet<(int, int)> uniquePairs = new HashSet<(int, int)>();

        while (uniquePairs.Count < count)
        {
            int first = random.Next(1, upperBound1 + 1);
            int second = random.Next(1, upperBound2 + 1);
            uniquePairs.Add((first, second));
        }

        return new List<(int, int)>(uniquePairs);
    }

    public void ActivateChildrenFromPairs(int pairCount, int outerUpperBound, int childUpperBound)
    {
        // Step 1: Disable all "outerX" parents (which also disables their children)
        foreach (Transform outer in transform)
        {
            if (outer.name.StartsWith("outer"))
            {
                outer.gameObject.SetActive(false);
            }
        }

        // Step 2: Generate the pairs to activate
        var pairs = GenerateUniquePairs(pairCount, outerUpperBound, childUpperBound);

        // Step 3: Enable specific parents and children
        foreach (var (outerIndex, childIndex) in pairs)
        {
            string parentName = "outer" + outerIndex;
            string childName = "outer_" + childIndex;

            Transform parent = transform.Find(parentName);
            if (parent != null)
            {
                // Activate the parent
                parent.gameObject.SetActive(true);

                // Disable all children under this parent
                foreach (Transform child in parent)
                {
                    child.gameObject.SetActive(false);
                }

                // Then activate only the selected child
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
