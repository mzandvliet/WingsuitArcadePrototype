using UnityEngine;

public class GoalZone : MonoBehaviour
{
    public event PlayerFinishedHandler OnPlayerFinished;

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            if (OnPlayerFinished != null)
                OnPlayerFinished();
        }
    }

    public delegate void PlayerFinishedHandler();
}