using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit; // Core XR namespace
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation; // Specific for Teleporting

public class SimpleRespawn : MonoBehaviour
{
    [Tooltip("Drag the 'Locomotion' object here (it must have a TeleportationProvider component)")]
    public TeleportationProvider teleportationProvider;

    [Tooltip("The height at which you reset")]
    public float fallThreshold = -10f;

    [Tooltip("Where to land. Leave empty for (0,0,0)")]
    public Transform spawnPoint;

    void Update()
    {
        // Check height (We check the XR Origin's height)
        if (transform.position.y < fallThreshold)
        {
            Respawn();
        }
    }

    void Respawn()
    {
        if (teleportationProvider == null) return;

        // Create a request to teleport to a specific location
        TeleportRequest request = new TeleportRequest()
        {
            destinationPosition = spawnPoint ? spawnPoint.position : Vector3.zero,
            destinationRotation = spawnPoint ? spawnPoint.rotation : Quaternion.identity,
            matchOrientation = MatchOrientation.TargetUpAndForward
        };

        // Submit the request to the XR system
        teleportationProvider.QueueTeleportRequest(request);
    }
}