using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class ScoreGate : MonoBehaviour
{
    [SerializeField]
    float scoreValue = 250f;

    [SerializeField]
    Animation leftAnimation;
    [SerializeField]
    Animation rightAnimation;

    void Start()
    {
        leftAnimation.Stop();
        rightAnimation.Stop();

        leftAnimation.Play();
        StartCoroutine(WaitASecAndPlay());
    }

    IEnumerator WaitASecAndPlay()
    {
        yield return new WaitForSeconds(0.33f);
        rightAnimation.Play();
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player") {
            ScoreManager.Instance.AddScore(scoreValue);
            GetComponent<AudioSource>().Play();
        }
    }
}