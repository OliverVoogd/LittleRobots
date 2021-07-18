using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Forklift : ControllableObject, Weighted
{
    #region Environment Interaction Variables
    [SerializeField]
    float raisedHeight = 5.0f;
    bool raised = false;

    bool grabbing = false;
    Weighted grabbedWeighted; // how do we rotate the grabbed object? how should forklift rotate??
    Vector3 grabbedOffset;

    [SerializeField]
    float baseWeight = 20f;

    public float GetWeight {
        get {
            if (grabbing) {
                return baseWeight + grabbedWeighted.GetWeight;
            } else {
                return baseWeight;
            }
        }
    }

    protected bool isGrabbed;
    public bool IsGrabbed {
        get {
            return isGrabbed;
        }
    }
    #endregion

    protected override void Awake() {
        base.Awake();
    }

    protected override void Update() {
        if (active) {
            // base controllable object update
            UpdateMovement();

            if (grabbing) {
                // at the moment rotation isn't taken into account
                // good idea, but not working at all
                grabbedWeighted.MoveObject(this, transform.position, grabbedOffset);
                // how do we rotate the offset to rotate 
                // perhaps if we can move a point from transform.position along object facing * magnitude of offset
            }
        }
    }

    /*protected override Dictionary<string, Action<List<string>>> AddCommands() {
        return new Dictionary<string, Action<List<string>>> {
            { "move", MoveCommand },
            { "raise", RaiseCommand },
            { "lower", LowerCommand },
            { "grab", GrabCommand },
            { "drop", DropCommand }
        };
    }*/

    void RaiseCommand(List<string> args) {
        Debug.Log("Raise");
        if (!raised) {
            grabbedOffset += Vector3.up * raisedHeight;
            raised = true;
        } else {
            throw new NotImplementedException();
        }
    }

    void LowerCommand(List<string> args) {
        Debug.Log("Lower");
        if (raised) {
            grabbedOffset += Vector3.down * raisedHeight;
            raised = false;
        } else {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Do we need to face a particulate direction to Grab? or Should we just grab any object that is adjacent?
    /// what about grab [direction], eg grab left
    /// </summary>
    /// <param name="args"></param>
    void GrabCommand(List<string> args) {
        if (args.Count != 1) throw new NotImplementedException();
        // Assign the direction we're grabbing in
        Vector3 grabDirection;
        switch (args[0]) {
            case "left":
                grabDirection = Vector3.left;
                break;
            case "right":
                grabDirection = Vector3.right;
                break;
            case "up":
                grabDirection = Vector3.forward;
                break;
            case "down":
                grabDirection = Vector3.back;
                break;
            default:
                throw new NotImplementedException();
        }

        int interactableMask = LayerMask.NameToLayer("Interactable");
        Collider[] grabColliders = Physics.OverlapSphere(transform.position + grabDirection * myGrid.GridSize, myGrid.GridSize / 2 - 0.5f, 1 << interactableMask);
        foreach (Collider collider in grabColliders) {
            if (collider.GetComponent<Weighted>() != null) {
                grabbing = true;
                grabbedWeighted = collider.gameObject.GetComponent<Weighted>();

                Vector3 grabbedPosition = grabbedWeighted.Grab(this);
                grabbedOffset = grabbedPosition - transform.position;

                Debug.Log($"Grabbed Weight: {grabbedWeighted.GetWeight}, CombinedWeight: {GetWeight}");

                break;
            }
        }
    }

    void DropCommand(List<string> args) {
        grabbedWeighted.Drop();
        Debug.Log("Drop");
    }

    public Vector3 Grab(Weighted other) {
        isGrabbed = true;
        active = false;

        return transform.position;
    }

    public void Drop() {
        isGrabbed = false;
        active = true;
    }

    public void MoveObject(Weighted other, Vector3 destination, Vector3 offset) {
        throw new NotImplementedException();
    }
}
