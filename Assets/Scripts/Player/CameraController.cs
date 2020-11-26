using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour
{
    [SerializeField]
    GameObject player;
    Wing wing;
    Controller controller;

    [SerializeField]
    private float _orbitDistance = 2.5f;

    [SerializeField] private float _positionSwayMultiplier = 0.25f;

    [SerializeField]
    private Vector3 _defaultRotationOffset = new Vector3(15f, 0f, 0f);
    [SerializeField]
    private float _rotationSpeed = 1.6f;

    [SerializeField]
    private bool _enableShake = true;
    [SerializeField]
    private float _shakeMultiplier = 1f;

    private Quaternion _baseOffset;

    private Vector3 _smoothAcceleration;

    public GameObject Player
    {
        get { return player; }
        set { player = value; }
    }

    void Awake()
    {
        GameManager.Instance.OnPlayerSpawn += OnPlayerSpawn;
        _baseOffset = Quaternion.Euler(_defaultRotationOffset);
    }

    private void Update()
    {
        // Camera orbits around player, looks at player and along flight direction, and sways based on player acceleration
        Vector3 playerDirection = (player.transform.position - transform.position).normalized;
        Vector3 flightDirection = wing.RelativeVelocity.normalized;

        Vector3 lookDirection = Vector3.Lerp(playerDirection, flightDirection, 0.5f);
        Quaternion lookRotation = Quaternion.LookRotation(lookDirection);

        Quaternion baseRotation = lookRotation * _baseOffset;
        Quaternion smoothRotation = Quaternion.Lerp(transform.rotation, baseRotation, Time.deltaTime * _rotationSpeed);
        Quaternion targetRotation = smoothRotation;

        // Smooth the acceleration values to compensate for numerical instability
        _smoothAcceleration = Vector3.Lerp(_smoothAcceleration, controller.Acceleration, Time.deltaTime);
        Vector3 positionOffset = _smoothAcceleration * -_positionSwayMultiplier;
        Vector3 basePosition = player.transform.position + positionOffset;
        Vector3 targetPosition = basePosition + targetRotation * new Vector3(0f, 0f, -_orbitDistance);

        // Shake the camera when player is close to the ground or is rotation fast.
        float angularSpeed = controller.LocalAngularVelocity.magnitude;

        Quaternion shakeRotation = Quaternion.identity;
        if (_enableShake)
        {
            float speedScalar = wing.TrueAirspeed / 80f;
            float shakeAmplitude = 0.0004f;
            shakeAmplitude += (80f - Mathf.Clamp(controller.Altitude, 0f, 80f)) * 0.00005f;
            shakeAmplitude += angularSpeed * 0.0009f;
            shakeRotation = Quaternion.Lerp(Quaternion.identity, Random.rotation, _shakeMultiplier * shakeAmplitude * speedScalar);
        }

        // Apply transformations
        transform.rotation = targetRotation * shakeRotation;
        transform.position = targetPosition;
    }

    void OnPlayerSpawn(GameObject player)
    {
        Debug.Log("OnPlayerSpawn");

        this.player = player;
        wing = player.GetComponent<Wing>();
        controller = player.GetComponent<Controller>();
    }
}