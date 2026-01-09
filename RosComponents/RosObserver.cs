using UnityEngine;
using RosMessageTypes.Duel;
using RosMessageTypes.Std;
using RosMessageTypes.Geometry;
using Unity.Robotics.ROSTCPConnector;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;

public class RosObserver : MonoBehaviour
{
    [Header("ROS Settings")]
    public string topicName = "arm_observations";

    [Header("Observed Components")]
    public ConfigurableJoint shoulderJoint;
    public HingeJoint elbowJoint;
    public ConfigurableJoint wristJoint;
    public GameObject cubeTarget;

    [Header("Other Settings")]
    public float publishFrequency = 0.1f;

    // Hold on to the ros connection
    private ROSConnection ros;
    private float timeElapsed = 0f;

    // Cached properties to save compute
    private Rigidbody shoulderRb;
    private Rigidbody wristRb;
    private Transform cubeTrans;
    private RegisterHit cubeHit;

    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {   
        // Establish ros connection and register publisher
        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterPublisher<DuelBotObservationMsg>(topicName);
        // Cache properties
        shoulderRb = shoulderJoint.GetComponent<Rigidbody>();
        wristRb = wristJoint.GetComponent<Rigidbody>();
        cubeTrans = cubeTarget.GetComponent<Transform>();
        cubeHit = cubeTarget.GetComponent<RegisterHit>();
    }

    void Update()
    {
        timeElapsed += Time.deltaTime;

        if (timeElapsed > publishFrequency) {
            PublishObservation();
            timeElapsed = 0;
        }
    }

    void PublishObservation()
    {
        DuelBotObservationMsg msg = new DuelBotObservationMsg();

        msg.relative_target_position = (cubeTrans.position - wristJoint.transform.position).To<FLU>();
        msg.sword_rotation = wristJoint.transform.rotation.To<FLU>();
        msg.shoulder_rotation = shoulderJoint.transform.localRotation.To<FLU>();
        msg.elbow_rotation = elbowJoint.angle;
        msg.wrist_rotation = wristJoint.transform.localRotation.To<FLU>();
        msg.shoulder_vel = shoulderRb.angularVelocity.To<FLU>();
        msg.elbow_vel = elbowJoint.velocity;
        msg.wrist_vel = wristRb.angularVelocity.To<FLU>();
        msg.hit_target = cubeHit.hit;

        //Debug.Log("Sending Message");
        ros.Publish(topicName, msg);
    }
}
