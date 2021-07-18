using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProgramHandler : MonoBehaviour
{
    /// <summary>
    /// This class will hold all available interpreters and handle the execution 
    /// of all code steps of all available interpreters at the same time.
    /// </summary>
    /// 
    [SerializeField]
    CodeView codeViewPrefab;

    [SerializeField]
    Canvas canvas;

    [SerializeField]
    float executeTime = 1; // in seconds
    public float ExecuteTime {
        get {
            return executeTime;
        }
    }
    public Rect executeWindow = new Rect(Screen.width - 100, Screen.height - 50, 100, 50);
    List<Interpreter> interpreters;

    bool execute, justStopped = false;
    float timeElapsed;

    // Start is called before the first frame update
    void Start()
    {
        execute = false;
        timeElapsed = executeTime;
    }

    // Update is called once per frame
    void Update()
    {
        // alternative to clicking execute button
        if (Input.GetKeyDown(KeyCode.F1)) {
            execute = true;
            RunPrograms();
        }

        // execute the next click
        timeElapsed += Time.deltaTime;
        if (justStopped && timeElapsed >= executeTime) {
            foreach (Interpreter i in interpreters) {
                // i.ReWriteInput();
                justStopped = false;
            }
        }
        if ((execute) && (timeElapsed >= executeTime)) {
            // we need to loop through all interpreters and execute the next line.
            // each interpreter should be responsible for managing loops, waits and gotos, and this class shouldn't have to worry about 
            // anything beyond executing the next line.
            timeElapsed = 0;
            bool stillExecuting = false;
            foreach (Interpreter i in interpreters) {
                i.ExecuteNextLine();

                stillExecuting = stillExecuting || i.CanExecute;
            }
            if (!stillExecuting) {
                execute = false;
                justStopped = true;
            }
        }

        if (!execute && Input.GetKeyDown(KeyCode.F3)) {
            foreach(Interpreter i in interpreters) {
                i.Reset();
            }
        }
    }

    private void RunPrograms() {
        foreach (Interpreter i in interpreters) {
            i.Reset();
            i.StartProgram();
            if (!i.PositionStored) {
                i.StorePosition();
            }
        }
    }
    public void StoreInterpreter(Interpreter i) {
        if (interpreters is null) interpreters = new List<Interpreter>();
        interpreters.Add(i);
    }

    private void OnGUI() {
        executeWindow = new Rect(Screen.width - 100, Screen.height - 50, 100, 50);
        executeWindow = GUI.Window(0, executeWindow, DoMyWindow, "");

        
    }

    void DoMyWindow(int windowID) {
        if (GUI.Button(new Rect(0, 0, 100, 50), "Execute") && !execute) {
            RunPrograms();
            execute = true;
        }
    }

    public CodeView CreateCodeView() {
        CodeView view = Instantiate<CodeView>(codeViewPrefab, canvas.transform);

        return view;
    }
}

