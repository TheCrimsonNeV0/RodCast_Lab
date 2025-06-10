using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class DenseBatchScript : MonoBehaviour
{
    void Start()
    {
        int[] randomNumbers = GetRandomNumbers(16, 6);

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
    }

    static int[] GetRandomNumbers(int upperBound, int numberCount)
    {
        if (upperBound < numberCount || upperBound < 1 || numberCount < 1)
            return new int[0];

        System.Random rand = new System.Random();
        List<int> numbers = Enumerable.Range(1, upperBound).ToList();
        return numbers.OrderBy(x => rand.Next()).Take(numberCount).ToArray();
    }
}
