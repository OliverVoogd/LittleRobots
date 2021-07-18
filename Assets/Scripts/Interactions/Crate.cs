using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crate : MonoBehaviour, Interactable
{
    private void Awake() {
        transform.position = FindObjectOfType<Grid>().GridClamp(transform.position);
    }

    /// <summary>
    /// Indicator of if this object is active. Crates are always active
    /// </summary>
    public bool IsActive {
        get => true;
    }

    /// <summary>
    /// TODO: Implement a nice little UI for the crates, supplying basic information
    /// </summary>
    public void OnClick() {
        throw new System.NotImplementedException();
    }

    /// <summary>
    /// TODO: how to destroy crates?
    /// </summary>
    public void DestroyInteractable() {
        throw new System.NotImplementedException();
    }
}
