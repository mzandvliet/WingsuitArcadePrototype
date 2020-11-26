using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(Wing))]
public class Controller : MonoBehaviour
{
    Wing wing;

    [SerializeField]
    bool invertPitch = true;
    [SerializeField]
    bool invertRoll = true;
    [SerializeField]
    bool quadraticInputScaling = false;

    [SerializeField]
    float inputPitchPower = 0.2f;
    [SerializeField]
    float inputRollPower = 0.2f;
    [SerializeField]
    float inputYawPower = 0.1f;

    [SerializeField]
    float trickPitchPower = 0.35f;
    [SerializeField]
    float trickRollPower = 0.35f;

    [SerializeField]
    float stabilityPitchPower = 0.2f;
    [SerializeField]
    float stabilityRollPower = 0.2f;
    [SerializeField]
    float stabilityYawPower = 0.1f;

    [SerializeField]
    float stabilityFactor = .5f;

    [SerializeField]
    float liftFalloffStart = 200f;
    [SerializeField]
    float liftFalloffEnd = 300f;

    [SerializeField]
    float groundEffectStart = 6f;
    [SerializeField]
    float maxGroundEffect = .5f;

    [SerializeField]
    float maxDownPitch = 40f;
    [SerializeField]
    float maxUpPitch = 50f;
    [SerializeField]
    float pitchFalloff = 20f;

    float inputPitch;
    float inputRoll;
    bool inputStunt;

    float altitude;
    float levelBaseAltitude;
    Vector3 acceleration;
    float glideRatio;
    Vector3 localAngularVelocity;

    int playerLayerMask;
    int levelBaseLayerMask;
    Vector3 previousVelocity;

    bool inFlight;

    public event PlayerCollisionHandler OnPlayerCollision;

    public bool InputStunt
    {
        get { return inputStunt; }
    }

    public float Altitude
    {
        get { return altitude; }
    }

    public float LevelBaseAltitude
    {
        get { return levelBaseAltitude; }
    }

    // Warning! Numerically unstable.
    public Vector3 Acceleration
    {
        get { return acceleration; }
    }

    public float GlideRatio
    {
        get { return glideRatio; }
    }

    public Vector3 LocalAngularVelocity
    {
        get { return localAngularVelocity; }
    }

    void Start()
    {
        wing = GetComponent<Wing>();
        playerLayerMask = ~(1 << LayerMask.NameToLayer("Player"));
        levelBaseLayerMask = (1 << LayerMask.NameToLayer("AltitudeBase"));
    }

    void Update()
    {
        inputPitch = Input.GetAxis("Vertical") * (invertPitch ? -1f : 1f);
        inputRoll = Input.GetAxis("Horizontal") * (invertRoll ? -1f : 1f);

        // Scale input quadratically
        if (quadraticInputScaling)
        {
            inputPitch *= Mathf.Abs(inputPitch);
            inputRoll *= Mathf.Abs(inputRoll);
        }

        acceleration = (GetComponent<Rigidbody>().velocity - previousVelocity) / Time.deltaTime; // Warning! Numerically unstable.
        previousVelocity = GetComponent<Rigidbody>().velocity;
    }

    void FixedUpdate()
    {
        float inputPitchStrength = Mathf.Abs(inputPitch);
        float inputRollStrength = Mathf.Abs(inputRoll);

        /* Determine glide ratio. */
        float horizontalDistance = Mathf.Sqrt(Mathf.Pow(GetComponent<Rigidbody>().velocity.x, 2f) + Mathf.Pow(GetComponent<Rigidbody>().velocity.z, 2f));
        float verticalDistance = -GetComponent<Rigidbody>().velocity.y;
        glideRatio = horizontalDistance / verticalDistance;

        /* Determine altitude to terrain. */
        RaycastHit hitInfo;
        bool hit = Physics.Raycast(transform.position, Vector3.down, out hitInfo, 2000f, playerLayerMask);
        if (hit)
            altitude = hitInfo.distance;

        /* Determine altitude to base of the level. (Defined by colliders on a separate layer) */
        hit = Physics.Raycast(transform.position, Vector3.down, out hitInfo, 2000f, levelBaseLayerMask);
        if (hit)
            levelBaseAltitude = hitInfo.distance;

        if (!inFlight)
        {
            if (levelBaseAltitude < liftFalloffStart)
                inFlight = true;
        }

        /* Manipulate the lift multiplier of the wing to change flight behaviour based on altitude */

        float liftFactor = 1f;
        
        // Remove lift amount above a certain altitude above the base of the level
        if (inFlight)
            liftFactor -= Mathf.Clamp01((levelBaseAltitude - liftFalloffStart) / (liftFalloffEnd - liftFalloffStart));
        
        // Boost lift when very close to the ground
        liftFactor += (1f - Mathf.Clamp01(altitude / groundEffectStart)) * maxGroundEffect;
        wing.LiftMultiplier = liftFactor;

        /* Determine steering torques, causing the player to rotate. Consists of several parts:
         *
         * Navigation
         * 
         * - Player input (directly affect heading, enabling navigation)
         * 
         * Balance / Fly By Wire
         * 
         * - Gyroscope sensors (find unwanted rotation and try to dampen or cancel it out)
         * - Heading sensors (attempt to align to flight direction)
         * 
         * Player input takes precedence over balacing influences.
         */

        inputStunt = Input.GetButton("Jump");

        float pitchTorque = 0f;
        float rollTorque = 0f;
        float yawTorque = 0f;

        Vector3 forward = transform.TransformDirection(Vector3.up);
        Vector3 right = transform.TransformDirection(Vector3.right);
        Vector3 down = transform.TransformDirection(Vector3.forward);
        Vector3 horizontalForward = new Vector3(forward.x, 0f, forward.z).normalized;

        /* For heading stability */

        // Determine Angle to ground around local axes.
        float bellyToGroundRollAngle = Mathx.AngleAroundAxis(Vector3.down, down, forward);
        float bellyToGroundPitchAngle = Mathx.AngleAroundAxis(horizontalForward, forward, right);

        // Determine angles to relative velocity
        float noseToIdlePitchAngle = (8f - bellyToGroundPitchAngle);
        float noseToVelocityPitchAngle = Mathx.AngleAroundAxis(wing.RelativeVelocity, forward, right);
        float noseToVelocityYawAngle = Mathx.AngleAroundAxis(wing.RelativeVelocity, forward, down);

        /* For rotation stability */

        localAngularVelocity = transform.InverseTransformDirection(GetComponent<Rigidbody>().angularVelocity);

        // If we're not in stunt mode, apply balancing torques to maintain stable flight
        if (!inputStunt)
        {
            const float limitDampeningFactor = .3f;

            // Limit roll angles
            float maxRollAngle = 50f;
            float rollFalloff = 30f;

            // Limit pitch angles
            float rollDampening = Mathf.Clamp(Mathf.Abs(bellyToGroundRollAngle) - maxRollAngle, 0f, rollFalloff) * (1f/rollFalloff);

            if (bellyToGroundRollAngle > maxRollAngle && localAngularVelocity.y > 0f)
            {
                rollTorque += -localAngularVelocity.y * rollDampening * limitDampeningFactor;
                inputRoll *= 1f - rollDampening;
            }
            if (bellyToGroundRollAngle < -maxRollAngle && localAngularVelocity.y < 0f)
            {
                rollTorque += -localAngularVelocity.y * rollDampening * limitDampeningFactor;
                inputRoll *= 1f - rollDampening;
            }

            float pitchDampeningDown = Mathf.Clamp(bellyToGroundPitchAngle - maxDownPitch, 0f, pitchFalloff) * (1f / pitchFalloff);
            float pitchDampeningUp = Mathf.Clamp(noseToVelocityPitchAngle + maxUpPitch, -pitchFalloff, 0f) * (-1f / pitchFalloff);

            if (bellyToGroundPitchAngle > maxDownPitch && localAngularVelocity.x > 0f)
            {
                pitchTorque += -localAngularVelocity.x * pitchDampeningDown * limitDampeningFactor;
                inputPitch *= 1f - pitchDampeningDown;
            }
            if (noseToVelocityPitchAngle < maxUpPitch && localAngularVelocity.x < 0f)
            {
                pitchTorque += -localAngularVelocity.x * pitchDampeningUp * limitDampeningFactor;
                inputPitch *= 1f - pitchDampeningDown;
            }
        }
        // If we are in stunt mode we just allow free rotation

        // User input
        pitchTorque += inputPitch * (inputStunt ? trickPitchPower : inputPitchPower);
        rollTorque += inputRoll * (inputStunt ? trickRollPower : inputRollPower);
        if (!inputStunt)
        {
            yawTorque += -inputRoll * inputYawPower * 0.5f;
            yawTorque += -bellyToGroundRollAngle * Mathx.ONEOVER180 * inputYawPower * 0.5f;
        }

        // Rotate towards relative velocity
        if (!inputStunt)
        {
            pitchTorque += noseToIdlePitchAngle * Mathx.ONEOVER180 * (1f - inputPitchStrength * 1f) * stabilityPitchPower;
            rollTorque += -bellyToGroundRollAngle * Mathx.ONEOVER180 * stabilityRollPower * (1f - inputRollStrength * 1f);
            yawTorque += noseToVelocityYawAngle * Mathx.ONEOVER180 * (1f - inputRollStrength * 1f) * stabilityYawPower;
        }

        float pitchStabilityInputScale = (1f - Mathf.Clamp(inputPitchStrength * 1f, 0f, 1f)) * stabilityFactor;
        float angVelPitch = localAngularVelocity.x;
        if (Mathf.Abs(angVelPitch) > 0f)
            pitchTorque += -angVelPitch * stabilityPitchPower * pitchStabilityInputScale;

        float rollStabilityInputScale = (1f - Mathf.Clamp(inputRollStrength * 1f, 0f, 1f)) * stabilityFactor;
        float angVelRoll = localAngularVelocity.y;
        if (Mathf.Abs(angVelRoll) > 0f)
            rollTorque += -angVelRoll * stabilityRollPower * rollStabilityInputScale;

        float yawStabilityInputScale = (1f - Mathf.Clamp(inputRollStrength * 1, 0f, 1f)) * stabilityFactor;
        float angVelYaw = GetComponent<Rigidbody>().angularVelocity.y;
        if (Mathf.Abs(angVelYaw) > 0f)
            yawTorque += -angVelYaw * stabilityYawPower * yawStabilityInputScale;

        /* The yaw and roll torques create a downward pitch moment which can be annoying. This cancels it out a bit. */
        if (!inputStunt && pitchTorque > 0f)
            pitchTorque += (-Mathf.Abs(rollTorque) - Mathf.Abs(yawTorque)) * (1f - inputPitchStrength);

        /* Apply the torques! */

        // Apply yaw in world space
        GetComponent<Rigidbody>().AddTorque(new Vector3(0f, yawTorque, 0f), ForceMode.VelocityChange);
        // Apply pitch and roll in local space
        GetComponent<Rigidbody>().AddRelativeTorque(new Vector3(pitchTorque, rollTorque, 0f), ForceMode.VelocityChange);
    }

    void OnCollisionEnter(Collision collisionInfo)
    {
        if (OnPlayerCollision != null)
            OnPlayerCollision(collisionInfo);
    }

    public delegate void PlayerCollisionHandler(Collision collisionInfo);
}