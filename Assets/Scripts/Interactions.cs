    using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Global Class for managing user interactions with interactable gameObject.
/// </summary>
public class Interactions : MonoBehaviour {
    [SerializeField]
    LayerMask interactableMask;

    Camera cam;

    // Start is called before the first frame update
    void Start()
    {
        cam = GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0)) {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, interactableMask)) {
                Interactable hitObj = hit.transform.gameObject.GetComponent<Interactable>();
                hitObj.OnClick();
            }
        }
    }
}
