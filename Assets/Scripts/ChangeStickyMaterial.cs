using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeStickyMaterial : MonoBehaviour
{
    private GameObject targetObject;
    public Material highlightMaterial;
    public Material defaultMaterial;
    private Vector3 originalPosition;




    private void Start()
    {
        // Subscribe to the event that sends the name of the hit object
        PlaceSticky stickyObjectDetector = FindObjectOfType<PlaceSticky>();
        stickyObjectDetector.OnStickyObjectHit.AddListener(OnStickyObjectHit);

        targetObject = this.GetComponent<Collider>().gameObject;

        originalPosition = targetObject.transform.position;

    }

    private void OnStickyObjectHit(GameObject hitObjectName, Vector3 position)
    {
        // Check if the name of the hit object matches the target object name
        if (hitObjectName == targetObject)
        {
            // Change this object's material
            Renderer renderer = GetComponent<Renderer>();
            renderer.material = highlightMaterial;
        }
        else
        {
            Renderer renderer = GetComponent<Renderer>();
            renderer.material = defaultMaterial;
        }

    }

    private void Update()
    {
        
    }
}
