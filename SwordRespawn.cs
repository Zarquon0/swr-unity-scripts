using UnityEngine;
// We shorten this namespace so we don't have to type it out every time
using UnityEngine.XR.Interaction.Toolkit.Interactables; 

public class ObjectRespawn : MonoBehaviour
{
    [Tooltip("Where should this object go when it falls? (Drag the BackHolster here)")]
    public Transform returnPoint;

    [Tooltip("How low can it fall before respawning? (e.g., -20 meters)")]
    public float fallThreshold = -10f;

    private Rigidbody rb;
    private XRGrabInteractable grabInteractable;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        grabInteractable = GetComponent<XRGrabInteractable>();
    }

    void Update()
    {
        // 1. Check if we are below the threshold (Infinite Floor Logic)
        if (transform.position.y < fallThreshold)
        {
            // 2. Check if the object is NOT currently being held
            // (isSelected is the modern property for "Is a hand holding this?")
            if (!grabInteractable.isSelected)
            {
                RespawnObject();
            }
        }
    }

    void RespawnObject()
    {
        // 3. Stop physics momentum (Unity 6 / PhysX 5.0 Syntax)
        // If this gives an error on older Unity versions, change back to .velocity
        rb.linearVelocity = Vector3.zero; 
        rb.angularVelocity = Vector3.zero;

        // 4. Move it to the holster
        transform.position = returnPoint.position;
        transform.rotation = returnPoint.rotation;
        
        // The Socket Interactor will automatically "catch" it from here!
    }
}