using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// Superclass of CommandLine to provide a UI representation of the MoveCommand,
/// available to any ControllableObject.
/// 
/// Basic premise is a CodeView obejct holds a list of CommandLine classes,
/// when code is executed, the CodeView will iterate of its list and call GetCommand on each CommandLine
/// UI element, which will provide the corresponding Command class to the given robot.
/// The Interpreter will then call the Run function of this Command which will perform it's action
/// on said robot.
/// </summary>
public class MoveCommandLine : CommandLine
{
    [SerializeField]
    TMP_Dropdown direction_dropdown;
    [SerializeField]
    TMP_InputField magnitude_field;
    // Start is called before the first frame update
    public override void Awake()
    {
        base.Awake();
/*        // Great, now we can get all the stuff that we need for code execution
        List<TMP_Dropdown.OptionData> options = direction_dropdown.options;

        foreach (TMP_Dropdown.OptionData opt in options) {
            Debug.Log(opt.text);
        }*/

    }

    /// <summary>
    /// Creates a MoveCommand class to handle the moving of an InterpretedObject based on the MoveCommand.Run script
    /// </summary>
    /// <returns>a new MoveCommand</returns>
    public override Command GetCommand() {
        return new MoveCommand(direction_dropdown.options[direction_dropdown.value].text, Int32.Parse(magnitude_field.text));
    }
}
