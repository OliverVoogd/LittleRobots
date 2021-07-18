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
    [SerializeField]
    string[] commandNames;
    [SerializeField]
    CommandLine[] commandPrefabs;
    [SerializeField]
    CommandLine[] defaultPrefabs;

    CommandLine[] fullCommandPrefabs;
    
    protected ProgramHandler handler;
    protected Interpreter interpreter;
    protected CodeView codeView;
    protected int lastProgramCounter = -1;

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

        AddInterpreterandCodeView();

        FinishedExecutingCommand = true;
    }

    private void OnValidate() {
        FillCommandNameArray();
    }

    /// <summary>
    /// Populate the commandName array by getting all of the names of the commands and idk filling it
    /// </summary>
    private void FillCommandNameArray() {
        fullCommandPrefabs = new CommandLine[commandPrefabs.Length + defaultPrefabs.Length];
        commandNames = new string[commandPrefabs.Length + defaultPrefabs.Length];
        for (int i = 0; i < commandPrefabs.Length; i++) {
            commandNames[i] = commandPrefabs[i].getCommandName();
            fullCommandPrefabs[i] = commandPrefabs[i];
        }

        for (int i = commandPrefabs.Length; i < commandPrefabs.Length + defaultPrefabs.Length; i++) {
            commandNames[i] = defaultPrefabs[i - commandPrefabs.Length].getCommandName();
            fullCommandPrefabs[i] = defaultPrefabs[i - commandPrefabs.Length];
        }
    }

    /// <summary>
    /// Add an interpreter (auto created) to this game object,
    /// filling out any required fields
    /// </summary>
    protected void AddInterpreterandCodeView() {
        interpreter = gameObject.GetComponent<Interpreter>() as Interpreter;
        interpreter.thisRobot = gameObject;
        interpreter.robotController = this;

        handler.StoreInterpreter(interpreter);

        codeView = handler.CreateCodeView();
        FillCommandNameArray(); // ensure that commandNames is correct
        // add to the commandNames and commandPrefabs the default commands??
        codeView.SetCommandNamesAndPrefabs(commandNames, fullCommandPrefabs);
    }

    /// <summary>
    /// If this object has finished executing the current command, execute the next one
    /// If the object is ready to execute another command, return true, otherwise return false
    /// </summary>
    /// <param name="callCommand">The command string indicating the command to run</param>
    /// <param name="arguments"> a list of arguments for the specific command</param>
    /// <returns></returns>
    public void ExecuteCommand(string callCommand, List<string> arguments) {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Destroy the interpreter using specific interpreter related methods
    /// </summary>
    public void DestroyInterpretedObject() {
        interpreter.DestroyInterpreter();
    }

    public Command[] GetActiveCommands() {
        return codeView.GetActiveCommands();
    }
}
