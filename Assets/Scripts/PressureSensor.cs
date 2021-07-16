using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PressureSensor : MonoBehaviour {
    [SerializeField]
    float triggerWeight = 10.0f;
    [SerializeField]
    float triggerTestHeight = 5.0f;
    [SerializeField]
    float triggerTestRadius = 2.2f;
    [SerializeField]
    LayerMask triggerMask;

    protected bool triggerActive = true;
    protected virtual void Update() {
        CheckTrigger();
    }

    protected virtual void CheckTrigger() {
        Collider[] colliders = Physics.OverlapSphere(transform.position + Vector3.up * triggerTestHeight, triggerTestRadius, triggerMask);

        foreach (Collider collider in colliders) {
            if (collider.GetComponent<Weighted>() != null &&
                collider.GetComponent<Weighted>().GetWeight >= triggerWeight) {
                triggerActive = true;
                return;
            }
        }

        triggerActive = false;
    }

    private void OnDrawGizmos() {
        Gizmos.color = Color.magenta;
        Gizmos.DrawSphere(transform.position + Vector3.up * triggerTestHeight, triggerTestRadius);
    }
}
