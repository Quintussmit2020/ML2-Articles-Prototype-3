using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.MagicLeap;

public class SpatialAnchorScript : MonoBehaviour
{
   

    [Tooltip("How often, in seconds, to check if localization has changed.")]
    public float SearchInterval = 10;


    private MagicLeapInputs _magicLeapInputs;
    private MagicLeapInputs.ControllerActions _controllerActions;

    //Spatial Anchor properties
    private MLAnchors.Request _mlAnchorsRequest;
    private MLAnchors.Request.Params _anchorRequestParams;

    public Pose PublicStickyPose
    {
        get { return this.publicStickyPose; }
    }

    public MLAnchors.Anchor Anchor
    {
        get { return this.anchor; }
    }


    [SerializeField, HideInInspector]
    private Pose publicStickyPose;

    [SerializeField, HideInInspector]
    private MLAnchors.Anchor anchor;


    //Dictionary to keep track of all the objects
    private Dictionary<string, GameObject> _gameObjectsByAnchorId = new Dictionary<string, GameObject>();

    void Start()
    {
        // Initialize controller input events.
        _magicLeapInputs = new MagicLeapInputs();
        _magicLeapInputs.Enable();
        _controllerActions = new MagicLeapInputs.ControllerActions(_magicLeapInputs);

        //Initialize Magic leap Spatial Anchor Requests
        _mlAnchorsRequest = new MLAnchors.Request();

        //Get the user's current Localization Info and debugs it
        MLResult mlResult = MLAnchors.GetLocalizationInfo(out MLAnchors.LocalizationInfo info);
        Debug.Log("Localization Info " + info);


    }

 
    void Update()
    {
        
    }

    public void SetAnchor(Pose activeStickyPose)
    {
        
        Pose stickyPose = new Pose(activeStickyPose.position, activeStickyPose.rotation);
        // Create a new anchor at the location of the controller.
        MLAnchors.Anchor.Create(stickyPose, 300, out MLAnchors.Anchor anchor);
        // Publish the anchor to the map after it is created.
        anchor.Publish();
        Debug.Log("anchor create at" + anchor);
    }


    private void OnDestroy()
    {

    }



}
