using System;
using UnityEngine;


public class MoveCommand : SpecialCommand {
    static string[] possibleDirections = { "up", "down", "left", "right" };
    string direction;
    int distance;
    bool movingToTarget = false;

    public MoveCommand(string direction, int distance) : base("move") {
        direction = direction.ToLower();
        if (!checkValidDirection(direction)) {
            throw new NotImplementedException();
        }

        this.direction = direction;
        this.distance = distance;
    }

    private bool checkValidDirection(string direction) {
        foreach (string direct in possibleDirections) {
            if (direction.Equals(direct)) {
                return true;
            }
        }
        return false;
    }

    public override CommandFinished Run(InterpretedObject robot) {
        Debug.Log("IT WORKS! INSIDE MOVECOMMAND.RUN");
        Debug.Log(direction + " " + distance.ToString());
        return CommandFinished.Finished;
    }

}