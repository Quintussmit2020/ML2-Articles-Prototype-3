using System;
using System.Collections;
using System.Collections.Generic;
using MagicLeap.Core;
using TMPro;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
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

    public GameObject stickyNote;    
    public GameObject stickyPlacementIndicator;
    public string newStickyText = "Click to add text";

    private bool isPlacing = false;
 
    public UnityEvent<GameObject, Vector3> OnStickyObjectHit;


    [Tooltip("How often, in seconds, to check if localization has changed.")]
    public float SearchInterval = 10;

    //Track the objects we already created to avoid duplicates
    private Dictionary<string, GameObject> _persistentObjectsById = new Dictionary<string, GameObject>();

    private string _localizedSpace;

    //Spatial Anchor properties
    private MLAnchors.Request _spatialAnchorRequest;
    private MLAnchors.Request.Params _anchorRequestParams;
    private Pose publicStickyPose;
    private MLAnchors.Anchor anchor;

    //Used to force search localization even if the current time hasn't expired
    private bool _searchNow;
    //The timestamp when anchors were last searched for
    private float _lastTick;

    //The amount of searches that were performed.
    //Used to make sure anchors are fully localized before instantiating them.
    private int numberOfSearches;

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

     

        //Load Data
        SimpleAnchorBinding.Storage.LoadFromFile();

        var result = MLPermissions.CheckPermission(MLPermission.SpatialAnchors);
        if (result.IsOk)
        {
            MLResult mlResult = MLAnchors.GetLocalizationInfo(out MLAnchors.LocalizationInfo info);
#if !UNITY_EDITOR
            if (info.LocalizationStatus == MLAnchors.LocalizationStatus.NotLocalized)
            {
                UnityEngine.XR.MagicLeap.SettingsIntentsLauncher.LaunchSystemSettings("com.magicleap.intent.action.SELECT_SPACE");
            }
#endif
        }

        _spatialAnchorRequest = new MLAnchors.Request();
    }
    void LateUpdate()
    {
        // Only search when the update time lapsed 
        if (!_searchNow && Time.time - _lastTick < SearchInterval)
            return;

        _lastTick = Time.time;

        MLResult mlResult = MLAnchors.GetLocalizationInfo(out MLAnchors.LocalizationInfo info);
        if (!mlResult.IsOk)
        {
            Debug.Log("Could not get localization Info " + mlResult);
            return;
        }

        if (info.LocalizationStatus == MLAnchors.LocalizationStatus.NotLocalized)
        {
            //Clear the old visuals
            ClearVisuals();
            _localizedSpace = "";
            numberOfSearches = 0;
            Debug.Log("Not Localized " + info.LocalizationStatus);
            return;
        }

        //If we are in a new space or have not localized yet then try to localize
        if (info.SpaceId != _localizedSpace)
        {
            ClearVisuals();
            if (Localize())
            {
                _localizedSpace = info.SpaceId;
            }
        }
    }

    private void ClearVisuals()
    {
        foreach (var prefab in _persistentObjectsById.Values)
        {
            Destroy(prefab);
        }
        _persistentObjectsById.Clear();
    }

    public void SearchNow()
    {
        _searchNow = true;
    }


    private bool Localize()
    {
        MLResult startStatus = _spatialAnchorRequest.Start(new MLAnchors.Request.Params(Camera.main.transform.position, 100, 0, false));
        numberOfSearches++;

        if (!startStatus.IsOk)
        {
            Debug.LogError("Could not start" + startStatus);
            return false;
        }

        MLResult queryStatus = _spatialAnchorRequest.TryGetResult(out MLAnchors.Request.Result result);

        if (!queryStatus.IsOk)
        {
            Debug.LogError("Could not get result " + queryStatus);
            return false;
        }

        //Wait a search to make sure anchors are initialized
        if (numberOfSearches <= 1)
        {
            Debug.LogWarning("Initializing Anchors");
            //Search again
            _searchNow = true;
            return false;
        }

        for (int i = 0; i < result.anchors.Length; i++)
        {
            MLAnchors.Anchor anchor = result.anchors[i];
            var savedAnchor = SimpleAnchorBinding.Storage.Bindings.Find(x => x.Id == anchor.Id);
            if (savedAnchor != null && _persistentObjectsById.ContainsKey(anchor.Id) == false)
            {
                if (savedAnchor.JsonData == stickyNote.name)
                {
                    var persistentObject = Instantiate(stickyNote, anchor.Pose.position, anchor.Pose.rotation);
                    TextMeshProUGUI stickyText = persistentObject.GetComponentInChildren<TextMeshProUGUI>();
                    stickyText.text = savedAnchor.StickyText;
                    _persistentObjectsById.Add(anchor.Id, persistentObject);
                }

            }
        }

        return true;
    }


    private void Bumper_performed(InputAction.CallbackContext obj)
    {
        if (!isPlacing)
        {
            stickyPlacementIndicator.SetActive(true);          
            isPlacing = true;
        }
        else
        {          
            Pose stickyPose = new Pose(stickyPlacementIndicator.transform.position, stickyPlacementIndicator.transform.rotation);
            //GameObject NewSticky = Instantiate(stickyNote, stickyPlacementIndicator.transform.position, stickyPlacementIndicator.transform.rotation);

            stickyPlacementIndicator.SetActive(false);
            isPlacing = false;        

            MLAnchors.Anchor.Create(stickyPose, 300, out MLAnchors.Anchor anchor);
            var result = anchor.Publish();
            if (result.IsOk)
            {        
                var persistentObject = Instantiate(stickyNote, stickyPlacementIndicator.transform.position, stickyPlacementIndicator.transform.rotation);
                TextMeshProUGUI stickyTextToSave = persistentObject.GetComponentInChildren<TextMeshProUGUI>();
                stickyTextToSave.text = newStickyText; //Just some placeholder text for now
                Debug.Log("this is the persistent object ID" + persistentObject.GetInstanceID());

                Transform buttonTransform = persistentObject.transform.Find("Canvas/Button");                

                if (buttonTransform != null)
                {                    
                    int instanceId = buttonTransform.gameObject.GetInstanceID();
                   
                }
                else
                {
                    Debug.Log("Button not found");
                }

                SimpleAnchorBinding savedAnchor = new SimpleAnchorBinding();
                savedAnchor.Bind(anchor, stickyTextToSave.text, stickyNote.name);
                
                _persistentObjectsById.Add(anchor.Id, persistentObject);
                SimpleAnchorBinding.Storage.SaveToFile();
               
            }


        }

    }

    private void Trigger_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        //going to add keyboard stuff here
    }

    private void Update()
    {
        Ray raycastRay = new Ray(controllerActions.Position.ReadValue<Vector3>(), controllerActions.Rotation.ReadValue<Quaternion>() * Vector3.forward);
        Vector3 newStickyPosition = raycastRay.direction * 1f;
        if (isPlacing & Physics.Raycast(raycastRay, out RaycastHit hitInfo, 100, LayerMask.GetMask("Mesh")))
        {
            stickyPlacementIndicator.transform.position = hitInfo.point;
            stickyPlacementIndicator.transform.rotation = Quaternion.LookRotation(-hitInfo.normal);
        }

        Physics.Raycast(raycastRay, out RaycastHit UIhitInfo, 100);
        int buttonID = UIhitInfo.collider.gameObject.GetInstanceID();
        Type hitType = UIhitInfo.collider.gameObject.GetType();
       

        //Debug.Log(buttonID+" "+ hitType.FullName);



    }


    public void SetAnchor(Pose activeStickyPose)
    {

        Pose stickyPose = new Pose(activeStickyPose.position, activeStickyPose.rotation);
        // Create a new anchor at the location of the controller.
        MLAnchors.Anchor.Create(stickyPose, 300, out MLAnchors.Anchor anchor);
        // Publish the anchor to the map after it is created.
        anchor.Publish();
        Debug.Log("anchor created at" + anchor);
    }

    public void DestroySticky()
    {
        Debug.Log("Close button detected");
        
    }

    private void OnPermissionGranted(string permission)
    {

    }




    private void OnPermissionDenied(string permission)
    {

    }

}


