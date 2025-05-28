using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class DenseBatchScript : MonoBehaviour
{
    public GameObject denseBatchPrefab;
    private GameObject denseBatch;

    void Start()
    {
        int[] randomNumbers = GetRandomNumbers(10, 4);
        denseBatch = Instantiate(denseBatchPrefab);

        foreach (Transform child in denseBatch.transform) child.gameObject.SetActive(false); // Disable all children

        denseBatch.transform.Find("Target").gameObject.SetActive(true);
        for (int i = 0; i < randomNumbers.Length; i++)
        {
            denseBatch.transform.Find("" + randomNumbers[i]).gameObject.SetActive(true);
        }
    }

    static int[] GetRandomNumbers(int upperBound, int numberCount)
    {
        if (upperBound < numberCount || upperBound < 1 || numberCount < 1)
            return new int[0];

        System.Random rand = new System.Random();
        // Create a list of numbers from 1 to upperBound
        List<int> numbers = Enumerable.Range(1, upperBound).ToList();

        // Shuffle and take the first numberCount numbers
        int[] result = numbers.OrderBy(x => rand.Next()).Take(numberCount).ToArray();

        return result;
    }
}
