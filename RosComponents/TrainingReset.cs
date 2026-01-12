using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Std;
using System.Collections.Generic;

public class TrainingReset : MonoBehaviour
{
    [Header("ROS Settings")]
    public string serviceName = "duel_bot/reset";

    [Header("Target/Respawn Settings")]
    public GameObject target;
    public float maxSpawnRadius = 1.5f;
    public float initSpawnRadius = 0.01f;
    public int spawnsAtSameDist = 1;
    public int numDistsAnchoredByTip = 25;
    public float distIncr = 0.03f;
    public int numPrevHits = 0; 

    [Header("Robot Arm Parts")]
    public GameObject armRoot;
    public Transform swordTrans;
    public float bladeLength = 0.7f; //NOTE: If this is editted during runtime, nothing will change (initial value cached)

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
        if (targetRh.hit) { numPrevHits++; }
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
        Vector3 anchorPoint;
        float spawnRadius;
        if (numPrevHits < numDistsAnchoredByTip * spawnsAtSameDist) {
            spawnRadius = (numPrevHits / spawnsAtSameDist)*distIncr + initSpawnRadius;
            anchorPoint = swordTrans.TransformPoint(new Vector3(0, bladeLength, 0));
        } else {
            spawnRadius = (numPrevHits / spawnsAtSameDist)*distIncr + initSpawnRadius;
            if (spawnRadius > maxSpawnRadius) { spawnRadius = maxSpawnRadius; }
            anchorPoint = armRoot.transform.position;
        }
        Vector3 spawnPos = anchorPoint + randDir*spawnRadius;
        // Move target to position and reset all its state
        target.transform.position = spawnPos;
        target.transform.rotation = Quaternion.identity;
        targetRb.useGravity = false;
        targetRb.linearVelocity = Vector3.zero;
        targetRb.angularVelocity = Vector3.zero;
        targetRh.hit = false;
    }
}
