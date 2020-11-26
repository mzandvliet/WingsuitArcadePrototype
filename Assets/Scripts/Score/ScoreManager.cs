using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    static ScoreManager instance;
    Controller playerController;

    float score;
    float multiplier;
    float previousMultiplier;
    float previousScore;
    
    [SerializeField]
    float maxMultiplier = 16f;
    [SerializeField]
    float multiplierStartHeight = 128f;
    [SerializeField]
    float multiplierSmoothTime = 1f;

    public static ScoreManager Instance
    {
        get
        {
            if (instance == null)
                instance = FindObjectOfType(typeof(ScoreManager)) as ScoreManager;
            if (instance == null)
                Debug.LogError("No instance of ScoreManager found in scene");
            return instance;
        }
    }

    public int Score
    {
        get { return Mathf.RoundToInt(score); }
    }

    public int PreviousScore
    {
        get { return Mathf.RoundToInt(previousScore); }
    }

    public float Multiplier
    {
        get { return multiplier; }
    }

    public float MaxMultiplier
    {
        get { return maxMultiplier; }
    }

    void Awake()
    {
        GameManager.Instance.OnPlayerSpawn += OnPlayerSpawn;
    }

    void Update()
    {
        float currentMultiplier = 1f + (1f - Mathf.Clamp01((playerController.Altitude + 2f) / multiplierStartHeight)) * (maxMultiplier - 1f);
        multiplier = Mathf.Lerp(previousMultiplier, currentMultiplier, multiplierSmoothTime * Time.deltaTime);
        previousMultiplier = multiplier;

        float angularSpeed = playerController.LocalAngularVelocity.magnitude;
        if (playerController.InputStunt && angularSpeed > 4f)
            score += (angularSpeed - 4f) * Time.deltaTime * multiplier;
    }

    public void AddScore(float amount)
    {
        score += amount * multiplier;
    }

    public void SetMultiplier(float amount)
    {
        multiplier = Mathf.Clamp(amount, 0f, maxMultiplier);
        previousMultiplier = multiplier;
    }

    void OnPlayerSpawn(GameObject player)
    {
        playerController = player.GetComponent<Controller>();

        previousScore = score;
        score = 0f;
        multiplier = 1f;
        previousMultiplier = 1f;
    }
}