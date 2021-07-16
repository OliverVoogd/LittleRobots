using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CodeView : MonoBehaviour
{
    [SerializeField]
    string[] commandNames;
    [SerializeField]
    CommandLine[] commandPrefabs;

    Dictionary<string, CommandLine> commandInputPairs;

    // how do we display a certain number of command lines which is scrollable??

    private void Awake() {
        populateCommandDictionary();
    }

    private void populateCommandDictionary() {
        commandInputPairs = new Dictionary<string, CommandLine>();

        // Populate the dictionary of possible commands
        for (int i = 0; i < commandNames.Length; i++) {
            commandInputPairs.Add(commandNames[i], commandPrefabs[i]);
        }
    }
}
