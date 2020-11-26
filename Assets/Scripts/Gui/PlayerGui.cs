using UnityEngine;

public class PlayerGui : MonoBehaviour
{
    GameObject player;
    Wing playerWing;

    [SerializeField]
    GUISkin guiSkin;
    [SerializeField]
    Texture2D multiplier_inner;
    [SerializeField]
    Texture2D multiplier_middle;
    [SerializeField]
    Texture2D multiplier_outer;

    void Awake()
    {
        GameManager.Instance.OnPlayerSpawn += OnPlayerSpawn;
    }

    // Todo: Separate this out into a dedicated game gui class that reads public interfaces or receives events
    void OnGUI()
    {
        if (player == null)
            return;

        if (guiSkin)
            GUI.skin = guiSkin;

        float multiLerp = (ScoreManager.Instance.Multiplier) / ScoreManager.Instance.MaxMultiplier; // todo: take these values from scoremanager as well
        GUI.color = Color.Lerp(Color.white, Color.green, Mathf.Clamp01(multiLerp * 3f));
        GUI.DrawTexture(new Rect(Screen.width * 0.5f - 17.5f, 0f, 35f, 18f), multiplier_inner);
        GUI.color = Color.Lerp(Color.white, Color.yellow, Mathf.Clamp01(Mathf.Clamp01(multiLerp - .33333f) * 4f));
        GUI.DrawTexture(new Rect(Screen.width * 0.5f - 34.5f, 0f, 69f, 35f), multiplier_middle);
        GUI.color = Color.Lerp(Color.white, Color.red, Mathf.Clamp01(Mathf.Clamp01(multiLerp - .66666f) * 5f));
        GUI.DrawTexture(new Rect(Screen.width * 0.5f - 47.0f, 0f, 94f, 47f), multiplier_outer);

        GUI.color = Color.white;

        GUI.BeginGroup(new Rect((float)Screen.width - 374f, (float)Screen.height * 0.75f, 374f, 35f), guiSkin.GetStyle("ScorePanel"));
        {
            GUI.contentColor = Color.white;
            GUI.Label(new Rect(16f, 7f, 120f, 35f), string.Format("{0:d6}", ScoreManager.Instance.Score));
            GUI.Label(new Rect(220f, 7f, 120f, 35f), "Score");
        }
        GUI.EndGroup();

        GUI.BeginGroup(new Rect(0, (float)Screen.height * 0.75f, 351f, 35f), guiSkin.GetStyle("GlidePanel"));
        {
            GUI.contentColor = Color.Lerp(Color.green, Color.red, (Mathf.Abs(playerWing.AngleOfAttackPitch) - 14f) / 50f);
            GUI.Label(new Rect(114f, 7f, 120f, 35f), "Glide");
            GUI.contentColor = Color.white;
            GUI.Label(new Rect(265f, 7f, 120f, 35f), string.Format("{0:f1}", playerWing.AngleOfAttackPitch));
        }
        GUI.EndGroup();
    }

    void OnPlayerSpawn(GameObject player)
    {
        this.player = player;
        playerWing = player.GetComponent<Wing>();
    }
}