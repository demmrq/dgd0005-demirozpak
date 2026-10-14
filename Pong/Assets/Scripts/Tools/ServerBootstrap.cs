using System;
using FishNet;
using UnityEngine;

public class ServerBootstrap : MonoBehaviour
{
    private void Start()
    {
        string[] args = Environment.GetCommandLineArgs();
        bool startServer = false;

        for (int i = 0; i < args.Length; i++)
        {
            string arg = args[i].Trim().ToLowerInvariant();
            if (arg == "-server" || arg == "--server")
            {
                startServer = true;
                break;
            }
        }

        if (!startServer)
            return;

        if (InstanceFinder.ServerManager == null)
        {
            Debug.LogError("[Server] ServerManager not found.");
            return;
        }

        if (InstanceFinder.ServerManager.Started)
            return;

        Debug.Log("[Server] Starting server from command line.");
        InstanceFinder.ServerManager.StartConnection();
    }
}