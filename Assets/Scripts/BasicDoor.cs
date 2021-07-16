using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicDoor : Triggerable
{
    bool lastTriggerState = false;

    /// <summary>
    /// Change the trigger status, and open (or close) the door
    /// </summary>
    public override bool TriggerStatus {
        get {
            return triggerActive;
        }
        set { 
            triggerActive = value;
            if (lastTriggerState != triggerActive) {
                ChangeDoorState();
            }
            lastTriggerState = triggerActive;
        }
    }

    protected virtual void ChangeDoorState() {
        Debug.Log("Door is changing state");
    }
}
