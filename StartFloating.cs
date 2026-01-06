using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables; // Needed to check for grabs

public class SlowFloat : MonoBehaviour
{
    [Tooltip("How fast (in m/s) the object drifts down.")]
    public float floatSpeed = 0.2f;

    private bool isFloating = true;
    private Rigidbody rb;
    private XRGrabInteractable grabInteractable;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        grabInteractable = GetComponent<XRGrabInteractable>();

        // 1. Turn off real gravity immediately
        rb.useGravity = false;
    }

    void FixedUpdate()
    {
        // Only run this logic if we are still in "Float Mode"
        if (isFloating)
        {
            // SAFETY CHECK: If the player grabs it mid-air, stop the artificial float
            // so we don't fight the hand tracking.
            if (grabInteractable != null && grabInteractable.isSelected)
            {
                ActivatePhysics();
                return;
            }

            // 2. Force a constant, slow downward velocity
            // (Using 'linearVelocity' for Unity 6 / PhysX 5)
            rb.linearVelocity = new Vector3(0, -floatSpeed, 0);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        // 3. The moment we touch ANYTHING solid (Floor, Table, etc.), activate real physics
        if (isFloating)
        {
            ActivatePhysics();
        }
    }

    void ActivatePhysics()
    {
        isFloating = false;
        rb.useGravity = true;
        
        // Optional: Remove this script component so it doesn't waste CPU
        Destroy(this); 
    }
}