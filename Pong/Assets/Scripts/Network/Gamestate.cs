using UnityEngine;
using FishNet.Object;
using FishNet.Object.Synchronizing;

public class GameState : NetworkBehaviour
{
    public static GameState Instance;

    public readonly SyncVar<int> LeftScore = new SyncVar<int>(0);
    public readonly SyncVar<int> RightScore = new SyncVar<int>(0);
    public readonly SyncVar<int> ServingSide = new SyncVar<int>(0);

    private const float ServeDelay = 1.5f;

    private float serveDelayTimer = ServeDelay;
    private GUIStyle scoreStyle;

    private void Awake()
    {
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    private void Update()
    {
        if (!IsServerInitialized) return;
        if (NetworkBall.Instance == null) return;
        if (NetworkBall.Instance.InPlay.Value) return;

        int activePlayers = 0;
        foreach (PaddleController p in PaddleController.All)
        {
            if (p != null && p.Side.Value <= 1) activePlayers++;
        }

        if (activePlayers >= 2)
        {
            serveDelayTimer -= Time.deltaTime;
            if (serveDelayTimer <= 0f)
            {
                serveDelayTimer = ServeDelay;
                NetworkBall.Instance.Serve(ServingSide.Value);
            }
        }
        else
        {
            serveDelayTimer = ServeDelay;
        }
    }

    public void Goal(int scorerSide)
    {
        if (!IsServerInitialized) return;

        if (scorerSide == 0) LeftScore.Value++;
        else if (scorerSide == 1) RightScore.Value++;

        ServingSide.Value = 1 - scorerSide;
        serveDelayTimer = ServeDelay;
    }

    private void OnGUI()
    {
        if (scoreStyle == null)
        {
            scoreStyle = new GUIStyle(GUI.skin.box)
            {
                fontSize = 26,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            scoreStyle.normal.textColor = Color.white;
        }

        float width = 160f;
        float height = 50f;
        float x = (Screen.width - width) / 2f;
        float y = 20f;

        Color previousBackground = GUI.backgroundColor;
        GUI.backgroundColor = new Color(0f, 0f, 0f, 0.6f);
        GUI.Box(new Rect(x, y, width, height), $"{LeftScore.Value} : {RightScore.Value}", scoreStyle);
        GUI.backgroundColor = previousBackground;
    }
}