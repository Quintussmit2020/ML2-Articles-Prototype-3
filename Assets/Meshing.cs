using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.XR.MagicLeap;

public class Meshing : MonoBehaviour
{

    private readonly MLPermissions.Callbacks mlPermissionsCallbacks = new MLPermissions.Callbacks();
    public MeshingSubsystemComponent meshingSubsystemComponent;
    //public MLSpatialMapper MlSpatialMapper;
    // Start is called before the first frame update

    private void Awake()
    {
        //MlSpatialMapper.enabled = false;
        mlPermissionsCallbacks.OnPermissionGranted += MlPermissionsCallbacks_OnPermissionGranted;
        mlPermissionsCallbacks.OnPermissionDenied += MlPermissionsCallbacks_OnPermissionDenied;
        mlPermissionsCallbacks.OnPermissionDeniedAndDontAskAgain += MlPermissionsCallbacks_OnPermissionDenied;
        MLPermissions.RequestPermission(MLPermission.SpatialMapping, mlPermissionsCallbacks);
    }

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    private void MlPermissionsCallbacks_OnPermissionDenied(string permission)
    {
        meshingSubsystemComponent.enabled = false;
    }

    private void MlPermissionsCallbacks_OnPermissionGranted(string permission)
    {
        meshingSubsystemComponent.enabled = true;
        //MlSpatialMapper.enabled = true;
    }
}
