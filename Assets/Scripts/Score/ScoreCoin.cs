using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class ScoreCoin : MonoBehaviour
{
    [SerializeField]
    Animation coinAnimation;
    [SerializeField]
    float scoreValue = 500f;


    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            ScoreManager.Instance.AddScore(scoreValue);
            coinAnimation.Play("pickup");
            Destroy(gameObject, 0.25f);
            GetComponent<AudioSource>().Play();
        }
    }
}