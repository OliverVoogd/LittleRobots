using System;

public enum CommandFinished {
    Finished,
    NotFinished
}

/// <summary>
/// A Command is a single function script which should be written to control a variety of 
/// robots in one specific capacity.
/// They are linked with CommandLines, which are the UI representation of the Command, 
/// and are passed to controlled objects which apply the functionality
/// </summary>
public abstract class Command {
    public string Name { get; }
 
    public Command(String thisName) {
        Name = thisName.ToLower();
    }

    public string Type { protected set; get; }
}

/// <summary>
/// Command class for any command that is available to every robot through virtue of being Interpreted by and Interpreter
/// </summary>
public abstract class BuiltInCommand : Command {
    public BuiltInCommand(String thisName) : base(thisName) {
        Type = "BuiltInCommand";
    }

    /// <summary>
    /// Execute this command.
    /// Differs from SpecialCommand.Run as by being a built in command, it might require access to 
    /// select values from an interpreter, and not just a robot
    /// </summary>
    /// <param name="robot"></param>
    /// <param name="interpreter"></param>
    /// <returns>Returns true if this command is finished, false if it needs to be run again</returns>
    public abstract CommandFinished Run(InterpretedObject robot, Interpreter interpreter);
}

/// <summary>
/// Comand class for any command that is different from a built in Interpreter command
/// </summary>
public abstract class SpecialCommand : Command {
    public SpecialCommand(String thisName) : base(thisName) {
        Type = "SpecialCommand";
    }


    /// <summary>
    /// Execute this Command.
    /// </summary>
    /// <param name="robot"></param>
    /// <returns>Returns true if this command is finished, false if it needs to be run again (for commands longer than one tick)</returns>
    public abstract CommandFinished Run(InterpretedObject robot);
}
