using System.Collections;
using TMPro;
using UnityEngine;

public class CountdownHandlerScript : MonoBehaviour
{
    public enum Command
    {
        ACTIVATE_WALL = 1,
        DISPLAY_OBJECT = 2
    }

    public int countdownValueStatic = 10;
    private int countdownValue;

    public TextMeshProUGUI countdownText;

    public void OnButtonClickStart(GameObject targetObject)
    {
        ExecuteFunction(targetObject, Command.DISPLAY_OBJECT);
        countdownValue = countdownValueStatic; // Reset countdown
        StartCoroutine(StartCountdown(targetObject));
    }

    public IEnumerator StartCountdown(GameObject targetObject)
    {
        while (countdownValue > 0)
        {
            countdownText.text = countdownValue.ToString();
            yield return new WaitForSeconds(1f); // Wait for 1 second
            countdownValue--; // Reduce value by 1
        }

        countdownText.text = countdownValue.ToString();
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
                else if (command == Command.DISPLAY_OBJECT)
                {
                    targetScript.SetObjectVisibility(true);
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
