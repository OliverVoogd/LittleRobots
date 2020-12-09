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

    public GameObject thisRobot;
    public InputField inputField; // we need to make an input field later


    internal ControllableObject robotController = null;


    int programCounter = 0; // represents the current line we are executing
    List<List<string>> commands; // assume for now we have a way of giving an array of strings to this variable
   
    public bool CanExecute { get; private set; }

    private void Start() {
        // testing
        if (robotController is null) robotController = thisRobot.GetComponent<ControllableObject>();
        LoadInput();
    }
    public void LoadInput() {
        // we need to load input from the attached text field
        commands = new List<List<string>>();
        string[] loadedCommands = new string[] {"move 1 1", "goto 0"};
        foreach (string com in loadedCommands) {
            commands.Add(new List<string>(com.Split()));
            //Debug.Log(commands[commands.Count - 1][0]);
        }
        if (commands.Count > 0) CanExecute = true;
        // at the end of this function we need to have commands representing a list of commands each with 1 keyword and as many arguments as needed.
    }

    public void ExecuteNextLine() {
        if (CanExecute) {
            List<string> currentLine = commands[programCounter++]; // grab the current line and increment program counter.
            if (programCounter >= commands.Count) CanExecute = false;

            robotController.ExecuteCommand(currentLine);
        }
    }

}
