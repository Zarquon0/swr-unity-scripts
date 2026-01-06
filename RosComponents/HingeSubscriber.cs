using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Std; // Access to standard ROS messages like Float64

public class HingeSubscriber : MonoBehaviour
{
    public HingeJoint joint; // The Hinge Joint we want to control 
    public string topicName = "shoulder_pan_pos"; // The ROS Topic Name

    void Start()
    {
        ROSConnection ros = ROSConnection.GetOrCreateInstance();
        ros.Subscribe<Float64Msg>(topicName, MoveJoint);
    }

    void MoveJoint(Float64Msg msg)
    {
        // float targetAngle = (float)msg.data;
        // joint.spring.targetPosition = targetAngle; // Set the target angle
    }
}