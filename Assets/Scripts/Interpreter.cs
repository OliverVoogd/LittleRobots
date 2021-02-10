using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
    private string inputCode = 
    "move left 1\nmove right 1\nseti count 0\nmark first\nmove right 1\naddi count 1\nseti test count\naddi test -1\ntjmp test out\ngoto first\nmark out";

    internal ControllableObject robotController = null;

    // UI objects
    Camera cam;

    // private variables for code execution
    public bool CanExecute { get; private set; }
    int programCounter = -1; // represents the current line we are executing
    int startProgramCounter = 0;
    int cur_wait_amount = 0;
    bool cur_waiting = false;
    
    //storing the starting position
    public bool PositionStored { get; private set; }
    [SerializeField]
    private Vector3 startPosition;
    
    // code execution
    //List<List<string>> commands; // the lines that can actually be executed
    public InputLine[] commands; // struct of all the lines.
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


    /// <summary>
    /// This method needs to handle basic code syntax available to all robots, such as 'wait', 'goto', etc
    /// </summary>
    public void ExecuteNextLine() {
        //if (robotController.FinishedExecutingCommand) programCounter = commands[programCounter].nextExecutable;

        if (!CanExecute) {
            return;
        }

        if (!commands[programCounter].executable) {
            // this should never enter
            programCounter = commands[programCounter].nextExecutable;
        }

        if (CanExecute && robotController.FinishedExecutingCommand) {
            ReWriteInput();


            string currentCommand = commands[programCounter].command;
            List<string> currentLine = commands[programCounter].arguments;
            Debug.Log(String.Join(": ", currentLine));
            // check for basic instructions
            switch (currentCommand) {
                case "goto":
                    programCounter = Goto(currentLine, programCounter); // this moves program counter itself
                    break;
                case "wait":
                    programCounter = Wait(currentLine, programCounter);
                    break;
                case "seti":
                    programCounter = Seti(currentLine, programCounter);
                    break;
                case "tjmp":
                    programCounter = Tjmp(currentLine, programCounter);
                    break;
                case "addi":
                    programCounter = Addi(currentLine, programCounter);
                    break;
                default:
                    robotController.ExecuteCommand(currentCommand, currentLine);
                    programCounter = commands[programCounter].nextExecutable;
                    break;
            }
        }
        if (programCounter >= commands.Length || programCounter < 0) {
            CanExecute = false;
            programCounter = -1;
            //ReWriteInput();
        }
    }
    public void ReWriteInput() {
        if (commands != null) {
            string[] joinedCommands = new string[commands.Length];
            for (int i = 0; i < commands.Length; i++) {
                joinedCommands[i] = String.Join(" ", commands[i].arguments);
                joinedCommands[i] = commands[i].command + " " + joinedCommands[i];
                if (i == programCounter) {
                    joinedCommands[i] = "\t" + joinedCommands[i];
                }
            }
            inputCode = String.Join("\n", joinedCommands);
            //inputField.text = String.Join("\n", joinedCommands);
        }
    }

    /// <summary>
    /// Reset the state of this interpreter, allowing the code to be read and executed again
    /// </summary>
    public void Reset() {
        programCounter = 0;
        startProgramCounter = 0;
        // at this stage, reset moves the objects back to their start position, without animation
        if (PositionStored)
            thisRobot.transform.position = startPosition;
    }
    /// <summary>
    /// Setup this interpreter for user code execution, by loading input, parsing it for 'mark' statements,
    /// and then setting the programCounter as the first executable line
    /// </summary>
    public void StartProgram() {
        LoadInput();
        PreParseInput();
        programCounter = startProgramCounter;
    }
    /// <summary>
    /// Read in the input code from the user, split it per line
    /// and create an array of InputLine objects, set as non executable
    /// </summary>
    protected void LoadInput() {
        string[] loadedCommands = inputCode.Split('\n');
        List<InputLine> lCommands = new List<InputLine>();
        string[] thisCommand;
        List<string> args;

        for (int i = 0; i < loadedCommands.Length; i++) {
            if (loadedCommands[i].Length <= 0) continue;
            thisCommand = loadedCommands[i].Split();
            // generate a list of arguments (everything in that line excluding inital command
            args = new List<string>();
            for (int j = 1; j < thisCommand.Length; j++) {
                args.Add(thisCommand[j]);
            }
            // store it as a new InputLine struct
            lCommands.Add(new InputLine(thisCommand[0], args));
        }
        commands = lCommands.ToArray(); 
    }
    protected void DebugLoad() {
        LoadInput();
        PreParseInput();

        // debug
        foreach (InputLine l in commands) {
            Debug.Log(InputLine.GetString(l));
        }
    }
    /// <summary>
    /// This method needs to take in all the input lines, and decide which
    /// ones are executable, and which ones are just loop marks
    /// For 'mark x' we need to fill our markedLines dict, and set this line as non executable
    /// 
    /// commands: mark, wait, goto, seti, tjmp
    /// </summary>
    protected void PreParseInput() {
        Debug.Log("PreParseInput");
        int lastExecutable = 0;
        bool wasExecutable = false;
        bool anyExecutable = false;

        for (int i = 0; i < commands.Length; i++) {
            if (commands[i].command == "mark") {
                commands[i].executable = false;
                wasExecutable = false;
                Mark(commands[i].arguments, i);
            } else {
                // unneccessary to do anything for goto, this should be handled differently when called
                commands[i].executable = true;
                // if this command is executable, and the previous wasn't,
                // fill the InputField.nextExecutable field as this line.
                if (!wasExecutable) {
                    for (int j = lastExecutable; j < i; j++) {
                        //Debug.Log("WasExecutable Loop: " + j.ToString() + " com: " + commands[i].command);
                        commands[j].nextExecutable = i;
                    }
                }

                commands[i].nextExecutable = i + 1;
                wasExecutable = true;
                anyExecutable = true;
                lastExecutable = i;
            }
        }

        commands[commands.Length - 1].nextExecutable = -1;
        // if the first command line is not executable, we need to set the programCounter (which will denote the start 
        if (!commands[0].executable) startProgramCounter = commands[0].nextExecutable;

        CanExecute = anyExecutable;
    }

    public void StorePosition() {
        PositionStored = true;
        startPosition = thisRobot.transform.position;
    }

    /// <summary>
    /// Pre execution setup of markedLines dictionary. This is necessary as the marked lines are 
    /// not executed in program execution.
    /// </summary>
    /// <param name="command"></param>
    /// <param name="thisLine"></param>
    protected void Mark(List<string> command, int thisLine) {
        // mark [command]
        // store the current position as a position to return to
        // NON EXECUTABLE FUNCTION
        if (markedLines == null) markedLines = new Dictionary<string, int>();
        if (command.Count != 1) throw new NotImplementedException();
        if (!markedLines.ContainsKey(command[0])) markedLines.Add(command[0], thisLine); // add the current line as a location accessable through loops and goto
    }

    // Each of these execution functions should move the program counter themselves.
    // ControllableObject specific functions should rely on the Interpreter to move the program counter
    /// <summary>
    /// Find the line marked by the given ID, and go to the next executable line after it
    /// 
    /// return: an int of the next command in 'commands' to execute
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    protected int Goto(List<string> arguments, int programCounter) {
        // goto [mark]
        // jump to the given mark
        // when we recieve the goto arguments, we want to check it's argument against the dictionary of marked locations, and change the programCounter to that value
        if (arguments.Count != 1) throw new NotImplementedException();
        if (!markedLines.ContainsKey(arguments[0])) throw new NotImplementedException();

        return commands[markedLines[arguments[0]]].nextExecutable;
    }
    protected int Wait(List<string> arguments, int programCounter) {
        // wait [sec]
        // how are we going to implement wait?
        // does wait do nothing except wait on the current line for x actions?
        // or does it repeat the above line x times?
        // or should we have a seperate 'repi' for doing that
        // for now let's just wait on the current line
        if (!Int32.TryParse(arguments[0], out int waitAmount)) throw new NotImplementedException();
        if (!cur_waiting) {
            cur_wait_amount = waitAmount;
            cur_waiting = true;
        }

        if (waitAmount <= 1) {
            commands[programCounter].arguments[0] = cur_wait_amount.ToString();
            cur_waiting = false;
            return commands[programCounter].nextExecutable;
        } else {
            waitAmount--;
            commands[programCounter].arguments[0] = waitAmount.ToString();
            return programCounter;
        }
    }
    protected int Seti(List<string> arguments, int programCounter) {
        // seti [loc] [val]
        // set the given variable to the given value
        if (arguments.Count != 2) throw new NotImplementedException();
        if (Int32.TryParse(arguments[1], out int value)) {
            if (storedVariables is null) storedVariables = new Dictionary<string, int>();
            if (!storedVariables.ContainsKey(arguments[0])) {
                storedVariables.Add(arguments[0], value);
            } else {
                storedVariables[arguments[0]] = value;
            }
        } else {
            // the second argument is not an int, it must be a variable
            if (storedVariables.ContainsKey(arguments[1])) {
                if (!storedVariables.ContainsKey(arguments[0])) {
                    storedVariables.Add(arguments[0], storedVariables[arguments[1]]);
                } else {
                    storedVariables[arguments[0]] = storedVariables[arguments[1]];
                }
            } else {
                throw new NotImplementedException();
            }
        }

        return commands[programCounter].nextExecutable;
    }
    protected int Addi(List<string> arguments, int programCounter) {
        if (arguments.Count != 2) throw new NotImplementedException();
        if (!storedVariables.ContainsKey(arguments[0])) {
            // the variable doesn't exist, what should the behaviour be?
            storedVariables.Add(arguments[0], 0);
        } else {
            // the variable does exist
            if (Int32.TryParse(arguments[1], out int add)) {
                storedVariables[arguments[0]] += add;
            } else {
                if (storedVariables.ContainsKey(arguments[1])) {
                    // the second argument is a variable
                    storedVariables[arguments[0]] += storedVariables[arguments[1]];
                } else {
                    // the second argument is a variable that doesn't exist
                    throw new NotImplementedException();
                }
            }
        }
        return commands[programCounter].nextExecutable;
    }
    protected int Tjmp(List<string> arguments, int programCounter) {
        // tjmp [loc] [mark]
        // if the value of [loc] is greater than 0, jump to [mark]
        if (arguments.Count != 2) throw new NotImplementedException();
        if (storedVariables.TryGetValue(arguments[0], out int value)) {
            if (value > 0) {
                return Goto(new List<string>() { arguments[1] }, programCounter);
            } else {
                return commands[programCounter].nextExecutable;
            }
        }
        throw new NotImplementedException();
    }
}
