using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CloseSticky : MonoBehaviour
{
    public Button destroyButton; // Assign a button to this in the Unity Inspector

    void Start()
    {
        // Attach a method to the button's onClick event to call the DestroyObject method
        destroyButton.onClick.AddListener(DestroyObject);
    }

    void DestroyObject()
    {
        Destroy(gameObject); // Destroy the game object this script is attached to
        Debug.Log("Sticky should now be dead");
        Debug.Log("GameObject instance ID was " + this.gameObject.GetInstanceID());
    }
}
