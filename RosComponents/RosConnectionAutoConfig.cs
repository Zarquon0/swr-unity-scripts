using UnityEngine;
using Unity.Robotics.ROSTCPConnector;

public class RosConnectionAutoConfig : MonoBehaviour
{
    // Set this in Inspector to your Mac's real LAN IP
    public string hostIpAddress = "192.168.1.162"; 

    void Awake()
    {
        ROSConnection ros = ROSConnection.GetOrCreateInstance();

        #if UNITY_EDITOR
            // In Editor, always stay local for speed/reliability
            ros.RosIPAddress = "127.0.0.1";
            Debug.Log("Auto-Config: Using Localhost for Editor.");
        #else
            // In the Build (Quest), use the real IP
            ros.RosIPAddress = hostIpAddress;
            Debug.Log($"Auto-Config: Using Host IP {hostIpAddress} for Build.");
        #endif
    }
}