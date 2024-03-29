using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using MagicLeap.Core;
using TMPro;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.MagicLeap;



public class PlaceSticky : MonoBehaviour
{
    
    private MagicLeapInputs magicLeapInputs;
    private MagicLeapInputs.ControllerActions controllerActions;
    private readonly MLPermissions.Callbacks permissionCallbacks = new MLPermissions.Callbacks();

    [Header("Eye tracking")]
    private MagicLeapInputs.EyesActions eyesActions;
    [Tooltip("Drag the eye tracker object from the XR origin in here.")]
    public GameObject eyeTracker;
    [Tooltip("Drag the eye tracker status menu parent object in here.")]
    public GameObject eyeTrackerMenu;
    [Header("Eye tracking status menu")]
    //Eye tracking menu properties
    [Tooltip("Add the eye tracking status text in here. This will be updated at runtime based on the eye tracking status")]
    public TextMeshProUGUI eyeTrackingText;

    [Header("Meshing")]
    [Tooltip("Add the mesh controller object.")]
    public GameObject MeshController;

    [Header("Sticky notes properties")]
    [Tooltip("Sticky note prefab.")]
    public GameObject stickyNote;
    [Tooltip("Sticky note indicator prefab.")]
    public GameObject stickyPlacementIndicator;
    

    //Creat a list to hold the random sticky text
    List<string> myReminders = new List<string>();

    [Header("Spatial checking menu")]
    //Spatial menu properties
    [Tooltip("Drag the Open spaces button in this slot.")]
    public Button openSpaceButton;
    [Tooltip("Drag the continue button in this slot.")]
    public Button continueButton;
    [Tooltip("Drag the Spatial check menu parent object in this slot.")]
    public GameObject spatialMenu;
    [Tooltip("The status text that will be updated goes in this slot.")]
    public TextMeshProUGUI menuSpatialText;
    [Tooltip("The status text that will be updated goes in this slot.")]
    public TextMeshProUGUI menuSpacesText;



    private bool isPlacing = false;

    private bool stickyRay = false;

 


    public UnityEvent<GameObject, Vector3> OnStickyObjectHit;

    //Delegate for the event that will inform a stickynote when a ray hits it. 
    //Not in use at the moment, this was to add higlighting to selected stickies. Will add later if time permits
    public delegate void StickyRayEventHandler(int stickyID);
    public static event StickyRayEventHandler OnStickyHitEvent;

    [Tooltip("How often, in seconds, to check if localization has changed.")]
    public float SearchInterval = 10;

    //Track the objects we already created to avoid duplicates
    private Dictionary<string, int> _persistentObjectsById = new Dictionary<string, int>();

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

    
    private void Awake()
    {
        permissionCallbacks.OnPermissionGranted += OnPermissionGranted;
        permissionCallbacks.OnPermissionDenied += OnPermissionDenied;
        permissionCallbacks.OnPermissionDeniedAndDontAskAgain += OnPermissionDenied;
        CloseSticky.InstanceIDEvent += CloseSticky_InstanceIDEvent;
    }

    private void OnDestroy()
    {
        permissionCallbacks.OnPermissionGranted -= OnPermissionGranted;
        permissionCallbacks.OnPermissionDenied -= OnPermissionDenied;
        permissionCallbacks.OnPermissionDeniedAndDontAskAgain -= OnPermissionDenied;
        CloseSticky.InstanceIDEvent -= CloseSticky_InstanceIDEvent;
    }

    private void Start()
    {
        //add some items to our sticky note content list
        myReminders.Add("buy milk");
        myReminders.Add("Call Bob from accounting");
        myReminders.Add("Password: #33$4156");
        myReminders.Add("Check email");
        myReminders.Add("Water plants");
        myReminders.Add("Pick up dry cleaning");
        myReminders.Add("Return library books");
        myReminders.Add("Schedule haircut");
        myReminders.Add("Pay electricity bill");
        myReminders.Add("Make grocery list");
        myReminders.Add("Book flight tickets");
        myReminders.Add("Attend team meeting");
        myReminders.Add("Order office supplies");
        myReminders.Add("Renew car insurance");
        myReminders.Add("Pick up dry cleaning");
        myReminders.Add("Feed cat");
        myReminders.Add("Call doctor");
        myReminders.Add("Write blog post");


        magicLeapInputs = new MagicLeapInputs();
        magicLeapInputs.Enable();
        controllerActions = new MagicLeapInputs.ControllerActions(magicLeapInputs);

        //subscribe to the controller button events
        controllerActions.Trigger.canceled += Trigger_performed;
        controllerActions.Bumper.performed += Bumper_performed;
        controllerActions.Menu.performed += Menu_performed;

        //Subscribe to the spatial check menu buttons
        continueButton.onClick.AddListener(ContinueToMain);
        openSpaceButton.onClick.AddListener(OpenSpaces);

        InputSubsystem.Extensions.MLEyes.StartTracking();
        //eyesActions = new MagicLeapInputs.EyesActions(magicLeapInputs);

        //Load JSON Data from file at start
        SimpleAnchorBinding.Storage.LoadFromFile();

        //Start the spatial check menu
        spatialMenu.SetActive(true);

        // First check for spatialization
        SpatialCheck();

        _spatialAnchorRequest = new MLAnchors.Request();

    }

    private void SpatialCheck()
    {
        var result = MLPermissions.CheckPermission(MLPermission.SpatialAnchors);

        if (result.IsOk)
        {
            TextMeshProUGUI permissionText = menuSpatialText.GetComponent<TextMeshProUGUI>();
            permissionText.text = "Spatial permission: ok";
            MLResult mlResult = MLAnchors.GetLocalizationInfo(out MLAnchors.LocalizationInfo info);
            //#if !UNITY_EDITOR
            if (info.LocalizationStatus == MLAnchors.LocalizationStatus.NotLocalized)
            {
                //Set error text
                TextMeshProUGUI spacesText = menuSpacesText.GetComponent<TextMeshProUGUI>();
                spacesText.text = "Localization: None. Click the button below to open the spaces app, select or set a space then run the demo project again";

                //activate Button
                openSpaceButton.gameObject.SetActive(true);
                continueButton.gameObject.SetActive(false);

            }
            else
            {
                //Set error text
                TextMeshProUGUI spacesText = menuSpacesText.GetComponent<TextMeshProUGUI>();
                spacesText.text = "Localization: ok";

                //activate Button
                openSpaceButton.gameObject.SetActive(false);
                continueButton.gameObject.SetActive(true);



                //debug print the error
                Debug.Log("GetLocalizationInfo Error " + mlResult);
            }
            //#endif

            //If we were able to get the localization info, debug it.
            if (mlResult.IsOk)
            {

                Debug.Log("GetLocalizationInfo " + mlResult);
            }
            else
            {
                Debug.Log("GetLocalizationInfo Error " + mlResult);
            }



        }
    }
    
    private void OpenSpaces()
    {
        UnityEngine.XR.MagicLeap.SettingsIntentsLauncher.LaunchSystemSettings("com.magicleap.intent.action.SELECT_SPACE");
        Application.Quit();
    }

    private void ContinueToMain()
    {
        //hide the spatial checks menu
        spatialMenu.SetActive(false);
        //switch on meshing
        MeshController.SetActive(true);
        eyeTrackerMenu.SetActive(true);
    }

    private void Menu_performed(InputAction.CallbackContext obj)
    {
        eyeTracker.SetActive(!eyeTracker.activeSelf);

        TextMeshProUGUI eyetrackerText = eyeTrackingText.GetComponent<TextMeshProUGUI>();
        
        if (eyeTracker.activeSelf)
        {
            eyetrackerText.text = "Eye tracking on";
        }
        else
        {
            eyetrackerText.text = "Eye tracking off";
        }

    }

    /*Gets the event fired by the close button on the sticky note script "CloseSticky" and receives the ID of the
   gameobject that was closed. 
     */
    private void CloseSticky_InstanceIDEvent(int instanceID)
    {
        string anchorToDestroy = _persistentObjectsById.FirstOrDefault(x => x.Value == instanceID).Key;
        RemoveAnchor(anchorToDestroy);

        Debug.Log("Instance ID received: " + instanceID +" and anchor is " + anchorToDestroy);
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

        //Physics.Raycast(raycastRay, out RaycastHit UIhitInfo, 100);
        //int buttonID = UIhitInfo.collider.gameObject.GetInstanceID();
        //Type hitType = UIhitInfo.collider.gameObject.GetType();

        if (stickyRay)
        {
            Physics.Raycast(raycastRay, out RaycastHit stickyHitInfo, 100);
            GameObject hitSticky = stickyHitInfo.collider.gameObject;
            Debug.Log("I just hit " + hitSticky.GetInstanceID());
            OnStickyHitEvent?.Invoke(stickyHitInfo.collider.gameObject.GetInstanceID());
            stickyRay = false;
        }


        var eyes = eyesActions.Data.ReadValue<UnityEngine.InputSystem.XR.Eyes>();



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


    public Vector3 GetHeadsetPosition()
    {
        Vector3 headPosition = Camera.main.transform.position + Camera.main.transform.forward * 1.0f;
        return headPosition;
    }



    private void ClearVisuals()
    {
        foreach (var prefab in _persistentObjectsById.Values)
        {
            GameObject objToDestroy = GameObject.FindObjectsOfType<GameObject>().FirstOrDefault(obj => obj.GetInstanceID() == prefab);

            if (objToDestroy != null)
            {
                Destroy(objToDestroy);
            }

            //Destroy(prefab);
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
            /*grab the list of anchors and the related gameobjects for each*/
            MLAnchors.Anchor anchor = result.anchors[i];
            var savedAnchor = SimpleAnchorBinding.Storage.Bindings.Find(x => x.Id == anchor.Id);
            if (savedAnchor != null && _persistentObjectsById.ContainsKey(anchor.Id) == false)
            {
               /* go through the list, check the type of prefab that was at the position, and instantiate it again
               AtlasPopulationMode the same position */
                if (savedAnchor.JsonData == stickyNote.name)
                {
                    var persistentObject = Instantiate(stickyNote, anchor.Pose.position, anchor.Pose.rotation);
                    //Grab the text from the saved file for the stickynote
                    TextMeshProUGUI stickyText = persistentObject.GetComponentInChildren<TextMeshProUGUI>();
                    stickyText.text = savedAnchor.StickyText;
                    /* create a list of instantiated objects for this session. This is so that we 
                     * can check later which one was closed and delete the associated anchor*/
                    _persistentObjectsById.Add(anchor.Id, persistentObject.GetInstanceID());
                }

            }
        }

        return true;
    }


    private void Bumper_performed(InputAction.CallbackContext obj)
    {
       //check if placing is currently false, if yes then start the indicator phase.
       //This is done so I can use the same button for both phases
        if (!isPlacing)
        {
            stickyPlacementIndicator.SetActive(true);          
            isPlacing = true;
        }
        //if no, start the placing phase
        else
        {          
            Pose stickyPose = new Pose(stickyPlacementIndicator.transform.position, stickyPlacementIndicator.transform.rotation);
            
            stickyPlacementIndicator.SetActive(false);
            isPlacing = false;        

            //Create an anchor for the placed note
            MLAnchors.Anchor.Create(stickyPose, 300, out MLAnchors.Anchor anchor);
            //Publish the anchor
            var result = anchor.Publish();
            if (result.IsOk)
            {   //instantiate a new stickynote in the same location as the placement indicator.     
                var persistentObject = Instantiate(stickyNote, stickyPlacementIndicator.transform.position, stickyPlacementIndicator.transform.rotation);
                TextMeshProUGUI stickyTextToSave = persistentObject.GetComponentInChildren<TextMeshProUGUI>();
                
                //Grab a random text from the sticky note text dictionary
                string randomText = myReminders[UnityEngine.Random.Range(0, myReminders.Count)];
                stickyTextToSave.text = randomText; //Just some placeholder text for now
                //this is just for debugging, can be removed.
                Debug.Log("this is the persistent object ID" + persistentObject.GetInstanceID());

                //Transform buttonTransform = persistentObject.transform.Find("Canvas/Button");                

                //if (buttonTransform != null)
                //{                    
                //    int instanceId = buttonTransform.gameObject.GetInstanceID();
                   
                //}
                //else
                //{
                //    Debug.Log("Button not found");
                //}

                SimpleAnchorBinding savedAnchor = new SimpleAnchorBinding();
                savedAnchor.Bind(anchor, stickyTextToSave.text, stickyNote.name);
                
                _persistentObjectsById.Add(anchor.Id, persistentObject.GetInstanceID());
                SimpleAnchorBinding.Storage.SaveToFile();
               
            }


        }

    }

    private void Trigger_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        //going to add keyboard stuff here
        stickyRay = true;
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


    //Returns true if the ID existed in the localized space and in the saved data
    private bool RemoveAnchor(string id)
    {
        //Delete the anchor using the Anchor's ID
        var savedAnchor = SimpleAnchorBinding.Storage.Bindings.Find(x => x.Id == id);
        //Delete the gameObject if it exists
        if (savedAnchor != null)
        {

            MLAnchors.Anchor.DeleteAnchorWithId(id);
            savedAnchor.UnBind();
            SimpleAnchorBinding.Storage.SaveToFile();
            return true;
        }

        return false;
    }

    private void OnPermissionGranted(string permission)
    {

    }




    private void OnPermissionDenied(string permission)
    {

    }


}


