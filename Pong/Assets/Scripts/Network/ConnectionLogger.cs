using FishNet;
using FishNet.Transporting;
using UnityEngine;

public class ConnectionLogger : MonoBehaviour
{
    private void Start()
    {
        var networkManager = InstanceFinder.NetworkManager;
        if (networkManager == null)
        {
            Debug.LogWarning("[Server] NetworkManager not found.");
            return;
        }

        networkManager.ServerManager.OnServerConnectionState += OnServerConnectionState;
        networkManager.ServerManager.OnRemoteConnectionState += OnRemoteConnectionState;
        networkManager.ClientManager.OnClientConnectionState += OnClientConnectionState;
    }

    private void OnDestroy()
    {
        var networkManager = InstanceFinder.NetworkManager;
        if (networkManager == null)
            return;

        networkManager.ServerManager.OnServerConnectionState -= OnServerConnectionState;
        networkManager.ServerManager.OnRemoteConnectionState -= OnRemoteConnectionState;
        networkManager.ClientManager.OnClientConnectionState -= OnClientConnectionState;
    }

    private void OnServerConnectionState(ServerConnectionStateArgs args)
    {
        Debug.Log($"[Server] Server connection state: {args.ConnectionState}");
    }

    private void OnRemoteConnectionState(FishNet.Connection.NetworkConnection connection, RemoteConnectionStateArgs args)
    {
        Debug.Log($"[Server] Remote connection state: {args.ConnectionState} | ClientId: {connection.ClientId} | Clients: {InstanceFinder.ServerManager.Clients.Count}");
    }

    private void OnClientConnectionState(ClientConnectionStateArgs args)
    {
        Debug.Log($"[Client] Client connection state: {args.ConnectionState}");
    }
}