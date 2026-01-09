using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Std;
using System.Collections.Generic;

public class TrainingReset : MonoBehaviour
{
    [Header("ROS Settings")]
    public string serviceName = "duel_bot/reset";

    [Header("Target Settings")]
    public GameObject target;
    public float spawnRadius = 1.5f;

    [Header("Robot Arm Parts")]
    public GameObject armRoot;

    // Cached object properties
    private ROSConnection ros;
    private Rigidbody targetRb;
    private RegisterHit targetRh;
    // Robot arm initial snapshot
    private struct BodyState {
        public Rigidbody rb;
        public Vector3 startLocalPos;
        public Quaternion startLocalRot;
        public bool wasKinematic;
    }
    private List<BodyState> armInitialStates = new List<BodyState>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Cache target rigidbody
        targetRb = target.GetComponent<Rigidbody>();
        targetRh = target.GetComponent<RegisterHit>();
        // Take snapshot of robot arm's initial state
        Rigidbody[] armBodies = armRoot.GetComponentsInChildren<Rigidbody>();
        foreach (var rb in armBodies) {
            armInitialStates.Add(new BodyState {
                rb = rb,
                startLocalPos = rb.transform.localPosition,
                startLocalRot = rb.transform.localRotation,
                wasKinematic = rb.isKinematic
            });
        }
        // Start up ros connection and register service
        ros = ROSConnection.GetOrCreateInstance();
        ros.ImplementService<TriggerRequest, TriggerResponse>(serviceName, Reset);
    }

    private TriggerResponse Reset(TriggerRequest req) {
        ResetRobot();
        RespawnTarget();
        return new TriggerResponse { success = true };
    }

    private void ResetRobot() {
        foreach (var bs in armInitialStates) {
            bs.rb.isKinematic = true;
        }
        foreach (var bs in armInitialStates) {
            bs.rb.transform.localPosition = bs.startLocalPos;
            bs.rb.transform.localRotation = bs.startLocalRot;
        }
        foreach (var bs in armInitialStates) {
            bs.rb.isKinematic = bs.wasKinematic;
            if (!bs.rb.isKinematic) {
                bs.rb.linearVelocity = Vector3.zero;
                bs.rb.angularVelocity = Vector3.zero;
            }
        }
    }

    void RespawnTarget() {
        // Determine new target position first in spherical, then convert to cartesian
        Vector3 randDir = Random.onUnitSphere;
        Vector3 spawnPos = armRoot.transform.position + randDir*spawnRadius;
        // Move target to position and reset all its state
        target.transform.position = spawnPos;
        target.transform.rotation = Quaternion.identity;
        targetRb.useGravity = false;
        targetRb.linearVelocity = Vector3.zero;
        targetRb.angularVelocity = Vector3.zero;
        targetRh.hit = false;
    }
}
