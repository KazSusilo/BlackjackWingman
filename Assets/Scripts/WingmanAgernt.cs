using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class WingmanAgernt : Agent {
    public override void Initialize() {
        Debug.Log("Initialize()");
    }

    public override void OnEpisodeBegin() {
        Debug.Log("OnEpisodeBegin()");
    }

    public override void CollectObservations(VectorSensor sensor) {
        // Placeholder: Add observations here
    }

    public override void OnActionReceived(ActionBuffers actions) {
        // Placeholder: Add action logic here
    }
}
