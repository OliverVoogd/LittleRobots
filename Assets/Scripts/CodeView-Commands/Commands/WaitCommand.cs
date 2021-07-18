using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaitCommand : BuiltInCommand
{
    private int initialWaitAmount;
    private int currentWaitAmount;

    public WaitCommand(int amount) : base("wait") {
        initialWaitAmount = currentWaitAmount = amount;
    }

    /// <summary>
    /// Command:
    ///    wait [sec] 
    /// </summary>
    /// <param name="robot"></param>
    /// <param name="interpreter"></param>
    /// <returns></returns>
    public override CommandFinished Run(InterpretedObject robot, Interpreter interpreter) {
        interpreter.Wait();

        currentWaitAmount--;
        if (currentWaitAmount == 0) {
            Reset();
            return CommandFinished.Finished;
        } else {
            return CommandFinished.NotFinished;
        }
    }

    private void Reset() {
        currentWaitAmount = initialWaitAmount;
    }
}
