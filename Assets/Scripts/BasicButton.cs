using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicButton : PressureSensor
{
    public Triggerable connectedObject;

    protected override void Update() {
        base.Update();

        connectedObject.TriggerStatus = triggerActive;
    }
}
