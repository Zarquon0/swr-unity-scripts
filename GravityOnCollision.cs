using UnityEngine;

public class GravityOnCollision : MonoBehaviour
{
    private Rigidbody rb;

    void Start() {
        rb = GetComponent<Rigidbody>();
    }
    
    void OnCollisionEnter(Collision collision) {
        rb.useGravity = true;
    }
}