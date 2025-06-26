using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelSelectorScript : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnButtonClickDistancePerception()
    {
        SceneManager.LoadScene("ObjectDistanceCognitionScene");
    }

    public void OnButtonClickDenseObjectSelection()
    {
        SceneManager.LoadScene("DenseObjectSelectionScene");
    }

    public void OnButtonClickSortingTask()
    {
        SceneManager.LoadScene("SortingSizesScene");
    }
}
