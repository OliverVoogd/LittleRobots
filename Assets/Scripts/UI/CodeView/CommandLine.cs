using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class InputLine {
    public string command;
    public List<string> arguments;
    public int nextExecutable;
    public bool executable;

    public InputLine(string com, List<string> args) {
        command = com;
        arguments = args;
        executable = false;
        nextExecutable = -1;
    }

    public static string GetString(InputLine l) {
        string s = l.command + ": ";
        s += String.Join(", ", l.arguments);
        s += "-> " + l.nextExecutable.ToString() + " ?" + l.executable.ToString() + "\n";
        return s;
    }
}

/// <summary>
/// Basically, a CommandLine object is going to exist as a UI element which the user will
/// be able to change the values of.
/// Each will have a command script attached??? Does this make sense? Yeah this probs makes sense
/// So each CommandLine will need to manage all the UI stuff and will also need to have a link
/// to a Command script, which it can pass off to the interpreter.
/// The Interpreter, instead of having commands populated by a InterpretedObject, it will instead have a connection to a 
/// UI CodeView (which will be able to be displayed and stuff) and can recieve all of the commands from there.
/// </summary>
public abstract class CommandLine : MonoBehaviour
{
    [SerializeField]
    protected string commandName;
    [SerializeField]
    protected TMP_InputField command_field;
    
    public virtual void Awake() {
        return; // do nothing rn
    }

    // public abstract InputLine getInputLine();

    public string getCommandName() {
        return commandName;
    }

    /// <summary>
    /// Get an instance of the associated Command class for this UI element
    /// </summary>
    /// <returns>An instantiated Command class which contains the function Run to run the code for the command</returns>
    public abstract Command GetCommand();
}
