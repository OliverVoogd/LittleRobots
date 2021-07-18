using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
class CommandLineSize {
    public float Width;
    public float Height;
}

/// <summary>
/// UI class for holding all data related to the user inputted Command
/// sequence for a specific robot.
/// Each robot will have a CodeView object which will display the available and selected
/// CommandLine objects, which giving access to a Command class, which will
/// perform a single action through the Run method.
/// </summary>
public class CodeView : MonoBehaviour
{
    // UI parameters
    [SerializeField]
    RectTransform commandViewTransform;
    [SerializeField]
    CommandLineSize commandSize = new CommandLineSize() { Width = 0.0f, Height = 0.0f };


    // Provides a dictionary reference for command name to CommandLine object
    Dictionary<string, CommandLine> commandInputPairs;

    
    /// <summary>
    /// selectedCommandLines gives us a list of references to the UI CommandLine objects
    /// being displayed to the user.
    /// These need to be positioned (every frame/when created??)
    /// </summary>
    [SerializeField] // for debug
    List<CommandLine> selectedCommandLines;
    Command[] activeCommandArray; // for debug

    private void Awake() {
        setupSelectedCommandLines();
    }

    private void Start() {

        // THIS REQUIRES THE comandInputPairs dictionary to already be created


        // Debug command generation for startup
        //
        AddCommandLine("move");
        AddCommandLine("wait");
        AddCommandLine("move");
        AddCommandLine("wait");
        AddCommandLine("move");
        // AddCommandLine("wait");
        activeCommandArray = GetActiveCommands();
        Debug.Log("activeCommandArray:");
        foreach (Command c in activeCommandArray) {
            Debug.Log("\t" + c.Name);
        }
    }

    private void Update() {
        // Update the size of the commandViewTransform to hold all of the code commands
        //commandViewTransform.

        // How do we do this??
       /// commandViewTransform.rect = new selectedCommandLines.Count * commandSize.Height;

        // Reposition the list of selectedCommandLines
        float topY = (commandViewTransform.rect.height / 2);

        for (int i = 0; i < selectedCommandLines.Count; i++) {
            // This assumes that we want it to be centred in every direction besides verticel
            selectedCommandLines[i].transform.localPosition =
                new Vector3(0,topY - (i * commandSize.Height + commandSize.Height / 2), 0);
        }
    }

    private void setupSelectedCommandLines() {
        selectedCommandLines = new List<CommandLine>();
    }

    /// <summary>
    /// Accessible through an InterpreterObject to create the dictionary of commandNames and commandPrefabs.
    /// Each InterpretedObject prefab will need to be setup with a list of object specific commands which
    /// will then be passed through to the generate CodeView instance for usage.
    /// </summary>
    /// <param name="commandNames"></param>
    /// <param name="commandPrefabs"></param>
    public void SetCommandNamesAndPrefabs(string[] commandNames, CommandLine[] commandPrefabs) {
        commandInputPairs = new Dictionary<string, CommandLine>();

        // Populate the dictionary of possible commands
        for (int i = 0; i < commandNames.Length; i++) {
            commandInputPairs.Add(commandNames[i], commandPrefabs[i]);
        }
    }

    /// <summary>
    /// Check for validity of command name (ie, exists within possible command dictionary),
    /// and add it to the UI through instantiation (making it a child
    /// of the commandViewTransform object which allows for scrolling),
    /// and add it to the selectedCommandLines list.
    /// </summary>
    /// <param name="commandName">a string name of the command to create and add</param>
    /// <returns>true if adding the command was successful, false otherwise</returns>
    public bool AddCommandLine(string commandName) {
        if (commandInputPairs.TryGetValue(commandName, out CommandLine value)) {
            value = Instantiate<CommandLine>(value, commandViewTransform);
            selectedCommandLines.Add(value);
            return true;
        }

        return false;
    }
    
    /// <summary>
    /// Build a Command array of the activated Commands
    /// such that these can be parsed and executed by an Interpreter object
    /// </summary>
    /// <returns> a populated Command[] with all of the Command classes that each 
    /// CommandLine object represents</returns>
    public Command[] GetActiveCommands() {
        Command[] activeCommandArray = new Command[selectedCommandLines.Count];
        for (int i = 0; i < selectedCommandLines.Count; i++) {
            activeCommandArray[i] = selectedCommandLines[i].GetCommand();
        }

        return activeCommandArray;
    }
}
