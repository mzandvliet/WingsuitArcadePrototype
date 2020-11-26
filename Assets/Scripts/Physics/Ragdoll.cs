using UnityEngine;

public class Ragdoll : MonoBehaviour
{
    void Update()
    {
        if (GetComponent<Rigidbody>().velocity.sqrMagnitude > 0.005f)
            Destroy(gameObject, 10f);
    }
}