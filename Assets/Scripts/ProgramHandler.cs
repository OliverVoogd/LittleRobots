using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProgramHandler : MonoBehaviour
{
    /// <summary>
    /// This class will hold all available interpreters and handle the execution 
    /// of all code steps of all available interpreters at the same time.
    /// </summary>
    /// 
    [SerializeField]
    float executeTime; // in seconds

    public Rect executeWindow = new Rect(Screen.width - 100, Screen.height - 50, 100, 50);
    
    List<Interpreter> interpreters;

    bool execute;
    float timeElapsed = 0;
    // Start is called before the first frame update
    void Start()
    {
        execute = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1)) {
            execute = true;
        }
        if (Input.GetKeyDown(KeyCode.F2)) {
            foreach (Interpreter i in interpreters) {
                i.LoadInput();
            }
        }

        timeElapsed += Time.deltaTime;
        if ((execute) && (timeElapsed >= executeTime)) {
            // we need to loop through all interpreters and execute the next line.
            // each interpreter should be responsible for managing loops, waits and gotos, and this class shouldn't have to worry about 
            // anything beyond executing the next line.
            timeElapsed = 0;
            bool stillExecuting = false;
            foreach (Interpreter i in interpreters) {
                i.ReWriteInput();
                i.ExecuteNextLine();

                stillExecuting = stillExecuting || i.CanExecute;
            }
            if (!stillExecuting) execute = false;

        }
    }

    private void RunPrograms() {

    }
    public void StoreInterpreter(Interpreter i) {
        if (interpreters is null) interpreters = new List<Interpreter>();
        interpreters.Add(i);
    }

    private void OnGUI() {
        executeWindow = GUI.Window(0, executeWindow, DoMyWindow, "");
    }

    void DoMyWindow(int windowID) {
        if (GUI.Button(new Rect(0, 0, 100, 50), "Execute")) {
            foreach (Interpreter i in interpreters) {
                i.LoadInput();
            }
            execute = true;
        }
    }
}

