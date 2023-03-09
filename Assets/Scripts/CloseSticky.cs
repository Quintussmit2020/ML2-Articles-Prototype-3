using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CloseSticky : MonoBehaviour
{
    public Button destroyButton; // Assign a button to this in the Unity Inspector

    public delegate void InstanceIDEventHandler(int instanceID);
    public static event InstanceIDEventHandler InstanceIDEvent;

    void Start()
    {
        // Attach a method to the button's onClick event to call the DestroyObject method
        destroyButton.onClick.AddListener(DestroyObject);
    }

    void DestroyObject()
    {
        int instanceID = this.gameObject.GetInstanceID();
        Destroy(gameObject);
        if (InstanceIDEvent != null)
        {
            InstanceIDEvent(instanceID); // Raise the event with the instance ID
        }
    }
}
