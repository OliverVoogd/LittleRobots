using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Abstract class specification for an object which uses an Interpreter
/// class to take user code input and perform actions.
/// Subclasses need to provide 'XCommand' methods which are added
/// to the commandDict.
/// </summary>
[RequireComponent(typeof(Interpreter))]
public abstract class InterpretedObject : MonoBehaviour
{
    protected ProgramHandler handler;
    protected Interpreter interpreter;
    protected int lastProgramCounter = -1;

    public Dictionary<string, Action<List<string>>> commandDict;

    /// <summary>
    /// property for if this object has finished executing the
    /// last command it was given. To be used for multi-frame commands
    /// such as movement
    /// </summary>
    public bool FinishedExecutingCommand {
        get;
        protected set;
    }
    
    protected virtual void Awake() {
        handler = FindObjectOfType<ProgramHandler>();
        // add functions
        commandDict = AddCommands();

        AddInterpreter();

        FinishedExecutingCommand = true;
    }

    /// <summary>
    /// Add an interpreter (auto created) to this game object,
    /// filling out any required fields
    /// </summary>
    protected void AddInterpreter() {
        interpreter = gameObject.GetComponent<Interpreter>() as Interpreter;
        interpreter.thisRobot = gameObject;
        interpreter.robotController = this;

        handler.StoreInterpreter(interpreter);
    }

    /// <summary>
    /// If this object has finished executing the current command, execute the next one
    /// If the object is ready to execute another command, return true, otherwise return false
    /// </summary>
    /// <param name="callCommand">The command string indicating the command to run</param>
    /// <param name="arguments"> a list of arguments for the specific command</param>
    /// <returns></returns>
    public void ExecuteCommand(string callCommand, List<string> arguments) {
        // grab the first argument, the command itself
        if (commandDict.ContainsKey(callCommand)) {
            commandDict[callCommand](arguments);
            // the list passed contains the command name at [0] and the other arguments from [1] onwards
        } else {
            throw new NotImplementedException(); // no command exists to handle the input
        }

    }

    /// <summary>
    /// Destroy the interpreter using specific interpreter related methods
    /// </summary>
    public void DestroyInterpretedObject() {
        interpreter.DestroyInterpreter();
    }

    /// <summary>
    /// Populate the commandDict with the commands available for a user 
    /// to call when writing in-game code.
    /// </summary>
    /// <returns>a Dictionary of program commands keyed by in-game code instruction</returns>
    protected abstract Dictionary<string, Action<List<string>>> AddCommands();
}
