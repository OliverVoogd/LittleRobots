using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControllableObject : MonoBehaviour
{
    public Dictionary<string, Action> // this needs fixing, need a way of using the input command to call a function. delegate? or if statement
    [SerializeField]
    ProgramHandler handler;

    Interpreter interpreter;

    // Start is called before the first frame update
    void Start()
    {
        // we need to add an interpreter that should be set up to this object, as well as an input field
        AddInterpreter();
        handler.StoreInterpreter(interpreter);
    }

    internal void ExecuteCommand(List<string> command) {
        Debug.Log(command[0]);

    }

    private void AddInterpreter() {
        interpreter = gameObject.AddComponent<Interpreter>() as Interpreter;
        interpreter.thisRobot = gameObject;
        interpreter.robotController = this;
    }
}
