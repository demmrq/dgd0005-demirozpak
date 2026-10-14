using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

public class NetworkBall : NetworkBehaviour
{
    public static NetworkBall Instance;

    public readonly SyncVar<bool> InPlay = new SyncVar<bool>(false);

    [SerializeField] private float speed = 8f;

    private Vector3 velocity;

    private void Awake()
    {
        Instance = this;
        InPlay.OnChange += OnInPlayChanged;
    }

    private void OnDestroy()
    {
        InPlay.OnChange -= OnInPlayChanged;
        if (Instance == this)
            Instance = null;
    }

    private void Update()
    {
        if (!IsServerInitialized) return;
        if (!InPlay.Value) return;

        transform.position += velocity * speed * Time.deltaTime;

        if (transform.position.z > Court.HalfHeight - Court.BallRadius)
        {
            Vector3 p = transform.position;
            p.z = Court.HalfHeight - Court.BallRadius;
            transform.position = p;
            velocity.z = -velocity.z;
        }
        else if (transform.position.z < -Court.HalfHeight + Court.BallRadius)
        {
            Vector3 p = transform.position;
            p.z = -Court.HalfHeight + Court.BallRadius;
            transform.position = p;
            velocity.z = -velocity.z;
        }

        foreach (PaddleController paddle in PaddleController.All)
        {
            if (paddle == null) continue;
            if (paddle.Side.Value > 1) continue;

            float px = paddle.transform.position.x;
            float pz = paddle.transform.position.z;

            if (Mathf.Abs(transform.position.x - px) < (Court.BallRadius + 0.25f) &&
                Mathf.Abs(transform.position.z - pz) < (Court.PaddleHalfLength + Court.BallRadius))
            {
                float hitOffset = (transform.position.z - pz) / Court.PaddleHalfLength;
                float dirX = paddle.Side.Value == 0 ? 1f : -1f;
                velocity = new Vector3(dirX, 0f, hitOffset).normalized;
                break;
            }
        }

        if (transform.position.x < -Court.HalfWidth)
        {
            ResetToCenter();
            if (GameState.Instance != null) GameState.Instance.Goal(1);
        }
        else if (transform.position.x > Court.HalfWidth)
        {
            ResetToCenter();
            if (GameState.Instance != null) GameState.Instance.Goal(0);
        }
    }

    private void ResetToCenter()
    {
        transform.position = new Vector3(0, Court.BallY, 0);
        velocity = Vector3.zero;
        InPlay.Value = false;
    }

    public void Serve(int towardSide)
    {
        if (!IsServerInitialized) return;

        transform.position = new Vector3(0, Court.BallY, 0);
        velocity = new Vector3(towardSide == 0 ? -1f : 1f, 0f, Random.Range(-0.5f, 0.5f)).normalized;
        InPlay.Value = true;
    }

    private void OnInPlayChanged(bool prev, bool next, bool asServer)
    {
        Renderer r = GetComponent<Renderer>();
        if (r != null) r.material.color = next ? Color.white : Color.gray;
    }
}