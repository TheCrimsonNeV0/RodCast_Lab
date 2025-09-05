using System;
using UnityEngine;

public class Sorting_TargetObjectScript : MonoBehaviour
{
    // public GameObject objectBatchCreator;
    // private SortingBatchCreatorScript objectBatchCreatorScript;

    public event Action OnTargetStay;

    private bool isColliding = false;
    private float collisionTimer = 0f;
    private const float requiredTime = 1.0f;

    void OnCollisionStay(Collision collision)
    {
        if (
            (this.gameObject.CompareTag("SmallTarget") && collision.gameObject.CompareTag("SmallTargetSurface")) ||
            (this.gameObject.CompareTag("MediumTarget") && collision.gameObject.CompareTag("MediumTargetSurface")) ||
            (this.gameObject.CompareTag("LargeTarget") && collision.gameObject.CompareTag("LargeTargetSurface"))
        )
        {
            isColliding = true;
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if (
            (this.gameObject.CompareTag("SmallTarget") && collision.gameObject.CompareTag("SmallTargetSurface")) ||
            (this.gameObject.CompareTag("MediumTarget") && collision.gameObject.CompareTag("MediumTargetSurface")) ||
            (this.gameObject.CompareTag("LargeTarget") && collision.gameObject.CompareTag("LargeTargetSurface"))
        )
        {
            isColliding = false;
            collisionTimer = 0f;
        }
    }

    void Start()
    {
        /*
        GameObject creatorObject = GameObject.FindWithTag("SortingCreatorTag");
        if (creatorObject != null)
        {
            objectBatchCreatorScript = creatorObject.GetComponent<SortingBatchCreatorScript>();
        }
        else
        {
            Debug.LogWarning("ObjectBatchCreator not found in the scene.");
        }
        */
    }

    void Update()
    {
        if (isColliding)
        {
            collisionTimer += Time.deltaTime;
            if (collisionTimer >= requiredTime)
            {
                OnTargetStay?.Invoke();
                collisionTimer = 0f;
            }
        }
    }
}
