using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class GroundSound : MonoBehaviour
{
    [SerializeField]
    GameObject player;

    Wing wing;
    Controller controller;

    [SerializeField]
    public float gain = 1.0f;

    void Start()
    {
        if (player == null)
            Debug.Log("No Player assigned to GroundSound");

        wing = player.GetComponent<Wing>();
        controller = player.GetComponent<Controller>();
    }

    void Update()
    {
        float speedFactor = wing.TrueAirspeed / 75f;
        float proximityFactor = (100f - Mathf.Clamp(controller.Altitude, 0f, 100f)) / 100f;
        float angVelFactor = controller.LocalAngularVelocity.magnitude / 8f;

        GetComponent<AudioSource>().pitch = 1f + angVelFactor * speedFactor + proximityFactor * speedFactor * 2f;
        GetComponent<AudioSource>().volume = speedFactor * 0.4f + (angVelFactor + proximityFactor) * speedFactor;
        GetComponent<AudioSource>().volume *= gain;
    }
}