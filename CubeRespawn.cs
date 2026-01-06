using UnityEngine;

public class CubeRespawn : MonoBehaviour
{
    public float fallThreshold = -10f;
    public Vector3 respawnPoint = new Vector3(0,0,0);

    private Rigidbody rb;

    void Start() {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (transform.position.y < fallThreshold) {
            transform.position = respawnPoint;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }
}
