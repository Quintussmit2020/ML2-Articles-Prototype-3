using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.MagicLeap;

public class GazeRayInput : ActionBasedController
{
    private MagicLeapInputs mlInputs;

    private MagicLeapInputs.EyesActions eyesActions;
    private MLPermissions.Callbacks permissionCallbacks;
    private bool permissionGranted;

    private Vector3 eyeOrigin;
    private Vector3 eyeDirection;

    // Start is called before the first frame update
    protected override void Awake()
    {
        base.Awake();

        permissionCallbacks = new MLPermissions.Callbacks();
        permissionCallbacks.OnPermissionGranted += PermissionCallbacksOnOnPermissionGranted;

        mlInputs = new MagicLeapInputs();
        mlInputs.Enable();
        eyesActions = new MagicLeapInputs.EyesActions(mlInputs);

        MLPermissions.RequestPermission(MLPermission.EyeTracking, permissionCallbacks);
    }

    private void PermissionCallbacksOnOnPermissionGranted(string permission)
    {
        InputSubsystem.Extensions.MLEyes.StartTracking();
        permissionGranted = true;
    }

    protected override void UpdateTrackingInput(XRControllerState controllerState)
    {
        base.UpdateTrackingInput(controllerState);
      
        if (permissionGranted)
        {
            UnityEngine.InputSystem.XR.Eyes eyes = eyesActions.Data.ReadValue<UnityEngine.InputSystem.XR.Eyes>();
            eyeOrigin = Camera.main.transform.position;
            eyeDirection = (eyes.fixationPoint - eyeOrigin).normalized;
            controllerState.position = eyeOrigin;
            controllerState.rotation = Quaternion.LookRotation(eyeDirection);
            controllerState.inputTrackingState = InputTrackingState.Position | InputTrackingState.Rotation;
        }
        else
        {
            controllerState.inputTrackingState = InputTrackingState.None;
        }
    }

}
