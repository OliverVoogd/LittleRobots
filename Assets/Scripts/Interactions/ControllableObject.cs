using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


[RequireComponent(typeof(Interpreter))]
public class ControllableObject : InterpretedObject, Interactable {
    protected Grid myGrid; // is there a better way to handle the grid system?

    protected bool active;
    public bool IsActive {
        get {
            return active;
        }
    }

    #region Movement Variables
    Vector3 targetPosition;
    bool movingToTarget = false;
    int speed = 1;
    #endregion

    protected override void Awake() {
        base.Awake();

        active = true;
        myGrid = FindObjectOfType<Grid>();
    }

    protected virtual void Update() {
        if (active) {
            UpdateMovement();
        }
    }

    protected void OnDrawGizmos() {
        /// purely for debug visualisation of where the object is moving to
        if (movingToTarget) {
            Gizmos.color = new Color(0, 1, 1, 0.3f);
            Gizmos.DrawCube(targetPosition != null ? targetPosition : transform.position, Vector3.one * myGrid.GridSize * .8f);
        }
    }

    public void OnClick() {
        /*interpreter.inputWindowVisible = !interpreter.inputWindowVisible;*/
    }

    protected void UpdateMovement() {
        /*if (movingToTarget) {
            if (Vector3.Distance(transform.position, targetPosition) <= .1f) {
                movingToTarget = false;
                transform.position = myGrid.GridClamp(transform.position);
                FinishedExecutingCommand = true;
            } else {
                Vector3 new_position = Vector3.MoveTowards(transform.position, targetPosition, (myGrid.GridSize * speed * Time.deltaTime / handler.ExecuteTime));

                int oldLayer = gameObject.layer;
                gameObject.layer = LayerMask.NameToLayer("NotCollidable");
                int interactableMask = LayerMask.NameToLayer("Interactable");
                int collidableMask = LayerMask.NameToLayer("Collidable");
                int layerMask = ((1 << interactableMask) | (1 << collidableMask));
                if (Physics.CheckSphere(new_position, myGrid.GridSize / 2 - .5f, layerMask)) {
                    Debug.Log("Exploded!");
                    FinishedExecutingCommand = true;
                    movingToTarget = false;

                    DestroyInteractable();
                } else {
                    transform.position = new_position;
                }
                gameObject.layer = oldLayer;

            }
        }*/
    }

    /// <summary>
    /// Destroy this gameObject, stop the simulation and make a boom?
    /// </summary>
    public void DestroyInteractable() {
        DestroyInterpretedObject();
        Destroy(this.gameObject);
    }

    /// <summary>
    /// Basic Move Command. Not Yet properly Implemented.
    /// Discuss with amos how to appropriately move the robot
    /// </summary>
    /// <param name="args"></param>
    protected void MoveCommand(List<string> args) {
        /*if (args.Count != 2) Debug.LogError(new ArgumentException("Invalid Number of Arguments to Move Command"));
        if (!Int32.TryParse(args[1], out int amount)) {
            throw new NotImplementedException(); // arguments are not an int
        }
        int move_x = 0;
        int move_y = 0;
        float rotation_y = 0.0f;
        switch (args[0]) {
            case "left":
                move_x = -1;
                rotation_y = -90.0f;
                break;
            case "right":
                move_x = 1;
                rotation_y = 90.0f;
                break;
            case "up":
                move_y = 1;
                rotation_y = 0.0f;
                break;
            case "down":
                move_y = -1;
                rotation_y = 180.0f;
                break;
            default:
                throw new NotImplementedException(); // not a valid move command
        }
        RotateOnMovement(rotation_y);
        move_x *= amount;
        move_y *= amount;

        movingToTarget = true;
        Vector3 movementAmount = new Vector3(move_x * myGrid.GridSize, 0.0f, move_y * myGrid.GridSize);
        targetPosition = myGrid.GridClamp(transform.position + movementAmount);
        // set flag for movement taking more than 1 tick
        if (amount > speed) {
            FinishedExecutingCommand = false;
        } else {
            FinishedExecutingCommand = true;
        }*/
    }

    protected void RotateOnMovement(float rotation_y) {
        // this is temporary rotation code
        transform.rotation = Quaternion.Euler(0.0f, rotation_y, 0.0f);
    }
}
