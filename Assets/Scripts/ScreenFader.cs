using UnityEngine;

/*
 * Todo:
 * - Separate timing values for fade in and out
 */
public class ScreenFader : MonoBehaviour
{
    [SerializeField]
	float fadeTime = 1f;
    [SerializeField]
    Color fadeColor = Color.black;
    [SerializeField]
    Texture baseTexture;
    [SerializeField]
    bool startOpage = false;

    public event FadeInDoneHandler OnFadeInDone;
    public event FadeOutDoneHandler OnFadeOutDone;

	Rect screenRect;
	FadeMode mode = FadeMode.Idle;
	float timer = 0f;
	
	void Awake()
	{
		screenRect = new Rect(0f, 0f, Screen.width, Screen.height);
		if (baseTexture == null)
            Debug.LogError("No texture assigned to ScreenFade");
		if (!startOpage)
			fadeColor.a = 0f;
	}
	
	public void FadeIn()
	{
		mode = FadeMode.In;
		timer = 0f;
		fadeColor.a = 1f;
	}
	
	public void FadeOut()
	{
		mode = FadeMode.Out;
		timer = 0f;
		fadeColor.a = 0f;
	}
	
	void Update()
	{
		// for testing
		//if (Input.GetKeyDown(KeyCode.T)) FadeOut();
		//if (Input.GetKeyDown(KeyCode.Y)) FadeIn();
		
		float alpha = fadeColor.a;

		switch (mode) {
		case FadeMode.In:
			UpdateTimer();
			alpha -= (1f/fadeTime) * Time.deltaTime;
			break;
		case FadeMode.Out:
			UpdateTimer();
			alpha += (1f/fadeTime) * Time.deltaTime;
			break;
		case FadeMode.Idle:
			break;
		}

		fadeColor.a = alpha;
	}
	
	void OnGUI()
	{
		GUI.color = fadeColor;
		GUI.DrawTexture(screenRect, baseTexture);
	}
	
	void UpdateTimer()
	{
		timer += Time.deltaTime;
		if (timer >= fadeTime) {
			switch (mode) {
			case FadeMode.In:
                Reset();
                if (OnFadeInDone != null)
                    OnFadeInDone();
				break;
			case FadeMode.Out:
                Reset();
                if (OnFadeOutDone != null)
                    OnFadeOutDone();
				break;
			}
		}
	}

    void Reset()
    {
        // Reset to idle mode
        mode = FadeMode.Idle;
        timer = 0f;
    }
	
	enum FadeMode {
		In,
		Out,
		Idle
	}

    public delegate void FadeInDoneHandler();
    public delegate void FadeOutDoneHandler();
}
