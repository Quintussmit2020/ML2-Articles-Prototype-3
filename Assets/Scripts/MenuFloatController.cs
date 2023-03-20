using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuFloatController : MonoBehaviour
{
    [Tooltip("Drag the main camera from the XR Origin rig into this slot.")]
    public Transform cameraTransform;
    [Tooltip("How far the menu will float from the user")]
    public float distanceFromCamera = 1f;
    [Tooltip("How fast the menu adjusts to the headset rotations.")]
    public float moveSpeed = 5f;




    void Start()
    {
        this.gameObject.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        // Calculate the position of the fixation point in world space
        Vector3 fixationPoint = cameraTransform.position + cameraTransform.forward * distanceFromCamera;

        // Move the object to the fixation point smoothly
        transform.position = Vector3.Lerp(transform.position, fixationPoint, moveSpeed * Time.deltaTime);

        // Rotate the object to face the camera
        transform.LookAt(cameraTransform);
        transform.Rotate(new Vector3(0f, 180f, 0f));
    }
}
