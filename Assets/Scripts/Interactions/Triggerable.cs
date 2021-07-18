using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Triggerable : MonoBehaviour {
    protected bool triggerActive;
    
    /// <summary>
    /// Property indicating the current status of the triggerable entity.
    /// Changing the status will usually result in a change in the state of the
    /// connected Triggerable entity
    /// </summary>
    public abstract bool TriggerStatus { get; set; }
}
