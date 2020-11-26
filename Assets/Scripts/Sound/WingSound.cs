using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class WingSound : MonoBehaviour
{
    [SerializeField]
    GameObject player;

    [SerializeField]
    float minPitch = 0.5f;
    [SerializeField]
    float gain = 1.0f;
    [SerializeField]
    float smoothSpeed = 16f;

    Wing wing;
    float previousAirspeed;
    float previousAlpha;

    void Start()
    {
        if (player == null)
            Debug.Log("No Player assigned to GroundSound");

        wing = player.GetComponent<Wing>();
    }

    void Update()
    {
        float smoothAirspeed = Mathf.Lerp(
            previousAirspeed,
            wing.TrueAirspeed,
            smoothSpeed * Time.deltaTime);
        previousAirspeed = smoothAirspeed;
        float smoothAlpha = Mathf.Lerp(
            previousAlpha,
            wing.AngleOfAttackPitch,
            smoothSpeed * Time.deltaTime);
        previousAlpha = smoothAlpha;

        float speedFactor = Mathf.Clamp01(smoothAirspeed / 75f);
        float alphaFactor = smoothAlpha;
        alphaFactor = (alphaFactor > 90f) ? 180f - alphaFactor : alphaFactor;
        alphaFactor /= 90f;
        
        GetComponent<AudioSource>().pitch =  minPitch + speedFactor * 1f + alphaFactor * 3f * speedFactor;
        GetComponent<AudioSource>().volume = speedFactor * 0.5f + alphaFactor * 0.5f * speedFactor * gain;
    }
}