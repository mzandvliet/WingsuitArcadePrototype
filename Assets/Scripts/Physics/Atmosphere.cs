using UnityEngine;

/**
 * This class all atmosperic effects that are present within the level. This includes
 * wind, gravity, air pressure, and other things.
 */
public class Atmosphere : MonoBehaviour
{
    private static Atmosphere instance;
    public static Atmosphere Instance
    {
        get
        {
            if (!instance) instance = FindObjectOfType(typeof(Atmosphere)) as Atmosphere;
            if (!instance) Debug.LogError("There is no AtmosphereInfo in the scene!");
            return instance;
        }
    }

    /* Air density in Kg/m3.*/
    [SerializeField]
    float airDensity = 1.293f;

    /* Global air velocity */
    [SerializeField]
    Vector3 windVelocity;
    [SerializeField]
    float turbulenceAmplitude = 1f;
    [SerializeField]
    float turbulenceFrequency = 1f;

    private Perlin perlin;

    public void Awake()
    {
        perlin = new Perlin();
    }

    /**
     * Retrieves the gravity at the given location
     */
    public float GravityAt(Vector3 position)
    {
        return airDensity;
    }

    /**
     * Retrieves the air density at the given location
     */
    public float AirDensityAt(Vector3 position)
    {
        return airDensity;
    }

    /**
     * Retrieves the wind vector at the given location.
     */
    public Vector3 WindVelocityAt(Vector3 position)
    {
        float timeTurbulence = Time.time * turbulenceFrequency;
        float timeX = timeTurbulence * 0.1365143f;
        float timeY = timeTurbulence * 1.21688f;
        float timeZ = timeTurbulence * 2.5564f;

        Vector3 turbulence = new Vector3(perlin.Noise(timeX + position.x, timeX + position.y, timeX + position.z),
                                        perlin.Noise(timeY + position.x, timeY + position.y, timeY + position.z),
                                        perlin.Noise(timeZ + position.x, timeZ + position.y, timeZ + position.z))
                                        * turbulenceAmplitude;

        return windVelocity + turbulence;
    }
}