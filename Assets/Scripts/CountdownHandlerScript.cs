using System.Collections;
using TMPro;
using UnityEngine;

public class CountdownHandlerScript : MonoBehaviour
{
    public enum Command
    {
        ACTIVATE_WALL = 1
    }

    public int countdownValue = 3;
    public TextMeshProUGUI countdownText;

    public void OnButtonClickStart(GameObject targetObject)
    {
        Debug.Log("Button click action performed");
        countdownValue = 3; // Reset countdown
        StartCoroutine(StartCountdown(targetObject));
    }

    public IEnumerator StartCountdown(GameObject targetObject)
    {
        while (countdownValue > 0)
        {
            Debug.Log("Countdown Value: " + countdownValue);
            countdownText.text = countdownValue.ToString();
            yield return new WaitForSeconds(1f); // Wait for 1 second
            countdownValue--; // Reduce value by 1
        }

        countdownText.text = countdownValue.ToString();
        Debug.Log("Countdown reached 0, executing function on target object...");
        ExecuteFunction(targetObject, Command.ACTIVATE_WALL);
    }

    void ExecuteFunction(GameObject targetObject, Command command)
    {
        if (targetObject != null)
        {
            ObjectDistanceCreatorScript targetScript = targetObject.GetComponent<ObjectDistanceCreatorScript>();

            if (targetScript != null)
            {
                if (command == Command.ACTIVATE_WALL) // Activate view blocker
                {
                    targetScript.SetBlockerVisibility(true);
                }
            }
            else
            {
                Debug.LogError("Target object does not have the required script!");
            }
        }
        else
        {
            Debug.LogError("Target object is NULL!");
        }
    }
}
