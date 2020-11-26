using UnityEngine;

/*
 * Todo
 * - Make sure the weights sum ok, might be what causes some of the popping
 * - Make sure the animations loop ok, might be what causes some of the popping
 */
public class AnimationController : MonoBehaviour
{
    [SerializeField]
    GameObject player;

    [SerializeField]
    string idle = "idle";
    [SerializeField]
    string left = "left";
    [SerializeField]
    string right = "right";
    [SerializeField]
    string up = "up";
    [SerializeField]
    string down = "down";
    [SerializeField]
    string frontflip = "frontflip";
    [SerializeField]
    string barrelroll = "barrelroll";
    [SerializeField]
    string unstable = "unstable";
    [SerializeField]
    string startIdle = "startidle";
    [SerializeField]
    string startJump = "startJump";

    [SerializeField]
    float smoothSpeed = 16f;

    Controller playerController;
    Wing wing;

    Vector3 previousAngularVelocity;
    float previousAirspeed;
    float previousAlpha;
    bool inFlight = false;

    void Awake()
    {
        playerController = player.GetComponent<Controller>();
        wing = player.GetComponent<Wing>();

        GetComponent<Animation>().Stop();
        GetComponent<Animation>().wrapMode = WrapMode.Loop;

        GetComponent<Animation>()[idle].layer = 0;
        GetComponent<Animation>()[idle].weight = 1f;
        GetComponent<Animation>()[idle].enabled = true;

        GetComponent<Animation>()[left].layer = 1;
        GetComponent<Animation>()[right].layer = 1;
        GetComponent<Animation>()[up].layer = 1;
        GetComponent<Animation>()[down].layer = 1;

        GetComponent<Animation>()[frontflip].layer = 2;
        GetComponent<Animation>()[barrelroll].layer = 2;

        GetComponent<Animation>()[unstable].layer = 2;

        GetComponent<Animation>()[left].enabled = true;
        GetComponent<Animation>()[right].enabled = true;
        GetComponent<Animation>()[up].enabled = true;
        GetComponent<Animation>()[down].enabled = true;

        GetComponent<Animation>()[frontflip].enabled = true;
        GetComponent<Animation>()[barrelroll].enabled = true;

        GetComponent<Animation>()[unstable].enabled = true;

        GetComponent<Animation>()[startIdle].layer = 10;
        GetComponent<Animation>()[startIdle].wrapMode = WrapMode.Loop;
        GetComponent<Animation>()[startJump].layer = 10;
        GetComponent<Animation>()[startJump].wrapMode = WrapMode.Once;
    }

    void Update()
    {
        // Smooth basic velocity and heading properties of the player

        Vector3 smoothAngularVelocity = Vector3.Lerp(
            previousAngularVelocity,
            playerController.LocalAngularVelocity,
            smoothSpeed * Time.deltaTime);

        previousAngularVelocity = smoothAngularVelocity;

        float smoothAirspeed = Mathf.Lerp(
            previousAirspeed,
            wing.TrueAirspeed,
            smoothSpeed * Time.deltaTime);

        previousAirspeed = wing.TrueAirspeed;

        float smoothAlpha = Mathf.Lerp(
            previousAlpha,
            wing.AngleOfAttackPitch,
            smoothSpeed * Time.deltaTime);

        previousAlpha = wing.AngleOfAttackPitch;

        float pitchVelocity = smoothAngularVelocity.x;
        float yawVelocity = smoothAngularVelocity.y - smoothAngularVelocity.z;

        // Use the above values (with some scaling) as blend weights for the animations

        GetComponent<Animation>()[up].weight = Mathf.Clamp01(-pitchVelocity);
        GetComponent<Animation>()[down].weight = Mathf.Clamp01(pitchVelocity);
        GetComponent<Animation>()[left].weight = Mathf.Clamp01(yawVelocity);
        GetComponent<Animation>()[right].weight = Mathf.Clamp01(-yawVelocity);

        GetComponent<Animation>()[frontflip].weight = Mathf.Clamp01(-4f + Mathf.Abs(pitchVelocity));
        GetComponent<Animation>()[barrelroll].weight = Mathf.Clamp01(-4f + Mathf.Abs(yawVelocity));

        if (!inFlight && wing.TrueAirspeed > 50f)
            inFlight = true;

        if (inFlight)
        {
            // Unstable animation when stalling and losing speed
            float totalWeight = GetComponent<Animation>()[frontflip].weight + GetComponent<Animation>()[barrelroll].weight;
            float instability = Mathf.Clamp01((smoothAlpha - 38f) / 10f) * (1f - Mathf.Clamp01((smoothAirspeed - 50f) / 5f));
            GetComponent<Animation>()[unstable].weight = instability * (1f - totalWeight);
        }
    }
}