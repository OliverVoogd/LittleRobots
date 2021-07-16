using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Interface for objects which can be interacted with by the user
/// through clicking, dragging, etc.
/// </summary>
public interface Interactable
{
    /// <summary>
    /// Specifies if this object is active and able to be clicked, dragged, touched.
    /// </summary>
    bool IsActive {
        get;
    }

    void OnClick();

    void DestroyInteractable();
}
