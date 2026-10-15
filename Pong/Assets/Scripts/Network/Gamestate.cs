using System;
using UnityEngine;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;

public class GameState : NetworkBehaviour
{
    public static GameState Instance;

    public const int MaxFeedLines = 6;

    public readonly SyncVar<int> LeftScore = new SyncVar<int>(0);
    public readonly SyncVar<int> RightScore = new SyncVar<int>(0);
    public readonly SyncVar<int> ServingSide = new SyncVar<int>(0);
    public readonly SyncList<string> Feed = new SyncList<string>();

    private GUIStyle scoreStyle;
    private Func<string> feedPanel;

    public static string SideName(int side)
    {
        if (side == 0) return "LEFT";
        if (side == 1) return "RIGHT";
        return "SPECTATOR";
    }

    public static void Announce(string line)
    {
        if (Instance == null) return;
        if (!Instance.IsServerInitialized) return;
        if (string.IsNullOrEmpty(line)) return;

        Instance.Feed.Add(line);
        while (Instance.Feed.Count > MaxFeedLines)
            Instance.Feed.RemoveAt(0);
    }

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        feedPanel = () => Feed.Count == 0 ? string.Empty : string.Join("\n", Feed);
        ConnectionHud.Panels.Add(feedPanel);
    }

    private void OnDestroy()
    {
        if (feedPanel != null)
            ConnectionHud.Panels.Remove(feedPanel);

        if (Instance == this)
            Instance = null;
    }

    public void Goal(int scorerSide)
    {
        if (!IsServerInitialized) return;

        if (scorerSide == 0) LeftScore.Value++;
        else if (scorerSide == 1) RightScore.Value++;

        ServingSide.Value = 1 - scorerSide;

        Announce($"{SideName(scorerSide)} scored! {LeftScore.Value}:{RightScore.Value}");

        PaddleController scorerPaddle = null;
        PaddleController concederPaddle = null;

        foreach (PaddleController paddle in PaddleController.All)
        {
            if (paddle == null) continue;

            if (paddle.Side.Value == scorerSide)
                scorerPaddle = paddle;
            else if (paddle.Side.Value == ServingSide.Value)
                concederPaddle = paddle;
        }

        if (scorerPaddle != null)
            scorerPaddle.AnnounceGoal(LeftScore.Value, RightScore.Value);

        if (concederPaddle != null)
        {
            NetworkConnection owner = concederPaddle.Owner;
            if (owner != null && owner.IsValid)
                concederPaddle.YouServe(owner);
        }
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