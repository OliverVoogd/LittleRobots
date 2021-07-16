using System;
using UnityEngine;
using TMPro;

public class MagnitudeField : MonoBehaviour {
    [SerializeField]
    int min_value = 0;
    [SerializeField]
    int max_value = 10;
    TMP_InputField magnitude_field;

    void Awake() {
        magnitude_field = GetComponent<TMP_InputField>();
    }

    public void DecreaseMagnitudeField() {
        ChangeMagnitudeField(-1);
    }

    public void IncreaseMagnitudeField() {
        ChangeMagnitudeField(1);
    }

    private void ChangeMagnitudeField(int amount) {
        magnitude_field.text = Math.Min(Math.Max(Int32.Parse(magnitude_field.text) + amount, min_value), max_value).ToString();
        
    }
}
