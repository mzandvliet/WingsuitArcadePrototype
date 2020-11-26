using UnityEngine;

/*
 * Todo
 * - Drag from the side
 */
public class Wing : MonoBehaviour
{
    [SerializeField]
    Vector2 wingDimensions;

    [SerializeField]
    AnimationCurve cLiftCurve;
    [SerializeField]
    AnimationCurve cDragCurve;
    [SerializeField]
    Vector3 centerOfPressure;
    [SerializeField]
    AnimationCurve sideFalloffCurve;

    float liftMultiplier = 1f; // Used to simulate ground effect by other script

    float surfaceArea;
    float cLift;
    float lift;
    Vector3 liftVector;
    float drag;
    Vector3 dragVector;
    Vector3 sideSlip;
    Vector3 relativeVelocity;
    float angleOfAttackPitch;
    float angleOfAttackYaw;
    float critPitchAlpha;
    float trueAirspeed;

    Atmosphere atmosphere;

    /* Scale of the debug lines drawn. */
    public const float DebugForceScale = 0.1f;
    public const float DebugVelocityScale = 1f;

    public float TrueAirspeed
    {
        get { return trueAirspeed; }
    }

    public float AngleOfAttackPitch
    {
        get { return angleOfAttackPitch; }
    }

    public float AngleOfAttackYaw
    {
        get { return angleOfAttackYaw; }
    }

    public Vector3 RelativeVelocity
    {
        get { return relativeVelocity; }
    }

    public float LiftMultiplier
    {
        get { return liftMultiplier; }
        set { liftMultiplier = value; }
    }

    void Awake()
    {
        atmosphere = Atmosphere.Instance;
        surfaceArea = (wingDimensions.x * wingDimensions.y) / 2f;

        // Find the stall angle for this wing, based on the defined lift coefficients
        for (float i = 0f; i < 90f; i += 1f)
        {
            if (cLiftCurve.Evaluate(i) > critPitchAlpha)
                critPitchAlpha = i;
        }
    }

    void Update()
    {
        /* Draw debug lines */

        // Change color of lift vector to red as the wing stalls.
        Color liftColor = Color.Lerp(Color.green, Color.yellow, Mathf.Clamp(angleOfAttackPitch - critPitchAlpha, 0f, 90f) * 0.1f);

        if (GameManager.Instance.ShowDebug)
        {
            Vector3 position = transform.position + transform.TransformDirection(centerOfPressure);

            Debug.DrawLine(position, position + (relativeVelocity * DebugVelocityScale), Color.blue);
            Debug.DrawLine(position, position + (liftVector / GetComponent<Rigidbody>().mass * DebugForceScale), liftColor);
            Debug.DrawLine(position, position + (dragVector / GetComponent<Rigidbody>().mass * DebugForceScale), Color.red);
            Debug.DrawLine(position, position + (sideSlip / GetComponent<Rigidbody>().mass * DebugForceScale), Color.red);
        }
    }

    void FixedUpdate()
    {
        /* Determine relative wind. */
        Vector3 windDirection = atmosphere.WindVelocityAt(transform.position);
        relativeVelocity = GetComponent<Rigidbody>().velocity - windDirection;
        trueAirspeed = relativeVelocity.magnitude;

        /* Determine angle of attack. (keep in mind that forward direction is the Up/Y axis, not Z) */
        Vector3 forward = transform.TransformDirection(Vector3.up);
        Vector3 right = transform.TransformDirection(Vector3.right);
        Vector3 up = transform.TransformDirection(Vector3.down);
        Vector3 horizontalRight = new Vector3(right.x, 0f, right.z);

        angleOfAttackPitch = Mathx.AngleAroundAxis(forward, relativeVelocity, right);
        angleOfAttackYaw = Mathx.AngleAroundAxis(right, relativeVelocity, up);

        // Angle of attack falls off to 0 based on the amount of sideslip (not being aligned to direction of flight)
        float alphaSideFalloff = Mathx.ClampAngle(angleOfAttackYaw + 90f, -360f, 360f, 180f);
        alphaSideFalloff = sideFalloffCurve.Evaluate(alphaSideFalloff);

        float airDensity = atmosphere.AirDensityAt(transform.position);

        /* Determine lift around pitch axis based on angle of attack, using coefficient lookup. */
        cLift = cLiftCurve.Evaluate(angleOfAttackPitch) * alphaSideFalloff;
        lift = .5f * airDensity * Mathf.Pow(trueAirspeed, 2f) * surfaceArea * cLift * liftMultiplier;

        // Determine lift vector as perpendicular to the relative airflow
        liftVector = Vector3.Cross(relativeVelocity, right).normalized * lift;

        /* Determine profile drag around the pitch axis. */
        float cDrag = cDragCurve.Evaluate(angleOfAttackPitch);
        drag = .5f * airDensity * trueAirspeed * trueAirspeed * surfaceArea * cDrag;
        dragVector = -relativeVelocity.normalized * drag;

        /* Add some simple lateral drag to minimize sideslip, convert a portion of it to forward speed like in Wipeout */
        sideSlip = Vector3.Project(relativeVelocity, horizontalRight);
        Vector3 sideSlipNegation = -sideSlip * 25f;
        sideSlipNegation += forward * sideSlipNegation.magnitude * 0.65f;

        /* Apply the forces. */
        GetComponent<Rigidbody>().AddForceAtPosition(liftVector + dragVector + sideSlipNegation, transform.TransformPoint(centerOfPressure));
    }
}
