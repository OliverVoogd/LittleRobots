using System;
using UnityEngine;
using TMPro;

public class WaitCommandLine : CommandLine {
    [SerializeField]
    TMP_InputField magnitude_field;
    public override Command GetCommand() {
        return new WaitCommand(Int32.Parse(magnitude_field.text));
    }
}
