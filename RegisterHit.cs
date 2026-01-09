using UnityEngine;

public class RegisterHit : MonoBehaviour
{
    [Tooltip("Has this object experienced a collision?")]
    public bool hit = false;

    void OnCollisionEnter(Collision collision) {
        hit = true;
    }

}
