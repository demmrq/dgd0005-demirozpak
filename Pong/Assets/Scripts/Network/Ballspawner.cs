using FishNet.Object;
using UnityEngine;

public class BallSpawner : NetworkBehaviour
{
    [SerializeField] private NetworkBall ballPrefab;

    public override void OnStartServer()
    {
        base.OnStartServer();

        if (ballPrefab == null)
        {
            Debug.LogError("[Server] BallSpawner: ballPrefab is not assigned.");
            return;
        }

        NetworkBall b = Instantiate(ballPrefab, new Vector3(0, Court.BallY, 0), Quaternion.identity);
        Spawn(b.gameObject);
    }
}