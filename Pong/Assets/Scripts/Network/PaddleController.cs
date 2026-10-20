using System.Collections.Generic;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;
using UnityEngine.InputSystem;

public class PaddleController : NetworkBehaviour
{
    public static readonly List<PaddleController> All = new List<PaddleController>();
    public static PaddleController Local;

    public readonly SyncVar<int> Side = new SyncVar<int>(2);

    [SerializeField] private float speed = 10f;

    private const int Spectator = 2;
    private float _serverAxis;

    private void Awake()
    {
        Side.OnChange += OnSideChanged;
    }

    private void OnDestroy()
    {
        Side.OnChange -= OnSideChanged;
    }

    private void Update()
    {
        if (!IsOwner) return;

        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
            RequestServe();
    }

    public override void OnStartNetwork()
    {
        base.OnStartNetwork();
        TimeManager.OnTick += OnTick;
    }

    public override void OnStopNetwork()
    {
        base.OnStopNetwork();
        TimeManager.OnTick -= OnTick;
    }

    private void OnTick()
    {
        if (IsOwner && Side.Value != Spectator)
        {
            float axis = 0f;
            if (Keyboard.current != null)
            {
                if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) axis += 1f;
                if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) axis -= 1f;
            }
            ServerSetAxis(axis);
        }

        if (IsServerInitialized)
        {
            transform.position += Vector3.forward * _serverAxis * speed * (float)TimeManager.TickDelta;
            Court.ClampPaddle(transform);
        }
    }

    [ServerRpc]
    private void ServerSetAxis(float axis)
    {
        _serverAxis = axis;
    }


    [ServerRpc]
    private void RequestServe()
    {
        if (GameState.Instance == null) return;
        if (NetworkBall.Instance == null) return;
        if (Side.Value > 1) return;
        if (Side.Value != GameState.Instance.ServingSide.Value) return;
        if (NetworkBall.Instance.InPlay.Value) return;

        NetworkBall.Instance.Serve(1 - Side.Value);
        GameState.Announce($"{GameState.SideName(Side.Value)} served");
    }

    [ObserversRpc]
    public void AnnounceGoal(int left, int right)
    {
        ConnectionHud.Flash($"Scored! {left}:{right}");
    }

    [TargetRpc]
    public void YouServe(NetworkConnection target)
    {
        ConnectionHud.Flash("YOU SERVE");
    }

    private float ClampZ(float z)
    {
        float paddleHalfLength = transform.lossyScale.z / 2f;
        float limit = Mathf.Max(0f, Court.HalfHeight - paddleHalfLength);
        return Mathf.Clamp(z, -limit, limit);
    }

    public override void OnStartServer()
    {
        base.OnStartServer();

        if (!All.Contains(this))
            All.Add(this);

        bool leftTaken = false;
        bool rightTaken = false;

        foreach (PaddleController paddle in All)
        {
            if (paddle == null || paddle == this)
                continue;

            if (paddle.Side.Value == 0)
                leftTaken = true;
            else if (paddle.Side.Value == 1)
                rightTaken = true;
        }

        int firstFreeSide = !leftTaken ? 0 : !rightTaken ? 1 : 2;
        Side.Value = firstFreeSide;

        GameState.Announce($"Player {OwnerId} joined as {GameState.SideName(firstFreeSide)}");
    }

    public override void OnStopServer()
    {
        base.OnStopServer();
        All.Remove(this);

        GameState.Announce($"Player {OwnerId} ({GameState.SideName(Side.Value)}) left");
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (!All.Contains(this))
            All.Add(this);

        if (IsOwner)
        {
            Local = this;
            Debug.Log($"I am client {OwnerId}, side {Side.Value}");
        }

        Place();
    }

    public override void OnStopClient()
    {
        base.OnStopClient();
        All.Remove(this);
    }

    private void OnSideChanged(int prev, int next, bool asServer)
    {
        Place();
    }

    private void Place()
    {
        Vector3 position = transform.position;
        if (Side.Value == 0)
            position.x = -Court.PaddleX;
        else if (Side.Value == 1)
            position.x = Court.PaddleX;
        position.z = ClampZ(position.z);
        transform.position = position;

        Renderer rend = GetComponent<Renderer>();
        if (rend != null)
        {
            if (Side.Value == 0)
            {
                rend.enabled = true;
                rend.material.color = Color.blue;
            }
            else if (Side.Value == 1)
            {
                rend.enabled = true;
                rend.material.color = new Color(1f, 0.5f, 0f);
            }
            else
            {
                rend.enabled = false;
            }
        }
    }
}