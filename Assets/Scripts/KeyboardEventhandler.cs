using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static UnityEngine.XR.MagicLeap.MLVirtualKeyboard;

public class KeyboardEventhandler : MonoBehaviour
{

    // The TextMeshPro text field that we want to update
    private TextMeshProUGUI textField;

    // Start is called before the first frame update
    void Start()
    {
        KeyboardSubmitEvent submitEvent = new KeyboardSubmitEvent();
        submitEvent.AddListener(UpdateTextField);
        textField = GetComponent<TextMeshProUGUI>();    
    }

    // Update is called once per frame
    private void UpdateTextField(string newText)
    {
        // Add the received string to the TextMeshPro text field
        textField.text += newText;
    }

}
