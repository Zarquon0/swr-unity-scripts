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
    public Transform swordTrans;
    public GameObject cubeTarget;

    [Header("Other Settings")]
    public float publishFrequency = 0.1f;
    public float bladeLength = 0.7f;

    // Hold on to the ros connection
    private ROSConnection ros;
    private float timeElapsed = 0f;

    // Cached properties to save compute
    private Rigidbody shoulderRb;
    private Rigidbody wristRb;
    private Transform cubeTrans;
    private RegisterHit cubeHit;
    private Vector3 bladeVec;

    
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
        bladeVec = new Vector3(0, bladeLength, 0);
    }

    void Update()
    {
        timeElapsed += Time.deltaTime;

        if (timeElapsed > publishFrequency) {
            PublishObservation();
            timeElapsed = 0;
        }
    }

    private Vector3 GetClosestPointOnSegment(Vector3 start, Vector3 end, Vector3 point)
    {
        Vector3 line = end - start;
        float lenSq = line.sqrMagnitude;
        
        // Safety: If the sword has 0 length, return the start
        if (lenSq < 1e-4f) return start; 

        // Project vector (start->point) onto vector (start->end)
        Vector3 startToPoint = point - start;
        float t = Vector3.Dot(startToPoint, line) / lenSq;

        // Clamp t to the segment [0, 1] so we don't extend past the tip or handle
        t = Mathf.Clamp01(t);

        return start + line * t;
    }

    void PublishObservation()
    {
        DuelBotObservationMsg msg = new DuelBotObservationMsg();
        
        Vector3 tipPos = swordTrans.TransformPoint(bladeVec);
        Vector3 closestPoint = GetClosestPointOnSegment(swordTrans.position, tipPos, cubeTrans.position);
        msg.handle_position = swordTrans.position.To<FLU>();
        msg.tip_position = tipPos.To<FLU>();
        msg.closest_blade_position = closestPoint.To<FLU>();
        msg.target_position = cubeTrans.position.To<FLU>();
        msg.shoulder_rotation = shoulderJoint.transform.localRotation.To<FLU>();
        msg.elbow_rotation = elbowJoint.angle;
        msg.wrist_rotation = wristJoint.transform.localRotation.To<FLU>();
        msg.shoulder_vel = shoulderRb.angularVelocity.To<FLU>();
        msg.elbow_vel = elbowJoint.velocity;
        msg.wrist_vel = wristRb.angularVelocity.To<FLU>();
        msg.hit_target = cubeHit.hit;

        //Debug.Log("Sending Message");
        Debug.DrawLine(closestPoint, cubeTrans.position, Color.red, publishFrequency);
        Debug.DrawLine(swordTrans.position, tipPos, Color.red, publishFrequency);
        ros.Publish(topicName, msg);
    }
}
