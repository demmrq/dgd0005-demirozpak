using System;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

public class GameState : NetworkBehaviour
{
    public static GameState Instance;

    public readonly SyncVar<int> LeftScore = new SyncVar<int>(0);
    public readonly SyncVar<int> RightScore = new SyncVar<int>(0);
    public readonly SyncVar<int> ServingSide = new SyncVar<int>(0);

    private const float ServeDelay = 1.5f;

    private float serveDelayTimer = ServeDelay;
    private Func<string> panel;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        panel = () => $"{LeftScore.Value} : {RightScore.Value}";
        ConnectionHud.Panels.Add(panel);
    }

    private void OnDestroy()
    {
        if (panel != null)
            ConnectionHud.Panels.Remove(panel);

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
}