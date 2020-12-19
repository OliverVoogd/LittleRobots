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
    public GameObject thisRobot;

    // Input Objects
    [SerializeField]
    private float inputRectWidth;
    [SerializeField]
    private float inputRectHeight;
    private Rect inputRect;
    private string inputCode = "Initial Input";

    internal ControllableObject robotController = null;

    // UI objects
    Camera cam;

    // private variables for code execution
    public bool CanExecute { get; private set; }
    int programCounter = 0; // represents the current line we are executing
    int cur_wait_amount = 0;
    bool cur_waiting = false;
    List<List<string>> commands; // assume for now we have a way of giving an array of strings to this variable
    Dictionary<string, int> markedLines; // represents the 'marked' program lines we can make when executing the program, to facilitate loops and goto statements

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
        inputRect = GUI.Window(this.GetInstanceID(), inputRect, MoveWindow, "Code Input");
        //inputRectTransform.rect.position = inputRect.position;
    }
    protected void MoveWindow(int windowID) {
        float tWidth = inputRectWidth * .01f;
        float tHeight = inputRectHeight * .075f;
        inputCode = GUI.TextArea(new Rect(tWidth, tHeight, inputRectWidth - (2f * tWidth), inputRectHeight - (tHeight * 1.1f)), inputCode);

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
                // when we recieve the goto command, we want to check it's argument against the dictionary of marked locations, and change the programCounter to that value
                if (currentLine.Count != 2) throw new NotImplementedException();
                if (!markedLines.ContainsKey(currentLine[1])) throw new NotImplementedException();

                programCounter = markedLines[currentLine[1]] + 1;
            } else if (currentLine[0] == "mark") {
                if (markedLines == null) markedLines = new Dictionary<string, int>();
                if (currentLine.Count != 2) throw new NotImplementedException();
                if (!markedLines.ContainsKey(currentLine[1])) markedLines.Add(currentLine[1], programCounter); // add the current line as a location accessable through loops and goto

                programCounter++;
            } else if (currentLine[0] == "wait") {
                // how are we going to implement wait?
                // does wait do nothing except wait on the current line for x actions?
                // or does it repeat the above line x times?
                // or should we have a seperate 'repi' for doing that
                // for now let's just wait on the current line
                if (!Int32.TryParse(currentLine[1], out int waitAmount)) throw new NotImplementedException();
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
            } else {
                // for all other commands which are assumed to be object specific commands
                robotController.ExecuteCommand(currentLine);
                programCounter++;
            }
        }
    }
}
