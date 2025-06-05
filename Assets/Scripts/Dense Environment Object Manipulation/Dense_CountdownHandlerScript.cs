using System.Collections;
using TMPro;
using UnityEngine;

public class Dense_CountdownHandlerScript : MonoBehaviour
{
    public enum Command
    {
        ACTIVATE_WALL = 1,
        DISPLAY_OBJECT = 2
    }

    public GameObject timeLoggerObject;
    private TimeLoggerScript timeLogger;

    public int countdownValueStatic = 10;
    private int countdownValue;

    public TextMeshProUGUI countdownText;

    private float lastButtonClickTime;
    private float lastCountdownCompletedTime;

    void Start()
    {
        if (timeLoggerObject != null)
        {
            timeLogger = timeLoggerObject.GetComponent<TimeLoggerScript>();
        }
    }

    public void OnButtonClickStart(GameObject targetObject)
    {
        // ExecuteFunction(targetObject, Command.DISPLAY_OBJECT);
        lastButtonClickTime = Time.time;
        timeLogger.LogStartButtonClicked(lastButtonClickTime);

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

        lastCountdownCompletedTime = Time.time;
        timeLogger.LogCountdownCompleted(lastCountdownCompletedTime);

        countdownText.text = countdownValue.ToString();
        ExecuteFunction(targetObject, Command.ACTIVATE_WALL);
    }

    void ExecuteFunction(GameObject targetObject, Command command)
    {
        if (targetObject != null)
        {
            ObjectBatchCreatorScript targetScript = targetObject.GetComponent<ObjectBatchCreatorScript>();

            if (targetScript != null)
            {
                if (command == Command.ACTIVATE_WALL) // Activate view blocker
                {
                    targetScript.SetAreaVisibility(true);
                }
                else if (command == Command.DISPLAY_OBJECT)
                {
                    targetScript.SetAreaVisibility(true);
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

    public float GetLastButtonClickTime()
    {
        return lastButtonClickTime;
    }
}
