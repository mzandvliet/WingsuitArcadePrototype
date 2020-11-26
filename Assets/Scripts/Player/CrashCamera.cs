using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CrashCamera : MonoBehaviour
{
    [SerializeField]
    Transform player;

    Quaternion previousRotation;

    public Transform Player
    {
        get { return player; }
        set { player = value; }
    }

    public void Init(Transform player)
    {
        this.player = player;
        previousRotation = transform.rotation;
    }

    void LateUpdate()
    {
        Quaternion lookRotation = Quaternion.LookRotation(player.position - transform.position);
        transform.rotation = Quaternion.Slerp(previousRotation, lookRotation, 2f * Time.deltaTime);
        previousRotation = transform.rotation;
    }
}