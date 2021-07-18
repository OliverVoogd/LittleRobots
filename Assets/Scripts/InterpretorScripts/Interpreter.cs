using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 
/// </summary>
public class Interpreter : MonoBehaviour
{
    /*
     * 
     * Interpreter should be responsible for recieving the code as an array of InputLines?
     */
    // GameObjects
    // These both refer to the same object
    public GameObject thisRobot = null;
    public InterpretedObject robotController = null;

    /// <summary>
    /// an array of the commands currently displayed in the CodeView object.
    /// These commands will all be valid for this particular interpreted object
    /// These commands can be parsed for command execution by an interpreter
    /// </summary>
    public Command[] activeCommands;

    private ProgramLine[] commands;

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
    Dictionary<string, int> functionEntry;
    // remove markLines, replaced with functions???
    Dictionary<string, int> markedLines; // represents the 'marked' program lines we can make when executing the program, to facilitate loops and goto statements
    Dictionary<string, int> storedVariables; // contains variables stored by the user

    private void Start() {
        if (robotController is null) robotController = thisRobot.GetComponent<ControllableObject>();

        markedLines = new Dictionary<string, int>();
        functionEntry = new Dictionary<string, int>();
        returnProgramCounterPosition = new Stack<int>();
    }

    /// <summary>
    /// This method needs to handle basic code syntax available to all robots, such as 'wait', 'goto', etc
    /// </summary>
    public void ExecuteNextLine() {
        if (!CanExecute) {
            return;
        }

        if (!commands[programCounter].executable) {
            // this should never enter
            programCounter = commands[programCounter].nextExecutable;
        }
    
        if (commands[programCounter].Type == "BuiltInCommand") {
            BuiltInCommand currentCommand = (BuiltInCommand)commands[programCounter].command;
        } else {
            SpecialCommand currentCommand = (SpecialCommand)commands[programCounter].command;

            if (currentCommand.Run(robotController) == CommandFinished.Finished) {
                programCounter = commands[programCounter].nextExecutable;
            } else {
                // Don't change programCounter
            }
        }

        if (programCounter >= commands.Length || programCounter < 0) {
            CanExecute = false;
            programCounter = -1;
        }


        /*
         * OLD
         * if (CanExecute && robotController.FinishedExecutingCommand) {
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
        */
    }

    /// <summary>
    /// Reset the state of this interpreter, allowing the code to be read and executed again
    /// </summary>
    public void Reset() {
        // what needs to be done to reset?
        activeCommands = null;
        commands = null;

        programCounter = 0;
        startProgramCounter = 0;

        // at this stage, reset moves the objects back to their start position, without animation
        if (PositionStored) {
            thisRobot.transform.position = startPosition;
        }

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
    /// Build the Command[] of the available commands for execution
    /// </summary>
    private void LoadInput() {
        activeCommands = robotController.GetActiveCommands();
    }

    /// <summary>
    /// Iterate over every Command in the activeCommands array and prepare it for
    /// executation by building a ProgramLine array of the states of the program
    /// </summary>
    private void PreParseInput() {
        commands = new ProgramLine[activeCommands.Length];

        int lastExecutable = 0;
        bool wasExecutable = false;
        bool anyExecutable = false;

        for (int i = 0; i < activeCommands.Length; i++) {
            commands[i] = new ProgramLine(activeCommands[i]);

            if (commands[i].Type == "BuiltInCommand") {
                BuiltInCommand command = (BuiltInCommand)commands[i].command;

                if (commands[i].Name == "mark") {
                    commands[i].executable = false;
                    wasExecutable = false;

                    //Mark(commands[i].arguments, i);
                }

            } else if (commands[i].Type == "SpecialCommand") {
                SpecialCommand command = (SpecialCommand)commands[i].command;
                commands[i].executable = true;

                if (!wasExecutable) {
                    for (int j = lastExecutable; j < i; j++) {
                        commands[j].nextExecutable = i;
                    }
                }

                commands[i].nextExecutable = i + 1;
                wasExecutable = true;
                anyExecutable = true;
                lastExecutable = i;

            } else {
                throw new NotImplementedException();
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
    public void Wait() {
        // wait [sec]

        // How do we properly wait?
        // just do nothing?
        return;
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
