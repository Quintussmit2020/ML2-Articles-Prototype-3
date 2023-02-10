using System;
using System.Collections;
using System.Collections.Generic;
using MagicLeap.Core;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.MagicLeap;

public class PlaceSticky : MonoBehaviour
{
    private MagicLeapInputs magicLeapInputs;
    private MagicLeapInputs.ControllerActions controllerActions;

    private readonly MLPermissions.Callbacks permissionCallbacks = new MLPermissions.Callbacks();

    [Tooltip("Drag the sticky note prefab in here.")]
 

    public GameObject stickyNote;
    private GameObject stickyToPlace;
    private bool isPlacing = true;

    // Start is called before the first frame update
    private void Awake()
    {
        permissionCallbacks.OnPermissionGranted += OnPermissionGranted;
        permissionCallbacks.OnPermissionDenied += OnPermissionDenied;
        permissionCallbacks.OnPermissionDeniedAndDontAskAgain += OnPermissionDenied;
    }

    private void OnDestroy()
    {
        permissionCallbacks.OnPermissionGranted -= OnPermissionGranted;
        permissionCallbacks.OnPermissionDenied -= OnPermissionDenied;
        permissionCallbacks.OnPermissionDeniedAndDontAskAgain -= OnPermissionDenied;
    }

    private void Start()
    {
        magicLeapInputs = new MagicLeapInputs();
        magicLeapInputs.Enable();
        controllerActions = new MagicLeapInputs.ControllerActions(magicLeapInputs);
        controllerActions.Trigger.performed += Trigger_performed;
        controllerActions.Bumper.performed += Bumper_performed;
    }

    private void Bumper_performed(InputAction.CallbackContext obj)
    {
        isPlacing = true;
        stickyToPlace = Instantiate(stickyNote, controllerActions.Position.ReadValue<Vector3>(), controllerActions.Rotation.ReadValue<Quaternion>());
          
    }

    private void Trigger_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        isPlacing = false;
    }

    private void Update()
    {
        Ray raycastRay = new Ray(controllerActions.Position.ReadValue<Vector3>(), controllerActions.Rotation.ReadValue<Quaternion>() * Vector3.forward);
        if (isPlacing & Physics.Raycast(raycastRay, out RaycastHit hitInfo, 100, LayerMask.GetMask("Mesh")))
        {
            Debug.Log(hitInfo.transform);
            stickyToPlace.transform.position = hitInfo.point;
            stickyToPlace.transform.rotation = Quaternion.LookRotation(-hitInfo.normal);

        }
    }

    public void ExitMediaPlayer()
    {
        stickyNote.gameObject.SetActive(false);
        isPlacing = true;
    }

    private void OnPermissionGranted(string permission)
    {

    }




    private void OnPermissionDenied(string permission)
    {

    }

}


