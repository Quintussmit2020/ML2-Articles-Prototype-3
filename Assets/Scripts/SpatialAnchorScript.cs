using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.MagicLeap;

public class SpatialAnchorScript : MonoBehaviour
{
    [Tooltip("Drag the sticky note prefab in here.")]
    public GameObject stickyNote;

    [Tooltip("How often, in seconds, to check if localization has changed.")]
    public float SearchInterval = 10;

    private Dictionary<string, GameObject> _persistentObjectsById = new Dictionary<string, GameObject>();

    private string _localizedSpace;

    private MagicLeapInputs _magicLeapInputs;
    private MagicLeapInputs.ControllerActions _controllerActions;

    private MLAnchors.Request _spatialAnchorRequest;

    //Used to force search localization even if the current time hasn't expired
    private bool _searchNow;
    //The timestamp when anchors were last searched for
    private float _lastTick;

    //The amount of searches that were performed.
    //Used to make sure anchors are fully localized before instantiating them.
    private int numberOfSearches;


    /// Storage field used for locally persisting TransformBindings across device boot ups.
    /// </summary>


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }







}
