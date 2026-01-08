using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Std; // For Float64MultiArray

public class RosArmController : MonoBehaviour
{
    [Header("ROS Settings")]
    public string topicName = "arm_command_targets";

    [Header("Joint Components")]
    // Drag your joints here in the Inspector
    public ConfigurableJoint shoulderJoint; // 3 DOF
    public HingeJoint elbowJoint;         // 1 DOF (Assuming you kept it simple)
    public ConfigurableJoint wristJoint;    // 3 DOF

    [Header("Joint Controls")]
    public float maxDelta = 2.0f;
    //public float[] shoulderLimits = new float[] { ... };
    //public float elboxLimit = ...f;
    //public float[] wristLimits = new float[] { ... };

    [Header("Debug")]
    public float[] currentTargets = new float[7]; // Just to see values in Inspector

    void Start()
    {
        // Sometimes, Unity can be finicky...
        if (currentTargets == null || currentTargets.Length != 7) { currentTargets = new float[7]; }
        // Subscribe to ROS
        ROSConnection ros = ROSConnection.GetOrCreateInstance();
        ros.Subscribe<Float64MultiArrayMsg>(topicName, OnRosMessageReceived);
    }

    void OnRosMessageReceived(Float64MultiArrayMsg msg)
    {
        // 1. Validation: Ensure we got enough data
        if (msg.data.Length < 7) {
            Debug.LogError($"ROS sent {msg.data.Length} values, but we need 7!");
            return;
        }

        // Scale rotation deltas
        for(int i=0; i<msg.data.Length; i++) currentTargets[i] = (float)msg.data[i] * maxDelta;

        // 2. Parse the Data (Indices 0-6)
        
        // --- SHOULDER (Indices 0, 1, 2) ---
        // Convert the 3 Euler angles into a Quaternion
        float sX = currentTargets[0];
        float sY = currentTargets[1];
        float sZ = currentTargets[2];
        Quaternion shoulderRot = Quaternion.Euler(sX, sY, sZ);
        
        // Apply to Configurable Joint (Remember: TargetRotation is usually Inverted logic)
        shoulderJoint.targetRotation *= Quaternion.Inverse(shoulderRot);


        // --- ELBOW (Index 3) ---
        // Simple Hinge Joint logic
        float elbowAngle = currentTargets[3];
        JointSpring spr = elbowJoint.spring;
        spr.targetPosition += elbowAngle;
        elbowJoint.spring = spr;


        // --- WRIST (Indices 4, 5, 6) ---
        float wX = currentTargets[4];
        float wY = currentTargets[5];
        float wZ = currentTargets[6];
        Quaternion wristRot = Quaternion.Euler(wX, wY, wZ);

        wristJoint.targetRotation *= Quaternion.Inverse(wristRot);
    }
}