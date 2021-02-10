using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Interpreter))]
public class ControllableObject : Interactable
{
    public Grid grid;

    public Dictionary<string, Action<List<string>>> commandDict;
    ProgramHandler handler;

    public bool FinishedExecutingCommand { get; private set; }
    Interpreter interpreter;
    int lastProgramCounter = -1;

    Vector3 targetPosition;
    bool movingToTarget = false;
    int speed = 1;

    // Start is called before the first frame update
    void Awake()
    {
        handler = FindObjectOfType<ProgramHandler>();
        grid = FindObjectOfType<Grid>();
        // add functions
        commandDict = new Dictionary<string, Action<List<string>>> {
            {"move", MoveCommand }
        };
        // we need to add an interpreter that should be set up to this object, as well as an input field
        AddInterpreter();
        // store this object in the programHandlers' list of executing objects, allowing it to sychronise the execution of each command step
        handler.StoreInterpreter(interpreter);

        // setup initial values
        FinishedExecutingCommand = true;
        // clamp initial starting position?
        transform.position = grid.GridClamp(transform.position);
    }

    private void AddInterpreter() {
        interpreter = gameObject.GetComponent<Interpreter>() as Interpreter;
        interpreter.thisRobot = gameObject;
        interpreter.robotController = this;
    }

    public override void OnClick() {
        interpreter.inputWindowVisible = !interpreter.inputWindowVisible;
    }

    private void Update() {
        if (movingToTarget) {
            if (Vector3.Distance(transform.position, targetPosition) <= .001f) {
                movingToTarget = false;
                transform.position = grid.GridClamp(transform.position);
                FinishedExecutingCommand = true;
            } else transform.position = Vector3.MoveTowards(transform.position, targetPosition, (grid.gridSize * speed) * (Time.deltaTime / handler.ExecuteTime));
        }
        //transform.position = grid.Clamp(transform.position);
    }

    /// <summary>
    /// If this object has finished executing the current command, execute the next one
    /// If the object is ready to execute another command, return true, otherwise return false
    /// </summary>
    /// <param name="callCommand"></param>
    /// <param name="arguments"></param>
    /// <returns></returns>
    internal void ExecuteCommand(string callCommand, List<string> arguments) {
        // grab the first argument, the command itself
        if (commandDict.ContainsKey(callCommand)) {
            commandDict[callCommand](arguments);
            // the list passed contains the command name at [0] and the other arguments from [1] onwards
        } else {
            throw new NotImplementedException(); // no command exists to handle the input
        }

    }

    /// <summary>
    /// Basic Move Command. Not Yet Implemented.
    /// Discuss with amos how to appropriately move the robot
    /// </summary>
    /// <param name="args"></param>
    public void MoveCommand(List<string> args) {
        if (args.Count != 2) Debug.LogError(new ArgumentException("Invalid Number of Arguments to Move Command"));
        if (!Int32.TryParse(args[1], out int amount)) {
            throw new NotImplementedException(); // arguments are not an int
        }
        int move_x = 0;
        int move_y = 0;
        switch (args[0]) {
            case "left":
                move_x = -1;
                break;
            case "right":
                move_x = 1;
                break;
            case "up":
                move_y = 1;
                break;
            case "down":
                move_y = -1;
                break;
            default:
                throw new NotImplementedException(); // not a valid move command
        }
        move_x *= amount;
        move_y *= amount;

        movingToTarget = true;
        Vector3 movementAmount = new Vector3(move_x * grid.gridSize, 0.0f, move_y * grid.gridSize);
        targetPosition = grid.GridClamp(transform.position + movementAmount);

        // set flag for movement taking more than 1 tick
        if (amount > speed) {
            FinishedExecutingCommand = false;
        } else {
            FinishedExecutingCommand = true;
        }

        //Debug.Log("New targetPosition: (" + (movementAmount.x * grid.gridSize).ToString() + ", " + 
        //                                (movementAmount.z * grid.gridSize).ToString() + ")");
        // move this shit smoothly but every tick only 1 space * speed of object
        //Debug.Log("Successful Move Command with args: " + move_x.ToString() + ", " + move_y.ToString());
    }
}
