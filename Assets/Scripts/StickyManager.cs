using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StickyManager : MonoBehaviour
{
    private GameObject targetObject;
    public Material highlightMaterial;
    public Material defaultMaterial;
    private Vector3 originalPosition;


    void Start()
    {
        // Subscribe to the event that sends the name of the hit object
        PlaceSticky stickyObjectDetector = FindObjectOfType<PlaceSticky>();
        stickyObjectDetector.OnStickyObjectHit.AddListener(OnStickyObjectHit);

        targetObject = this.GetComponent<Collider>().gameObject;
        Debug.Log("Target object is" + targetObject);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnStickyObjectHit(GameObject hitObjectName, Vector3 position)
    {
        Renderer renderer;

        // Check if the name of the hit object matches the target object name
        if (hitObjectName == targetObject)
        {
            Debug.Log("Hit object detected");
            Transform cubeTransform = this.transform.Find("Cube");
            Debug.Log("CubeTransform is " + cubeTransform);
            if (cubeTransform != null)
            {
                Debug.Log("Cube detected");
                // Get the GameObject component of the child object
                GameObject cubeObject = cubeTransform.gameObject;

                renderer = cubeObject.GetComponent<Renderer>();
                renderer.material = highlightMaterial;
            }
        }
        else
        {
            Transform cubeTransform = transform.Find("Cube");
            if (cubeTransform != null)
            {
                // Get the GameObject component of the child object
                GameObject cubeObject = cubeTransform.gameObject;

                renderer = cubeObject.GetComponent<Renderer>();
                renderer.material = defaultMaterial;
            }
        }
    }

}
