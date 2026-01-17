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
    public float initSpawnRadius = 0.15f;
    public int spawnsAtSameDist = 30; // UPDATED: Defaulted to higher value per our discussion
    public int numDistsAnchoredByTip = 25;
    public float distIncr = 0.03f;
    public int numPrevHits = 0; 

    [Header("Robot Arm Parts")]
    public GameObject armRoot;
    public Transform swordTrans;
    public float bladeLength = 0.7f; 
    public float rotLimit = 180f;

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

        // Joint State Caching
        public ConfigurableJoint configJoint;
        public Quaternion startTargetRot; // For ConfigurableJoints

        public HingeJoint hingeJoint;
        public float startHingeTarget;    // For HingeJoints (Spring Target)
    }
    private List<BodyState> armInitialStates = new List<BodyState>();

    void Start()
    {
        // Cache target rigidbody
        targetRb = target.GetComponent<Rigidbody>();
        targetRh = target.GetComponent<RegisterHit>();

        // Take snapshot of robot arm's initial state
        Rigidbody[] armBodies = armRoot.GetComponentsInChildren<Rigidbody>();
        foreach (var rb in armBodies) {
            BodyState state = new BodyState {
                rb = rb,
                startLocalPos = rb.transform.localPosition,
                startLocalRot = rb.transform.localRotation,
                wasKinematic = rb.isKinematic,
                
                // Grab joint references if they exist on this rigidbody
                configJoint = rb.GetComponent<ConfigurableJoint>(),
                hingeJoint = rb.GetComponent<HingeJoint>()
            };

            // Save initial targets to prevent "Spring Back"
            if (state.configJoint != null) {
                state.startTargetRot = state.configJoint.targetRotation;
            }
            if (state.hingeJoint != null) {
                state.startHingeTarget = state.hingeJoint.spring.targetPosition;
            }

            armInitialStates.Add(state);
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
        // 1. Lock physics first to prevent fighting
        foreach (var bs in armInitialStates) {
            bs.rb.isKinematic = true;
        }

        // 2. Reset Position AND Joint Targets
        foreach (var bs in armInitialStates) {
            // Reset Local Position
            bs.rb.transform.localPosition = bs.startLocalPos; // Should never be randomized
            // Reset Configurable Joint Target
            if (bs.configJoint != null) {
                // Generate random offset from starting position
                float xNoise = Random.Range(-rotLimit, rotLimit);
                float yNoise = Random.Range(-rotLimit, rotLimit);
                float zNoise = Random.Range(-rotLimit, rotLimit);
                Quaternion randRot = Quaternion.Euler(xNoise, yNoise, zNoise);
                // Apply offset
                bs.rb.transform.localRotation = bs.startLocalRot * randRot;
                bs.configJoint.targetRotation = bs.startTargetRot * randRot;
            } else if (bs.hingeJoint != null) {
                // Random rotational offset
                float xNoise = Random.Range(-rotLimit, rotLimit);
                // Apply offset
                bs.rb.transform.localRotation = bs.startLocalRot * Quaternion.Euler(xNoise, 0, 0);
                JointSpring spring = bs.hingeJoint.spring;
                spring.targetPosition = bs.startHingeTarget + xNoise;
                bs.hingeJoint.spring = spring;
            } else { // Target not a joint - this is the base
                bs.rb.transform.localRotation = bs.startLocalRot;
            }
        }

        // 3. Unlock physics and kill momentum
        foreach (var bs in armInitialStates) {
            bs.rb.isKinematic = bs.wasKinematic;
            if (!bs.rb.isKinematic) {
                bs.rb.linearVelocity = Vector3.zero;
                bs.rb.angularVelocity = Vector3.zero;
            }
        }
    }

    private Quaternion RandQuaternion(float rotLimit) {
        float xNoise = Random.Range(-rotLimit, rotLimit);
        float yNoise = Random.Range(-rotLimit, rotLimit);
        float zNoise = Random.Range(-rotLimit, rotLimit);
        Quaternion randRot = Quaternion.Euler(xNoise, yNoise, zNoise);
        return randRot;
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
        // Prevent the cube from spawning out of the reach of the arm
        if (Vector3.Distance(armRoot.transform.position, spawnPos) > maxSpawnRadius) {
            Vector3 dirFromRoot = (spawnPos - armRoot.transform.position).normalized;
            spawnPos = armRoot.transform.position + (dirFromRoot * maxSpawnRadius);
        }
        // Move target to position and reset all its state
        target.transform.position = spawnPos;
        target.transform.rotation = Quaternion.identity;
        targetRb.useGravity = false;
        targetRb.linearVelocity = Vector3.zero;
        targetRb.angularVelocity = Vector3.zero;
        targetRh.hit = false;
    }
}