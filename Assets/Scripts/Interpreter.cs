using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Interpreter : MonoBehaviour
{
    /* so we want this class to be able to be given text input, and to parse it for 
     * known tokens one by one. Let's start with using the tokens of 'move' and 'goto' for some basic branching.
     * we need to load the text input as an array of text lines, and we need a program counter to know which line we are
     * currently executing. 
     * Every object executing code will have one of these interpreter classes. The classes will grab the text input, and then will 
     * execute instructions each tick of the clock, which will be in a seperate global class. The clock will tell each interpreter to
     * execute the current program line, which is represented by the programCounter.
     */
    // GameObjects
    public GameObject thisRobot = null;

    // Input Objects
    public bool inputWindowVisible = true;
    [SerializeField]
    private float inputRectWidth = 200;
    [SerializeField]
    private float inputRectHeight = 250;
    private Rect inputRect = Rect.zero;
    private bool reDrawWindow = false;
    
    private string inputCode = "Initial Input";

    internal ControllableObject robotController = null;

    // UI objects
    Camera cam;

    // private variables for code execution
    public bool CanExecute { get; private set; }
    int programCounter = 0; // represents the current line we are executing
    int cur_wait_amount = 0;
    bool cur_waiting = false;
    
    // code execution
    List<List<string>> commands; // assume for now we have a way of giving an array of strings to this variable
    Dictionary<string, int> markedLines; // represents the 'marked' program lines we can make when executing the program, to facilitate loops and goto statements
    Dictionary<string, int> storedVariables; // contains variables stored by the user

    private void Start() {
        // Setup required object references;
        cam = GameObject.FindObjectOfType<Camera>();

        if (robotController is null) robotController = thisRobot.GetComponent<ControllableObject>();
        SetupInputField();

        markedLines = new Dictionary<string, int>();
    }

    private void SetupInputField() {
        Vector3 screenPoint = cam.WorldToScreenPoint(transform.position);
        inputRect = new Rect(screenPoint.x, Screen.height - screenPoint.y, inputRectWidth, inputRectHeight);
    }

    private void Update() {
        // temporary input loading
    }

    private void OnGUI() {
        // all of this OnGUI stuff should be changed later, as it is only intended for development.
        // might need to use the old InputField method again, inside a draggable object.
        if (inputWindowVisible) {
            inputRect = GUI.Window(this.GetInstanceID(), inputRect, MoveableCodeWindow, "Code Input");
        }
    }
    protected void MoveableCodeWindow(int windowID) {
        float tWidth = inputRectWidth * .01f;
        float tHeight = inputRectHeight * .075f;
        inputCode = GUI.TextArea(new Rect(tWidth, tHeight, inputRectWidth - (2f * tWidth), inputRectHeight - (tHeight * 1.1f)), inputCode);

        if (GUI.Button(new Rect(inputRectWidth - 22, 2, 20, 15), "X")) {
            Debug.Log("Cross");
            inputWindowVisible = !inputWindowVisible;
        }

        GUI.DragWindow();
    }


    public void LoadInput() {
        // we need to load input from the attached text field
        commands = new List<List<string>>();
        //string[] loadedCommands = inputField.text.Split('\n');
        string[] loadedCommands = inputCode.Split('\n');
        //string[] loadedCommands = new string[] {"move 1 1", "mark first", "move 2 2", "wait 3", "goto first"}; /// this is only testing method of adding commands
        foreach (string com in loadedCommands) {
            commands.Add(new List<string>(com.Split()));
            Debug.Log(commands[commands.Count - 1][0]);
        }
        if (commands.Count > 0) CanExecute = true;
        // at the end of this function we need to have commands representing a list of commands each with 1 keyword and as many arguments as needed.
    }

    public void ReWriteInput() {
        string[] joinedCommands = new string[commands.Count];
        for (int i = 0; i < commands.Count; i++) {
            joinedCommands[i] = String.Join(" ", commands[i]);
            if (i == programCounter) {
                joinedCommands[i] = "\t" + joinedCommands[i];
            }
        }
        inputCode = String.Join("\n", joinedCommands);
        //inputField.text = String.Join("\n", joinedCommands);
    }

    /// <summary>
    /// This method needs to handle basic code syntax available to all robots, such as 'wait', 'goto', etc
    /// </summary>
    public void ExecuteNextLine() {
        if (programCounter >= commands.Count) CanExecute = false;
        if (CanExecute) {
            List<string> currentLine = commands[programCounter];
            Debug.Log(String.Join(": ", currentLine));
            // check for basic instructions
            if (currentLine[0] == "goto") {
                Goto(currentLine);
                ExecuteNextLine();
            } else if (currentLine[0] == "mark") { // no time
                Mark(currentLine);
                ExecuteNextLine();
            } else if (currentLine[0] == "wait") {
                Wait(currentLine);
            } else if (currentLine[0] == "seti") {
                Seti(currentLine);
                ExecuteNextLine();
            } else if (currentLine[0] == "tjmp") {
                Tjmp(currentLine);
                ExecuteNextLine();
            } else {
                // for all other commands which are assumed to be object specific commands
                robotController.ExecuteCommand(currentLine);
                programCounter++;
            }
        }
    }

    protected void Goto(List<string> command) {
        // goto [mark]
        // jump to the given mark
        // when we recieve the goto command, we want to check it's argument against the dictionary of marked locations, and change the programCounter to that value
        if (command.Count != 2) throw new NotImplementedException();
        if (!markedLines.ContainsKey(command[1])) throw new NotImplementedException();

        programCounter = markedLines[command[1]] + 1;
    }
    protected void Mark(List<string> command) {
        // mark [mark]
        // store the current position as a position to return to
        if (markedLines == null) markedLines = new Dictionary<string, int>();
        if (command.Count != 2) throw new NotImplementedException();
        if (!markedLines.ContainsKey(command[1])) markedLines.Add(command[1], programCounter); // add the current line as a location accessable through loops and goto

        programCounter++;
    }
    protected void Wait(List<string> command) {
        // wait [sec]
        // how are we going to implement wait?
        // does wait do nothing except wait on the current line for x actions?
        // or does it repeat the above line x times?
        // or should we have a seperate 'repi' for doing that
        // for now let's just wait on the current line
        if (!Int32.TryParse(command[1], out int waitAmount)) throw new NotImplementedException();
        if (!cur_waiting) {
            cur_wait_amount = waitAmount;
            cur_waiting = true;
        }

        if (waitAmount <= 1) {
            commands[programCounter][1] = cur_wait_amount.ToString();
            cur_waiting = false;
            programCounter++;
        } else {
            waitAmount--;
            commands[programCounter][1] = waitAmount.ToString();
        }
    }
    protected void Seti(List<string> command) {
        // seti [loc] [val]
        // set the given variable to the given value
        if (!Int32.TryParse(command[2], out int value)) throw new NotImplementedException();
        if (storedVariables is null) storedVariables = new Dictionary<string, int>();
        if (!storedVariables.ContainsKey(command[1])) {
            storedVariables.Add(command[1], value);
        } else {
            storedVariables[command[1]] = value;
        }
        programCounter++;
    }
    protected void Tjmp(List<string> command) {
        // tjmp [loc] [mark]
        // if the value of [loc] is greater than 0, jump to [mark]
        if (storedVariables.TryGetValue(command[1], out int value)) {
            if (value > 0) {
                Goto(new List<string>() { "goto", command[2] });
            }
        }
    }
}
