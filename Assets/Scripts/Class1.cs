using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// old things
class OldFunctions : MonoBehaviour {
    // Input Objects
    public InputField inputField; // we need to make an input field later
    private GameObject inputObject;
    private RectTransform inputRectTransform;
    [SerializeField]
    private float inputRectWidth;
    [SerializeField]
    private float inputRectHeight;
    private Rect inputRect;
    private string inputCode = "Initial Input";

    internal InputField inputFieldPrefab;
    public void OnInputValueChange() {
        // get height of text and change the height of the box.
        // NOT WORKING????
        //inputRectTransform.rect.Set(inputRectTransform.rect.x, inputRectTransform.rect.y, inputRectTransform.rect.width, 100);
        //Debug.Log("Value Changed");
        //ResizeInputField();
    }
    private void SetupInputField() {
        // Create the input Object
        //inputObject = Instantiate(inputFieldPrefab.gameObject);
        //inputField = inputObject.GetComponent<InputField>();

        //inputField.lineType = InputField.LineType.MultiLineNewline;
        // This will need to be changed later
        //inputRectTransform = inputField.GetComponent<RectTransform>();

        //inputField.text = "This is Working";
        //inputField.onValueChanged.AddListener(delegate { OnInputValueChange(); }) ;
        //inputObject.transform.SetParent(canvas.transform);
        //inputObject.transform.position = cam.WorldToScreenPoint(transform.position);
    }
}