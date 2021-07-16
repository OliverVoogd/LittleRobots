using System;
using UnityEngine;

/// <summary>
/// A Command is a single function script which should be written to control a variety of 
/// robots in one specific capacity.
/// They are linked with CommandLines, which are the UI representation of the Command, 
/// and are passed to controlled objects which apply the functionality
/// </summary>
public abstract class Command {
    public String Name { get; }
 
    public Command(String thisName) {
        Name = thisName;
    }
    
    public abstract void Run(InterpretedObject robot);
}

