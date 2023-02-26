using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SummonSticky : MonoBehaviour
{
    private GameObject targetObject;
    private Vector3 originalPosition;





    private void Start()
    {
        PlaceSticky stickyObjectDetector = FindObjectOfType<PlaceSticky>();
        stickyObjectDetector.OnStickyObjectHit.AddListener(MoveStickyToNewPosition);


        targetObject = this.GetComponent<Collider>().gameObject;

        originalPosition = targetObject.transform.position;

    }

    private void MoveStickyToNewPosition(GameObject hitObjectName, Vector3 newPosition)
    {
        // Check if the name of the hit object matches the target object name
        if (hitObjectName == targetObject)
        {
            targetObject.transform.position = newPosition;
        }
        else
        {
            targetObject.transform.position = originalPosition;

        }

    }

    private void MoveStickyToOringalPosition()
    {
        this.transform.position = originalPosition;
    }

    private void Update()
    {

    }
}
