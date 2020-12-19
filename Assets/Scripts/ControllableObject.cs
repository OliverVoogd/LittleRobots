using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Interpreter))]
public class ControllableObject : MonoBehaviour
{
    public Dictionary<string, Action<List<string>>> commandDict;
    ProgramHandler handler;

    Interpreter interpreter;

    // Start is called before the first frame update
    void Awake()
    {
        handler = FindObjectOfType<ProgramHandler>();
        // add functions
        commandDict = new Dictionary<string, Action<List<string>>> {
            {"move", moveCommand }
        };
        // we need to add an interpreter that should be set up to this object, as well as an input field
        AddInterpreter();
        // store this object in the programHandlers' list of executing objects, allowing it to sychronise the execution of each command step
        handler.StoreInterpreter(interpreter);
    }

    internal void ExecuteCommand(List<string> command) {
        string callCommand = command[0];
        List<string> commands = new List<string>();
        for (int i = 1; i < command.Count; i++) {
            commands.Add(command[i]);
        }
        if (commandDict.ContainsKey(callCommand)) {
            commandDict[callCommand](commands);
        } else {
            throw new NotImplementedException(); // no command exists to handle the input
        }
    }

    private void AddInterpreter() {
        interpreter = gameObject.GetComponent<Interpreter>() as Interpreter;
        interpreter.thisRobot = gameObject;
        interpreter.robotController = this;
    }

    /// <summary>
    /// Basic Move Command. Not Yet Implemented.
    /// Discuss with amos how to appropriately move the robot
    /// </summary>
    /// <param name="args"></param>
    public void moveCommand(List<string> args) {
        
        if (args.Count != 2) Debug.LogError(new ArgumentException("Invalid Number of Arguments to Move Command"));
        if (!Int32.TryParse(args[0], out int move_x) || !Int32.TryParse(args[1], out int move_y)) {
            throw new NotImplementedException(); // arguments are not an int
        }
        Debug.Log("Successful Move Command with args: " + move_x.ToString() + ", " + move_y.ToString());
    }
}
