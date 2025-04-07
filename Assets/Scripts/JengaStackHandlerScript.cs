using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JengaStackHandlerScript : MonoBehaviour
{
    public GameObject jengaStackPrefab;
    public GameObject jengaStack;

    public void OnButtonClickJengaStack()
    {
        Destroy(jengaStack);
        jengaStack = Instantiate(jengaStackPrefab);
    }
}
