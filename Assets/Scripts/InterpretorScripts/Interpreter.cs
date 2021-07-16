using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Interpreter : MonoBehaviour
{
    /*
     * 
     * Interpreter should be responsible for recieving the code as an array of InputLines?
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
    private string inputCode = "define func1\nmove up 2\nmove right 2\nmove down 5\nmove left 2\nmove up 3\nend\nmove right 1\ngoto func1\nwait 3\n";
    //private string inputCode = 
    //"move left 1\nmove right 1\nseti count 0\nmark first\nmove right 1\naddi count 1\nseti test count\naddi test -1\ntjmp test out\ngoto first\nmark out";

    public InterpretedObject robotController = null;

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

    // function handling
    private Stack<int> returnProgramCounterPosition;

    // code execution
    //List<List<string>> commands; // the lines that can actually be executed
    public InputLine[] commands; // struct of all the lines.
    Dictionary<string, int> functionEntry;
    // remove markLines, replaced with functions???
    Dictionary<string, int> markedLines; // represents the 'marked' program lines we can make when executing the program, to facilitate loops and goto statements
    Dictionary<string, int> storedVariables; // contains variables stored by the user

    private void Start() {
        // Setup required object references;
        cam = GameObject.FindObjectOfType<Camera>();

        if (robotController is null) robotController = thisRobot.GetComponent<ControllableObject>();
        SetupInputField();

        markedLines = new Dictionary<string, int>();
        functionEntry = new Dictionary<string, int>();
        returnProgramCounterPosition = new Stack<int>();
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
            // check for basic instructions
            switch (currentCommand) {
                case "end":
                    programCounter = End(currentLine, programCounter);
                    ExecuteNextLine();
                    break;
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
        functionEntry = new Dictionary<string, int>();
        storedVariables = new Dictionary<string, int>();
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
        // THIS NEEDS TO CHANGE FOR NEW SYSTEM
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
    /// For define x, we fill our function definition dictionary, and continue until we reach 'end', then 
    /// regular parsing can continue. Later, during program execution, when a function is called,
    /// 'returnProgramCounterPosition' will be pushed to, the function will execute, and 'end' will return the programCounter
    /// to whatever is on top of the returnProgramCounterPosition stack.
    /// 
    /// commands: mark, wait, goto, seti, tjmp
    /// </summary>
    protected void PreParseInput() {
        int lastExecutable = 0;
        int functionCount = 0;
        bool wasExecutable = false;
        bool anyExecutable = false;

        for (int i = 0; i < commands.Length; i++) {
            /*
            if (commands[i].command == "mark") {
                commands[i].executable = false;
                wasExecutable = false;
                Mark(commands[i].arguments, i);
            }*/
            if (commands[i].command == "define") {
                functionCount++;
                DefineFunction(commands[i].arguments, i + 1);
            } else {
                if (functionCount == 0) {
                    startProgramCounter = i;
                    functionCount = -10;
                }

                if (commands[i].command == "end") functionCount--;

                // unneccessary to do anything for goto, this should be handled differently when called
                commands[i].executable = true;
                // if this command is executable, and the previous wasn't,
                // fill the InputField.nextExecutable field as this line.
                if (!wasExecutable) {
                    for (int j = lastExecutable; j < i; j++) {
                        commands[j].nextExecutable = i;
                    }
                }

                commands[i].nextExecutable = i + 1;
                wasExecutable = true;
                anyExecutable = true;
                lastExecutable = i;
            }
        }

        CanExecute = anyExecutable;
    }

    public void StorePosition() {
        PositionStored = true;
        startPosition = thisRobot.transform.position;
    }

    public void DestroyInterpreter() {
        Destroy(this.gameObject);
        
    }

    #region Basic Available Commands

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
    /// <summary>
    /// Define the current line as the start of a function, which ends with a 'end' command.
    /// </summary>
    /// <param name="command"></param>
    /// <param name="thisLine"></param>
    protected void DefineFunction(List<string> command, int thisLine) {
        if (command.Count != 1) throw new NotImplementedException();
        if (functionEntry == null) functionEntry = new Dictionary<string, int>();
        functionEntry.Add(command[0], thisLine);
    }

    // Each of these execution functions should move the program counter themselves.
    // ControllableObject specific functions should rely on the Interpreter to move the program counter
    /// <summary>
    /// Call the given function, popping the return position to the stack
    /// </summary>
    /// <param name="arguments"></param>
    /// <param name="programCounter"></param>
    /// <returns></returns>
    protected int Goto(List<string> arguments, int programCounter) {
        // goto [mark]
        // jump to the given mark
        // when we recieve the goto arguments, we want to check it's argument against the dictionary of marked locations, and change the programCounter to that value
        if (arguments.Count != 1) throw new NotImplementedException();
        if (!functionEntry.ContainsKey(arguments[0])) throw new NotImplementedException();

        returnProgramCounterPosition.Push(programCounter + 1);
        return functionEntry[arguments[0]];
    }
    /// <summary>
    /// Upon reaching the end of a function, simply pop whatever is on the top of the returnProgramCounterPosition stack
    /// and resume the program at that position
    /// </summary>
    /// <param name="arguments"></param>
    /// <param name="programCounter"></param>
    /// <returns></returns>
    protected int End(List<string> arguments, int programCounter) {
        if (returnProgramCounterPosition.Count == 0) throw new NotImplementedException();
        return returnProgramCounterPosition.Pop();
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

    #endregion
}
