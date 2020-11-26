using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/*
 * Bugs:
 * - Dual ragdoll/player spawn
 * 
 * Todo:
 * - Find a better way to handle global stuff between states
 */

public class GameManager : MonoBehaviour
{
    private static GameManager instance;

    private GameObject currentPlayer;
    private Controller playerController;
    private Wing playerWing;
    private Rigidbody playerRigidbody;
    private Animation playerAnimation;
    private AnimationController playerAnimationController;
    private CameraController playerCameraController;
    private CrashCamera playerCrashCamera;
    private ScreenFader playerScreenFader;

    private GameState currentState;
    private Dictionary<GameStateID, GameState> gameStates;

    private Collision crashInfo;

    [SerializeField]
    private bool lockCursor = false;
    [SerializeField]
    private bool showDebug = true;
    [SerializeField]
    private GameObject playerPrefab;
    [SerializeField]
    private GameObject playerCamera;

    [SerializeField]
    private GameObject ragdollPrefab;

    public delegate void PlayerSpawnHandler(GameObject player);
    public event PlayerSpawnHandler OnPlayerSpawn;

    public static GameManager Instance {
        get {
            if (!instance) instance = FindObjectOfType(typeof(GameManager)) as GameManager;
            if(!instance) Debug.LogError("There is no GameInfo in the scene!");
            return instance;
        }
    }

    public GameObject Player {
        get { return currentPlayer; }
    }

    public Controller PlayerController
    {
        get { return playerController; }
    }

    public Wing PlayerWing
    {
        get { return playerWing; }
    }

    public Rigidbody PlayerRigidbody
    {
        get { return playerRigidbody; }
    }

    public Animation PlayerAnimation
    {
        get { return playerAnimation; }
    }

    public AnimationController PlayerAnimationController
    {
        get { return playerAnimationController; }
    }

    public GameObject PlayerCamera
    {
        get { return playerCamera; }
    }

    public CameraController PlayerCameraController
    {
        get { return playerCameraController; }
    }

    public CrashCamera PlayerCrashCamera
    {
        get { return playerCrashCamera; }
    }

    public ScreenFader PlayerScreenFader
    {
        get { return playerScreenFader; }
    }

    public bool ShowDebug {
        get { return showDebug; }
        set { showDebug = value; }
    }

    void Awake()
    {
        gameStates = new Dictionary<GameStateID, GameState>
            {
                {GameStateID.Intro, new IntroState()},
                {GameStateID.Start, new StartState()},
                {GameStateID.Flying, new FlyingState()},
                {GameStateID.Finished, new FinishedState()},
                {GameStateID.Crashed, new CrashedState()}
            };

        DontDestroyOnLoad(gameObject);
        Screen.lockCursor = lockCursor;
    }

    void Start()
    {
        ChangeState(GameStateID.Start);
    }

    void Update()
    {
        currentState.Update();
    }

    public void ChangeState(GameStateID newState)
    {
        if (currentState != null)
            currentState.OnExit();

        currentState = gameStates[newState];
        currentState.OnEnter();
    }

    internal GameObject SpawnPlayer()
    {
        Debug.Log("Spawning!");
        Transform spawn = GameObject.Find("PlayerSpawn").transform;
        currentPlayer = Instantiate(playerPrefab, spawn.position, spawn.rotation) as GameObject;
        InitPlayer();

        if (OnPlayerSpawn != null)
            OnPlayerSpawn(currentPlayer);

        return currentPlayer;
    }

    internal void InitPlayer()
    {
        playerController = currentPlayer.GetComponent<Controller>();
        playerWing = currentPlayer.GetComponent<Wing>();
        playerRigidbody = currentPlayer.GetComponent<Rigidbody>();
        playerAnimation = currentPlayer.GetComponentInChildren<Animation>();
        playerAnimationController = currentPlayer.GetComponentInChildren<AnimationController>();
        playerCameraController = playerCamera.GetComponent<CameraController>();
        playerCrashCamera = playerCamera.GetComponent<CrashCamera>();
        playerScreenFader = playerCamera.GetComponent<ScreenFader>();
    }

    internal GameObject SpawnRagdoll(Vector3 velocity)
    {
        // Spawn ragdoll
        GameObject newRagdoll = Instantiate(ragdollPrefab, currentPlayer.transform.position, currentPlayer.transform.rotation) as GameObject;
        
        // Give each ragdoll part an initial velocity
        if (newRagdoll != null)
        {
            Rigidbody[] ragdollBodies = newRagdoll.GetComponentsInChildren<Rigidbody>();

            foreach (Rigidbody body in ragdollBodies)
                body.AddForce(velocity + Random.insideUnitSphere, ForceMode.VelocityChange);
        }

        return newRagdoll;
    }

    public enum GameStateID
    {
        Intro,
        Start,
        Flying,
        Crashed,
        Finished
    }

    public abstract class GameState
    {
        readonly GameStateID stateID;
        State myState = State.Inactive;
        static GameManager g;
        static bool initialized = false;

        public GameStateID StateID
        {
            get { return stateID; }
        }

        internal State MyState
        {
            get { return myState; }
        }

        internal GameManager Game
        {
            get { return g; }
        }

        public GameState(GameStateID stateID)
        {
            this.stateID = stateID;
            if (!initialized)
                g = Instance;
        }

        public virtual void Update()
        {
        }

        public virtual void OnEnter()
        {
            myState = State.Entering;
        }

        public virtual void OnExit()
        {
            myState = State.Exiting;
        }

        internal enum State
        {
            Inactive,
            Entering,
            Updating,
            Exiting
        }
    }

    class IntroState : GameState
    {
        public IntroState() : base(GameStateID.Intro) { }
    }

    class StartState : GameState
    {
        public StartState() : base(GameStateID.Start)
        {

        }

        public override void Update()
        {
            base.Update();

            if (Input.GetButtonDown("Jump"))
            {
                Game.PlayerAnimation.Play("startjump");
                Game.StartCoroutine(WaitForJumpAnimation());
            }
        }

        public override void OnEnter()
        {
            base.OnEnter();

            Debug.Log("Entering start state");

            Game.SpawnPlayer();

            Game.PlayerController.enabled = false;
            Game.PlayerWing.enabled = false;
            Game.PlayerRigidbody.isKinematic = true;
            Game.PlayerAnimationController.enabled = false;
            Game.PlayerCameraController.enabled = false;
            Game.PlayerCrashCamera.enabled = false;

            Game.PlayerAnimation.Play("startidle");

            // Player camera's introduction animation
            Game.PlayerCameraController.Player = Game.currentPlayer;
            Game.PlayerCamera.GetComponent<Animation>().Play("cameraIdle");

            Game.playerScreenFader.FadeIn();
        }

        public override void OnExit()
        {
            base.OnExit();

            Debug.Log("Exiting start state");

            Game.PlayerController.enabled = true;
            Game.PlayerWing.enabled = true;
            Game.PlayerRigidbody.isKinematic = false;
            Game.PlayerAnimation.Stop("startjump");
            Game.PlayerAnimationController.enabled = true;
            Game.PlayerCameraController.enabled = true;
            
            Transform spawn = GameObject.Find("PlayerSpawn").transform;
            Vector3 initialVelocity = (spawn.TransformDirection(Vector3.up) + spawn.TransformDirection(Vector3.forward)) * 20f;
            Game.PlayerRigidbody.AddForce(initialVelocity, ForceMode.VelocityChange);

            Game.PlayerCamera.GetComponent<Animation>().Stop();
        }

        IEnumerator WaitForJumpAnimation()
        {
            yield return new WaitForSeconds(.9f);
            Game.ChangeState(GameStateID.Flying);
        }
    }

    class FlyingState : GameState
    {
        GoalZone goalZone;

        public FlyingState()
            : base(GameStateID.Flying)
        {
            goalZone = FindObjectOfType(typeof(GoalZone)) as GoalZone;
        }

        public override void OnEnter()
        {
            base.OnEnter();

            Debug.Log("Entering flying state");

            Game.PlayerController.OnPlayerCollision += OnPlayerCollision;
            goalZone.OnPlayerFinished += OnPlayerFinished;
        }

        public override void Update()
        {
            base.Update();

            if (Input.GetButtonDown("Reset"))
                Game.ChangeState(GameStateID.Start);

            if (Input.GetButtonDown("Quit"))
                Application.Quit();

        }

        public override void OnExit()
        {
            base.OnExit();

            Debug.Log("Exiting flying state");

            Game.PlayerController.OnPlayerCollision -= OnPlayerCollision;
            goalZone.OnPlayerFinished -= OnPlayerFinished;
        }

        void OnPlayerCollision(Collision collision)
        {
            Game.crashInfo = collision;
            Game.ChangeState(GameStateID.Crashed);
        }

        void OnPlayerFinished()
        {
            if (MyState == State.Exiting)
                return;

            Game.ChangeState(GameStateID.Finished);
        }
    }

    class CrashedState : GameState
    {
        public CrashedState()
            : base(GameStateID.Crashed)
        { 
        }

        public override void OnEnter()
        {
            base.OnEnter();

            Game.PlayerCameraController.enabled = false;

            Game.Player.SetActive(false);

            Collision collision = Game.crashInfo;

            Vector3 velocity = -collision.relativeVelocity;
            Vector3 velocityOnNormal = Vector3.Project(velocity, collision.contacts[0].normal);
            Vector3 initialVelocity = (velocity - velocityOnNormal * 1.1f) * 0.5f;
            GameObject newRagdoll = Game.SpawnRagdoll(initialVelocity);

            Game.PlayerCrashCamera.Init(newRagdoll.transform);
            Game.PlayerCrashCamera.enabled = true;

            Game.PlayerScreenFader.OnFadeOutDone += OnFadeOutDone;
            Game.PlayerScreenFader.FadeOut();
        }

        void OnFadeOutDone()
        {
            Destroy(Game.Player);

            Game.PlayerScreenFader.OnFadeOutDone -= OnFadeOutDone;
            Game.ChangeState(GameStateID.Start);
        }
    }

    class FinishedState : GameState
    {
        public FinishedState()
            : base(GameStateID.Finished)
        {

        }

        public override void OnEnter()
        {
            base.OnEnter();

            Game.PlayerCameraController.enabled = false;

            Game.PlayerCrashCamera.Player = Game.Player.transform;
            Game.PlayerCrashCamera.enabled = true;

            Game.PlayerScreenFader.OnFadeOutDone += OnFadeOutDone;
            Game.PlayerScreenFader.FadeOut();
        }
        void OnFadeOutDone()
        {
            Destroy(Game.Player);

            Game.PlayerScreenFader.OnFadeOutDone -= OnFadeOutDone;
            Game.ChangeState(GameStateID.Start);
        }
    }
}