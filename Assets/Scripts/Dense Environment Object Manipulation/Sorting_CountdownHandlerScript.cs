using System.Collections;
using TMPro;
using UnityEngine;

public class Sorting_CountdownHandlerScript : MonoBehaviour
{

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
        ExecuteFunction(targetObject);
    }

    void ExecuteFunction(GameObject targetObject)
    {
        if (targetObject != null)
        {
            SortingBatchCreatorScript targetScript = targetObject.GetComponent<SortingBatchCreatorScript>();
            if (targetScript != null)
            {
                targetScript.SetObjectVisibility(true);
                targetScript.SetAreaVisibility(false);
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
