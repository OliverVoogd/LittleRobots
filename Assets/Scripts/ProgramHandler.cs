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

        timeElapsed += Time.deltaTime;
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
            if (!stillExecuting) execute = false;
        }
    }

    public void StoreInterpreter(Interpreter i) {
        if (interpreters is null) interpreters = new List<Interpreter>();
        interpreters.Add(i);
    }
}
