using System;
using System.Collections.Generic;
using FishNet;
using FishNet.Managing;
using UnityEngine;

public class ConnectionHud : MonoBehaviour
{
    public static readonly List<Func<string>> Panels = new List<Func<string>>();

    private const float FlashDuration = 2.5f;

    private static string _flashMessage;
    private static float _flashUntil;

    private string _address = "localhost";
    private GUIStyle _flashStyle;

    public static void Flash(string message)
    {
        _flashMessage = message;
        _flashUntil = Time.unscaledTime + FlashDuration;
    }

    private void OnGUI()
    {
        NetworkManager networkManager = InstanceFinder.NetworkManager;

        GUILayout.BeginArea(new Rect(10f, 10f, 260f, Screen.height - 20f));
        GUILayout.BeginVertical("box");

        if (networkManager == null)
        {
            GUILayout.Label("NetworkManager not found.");
            GUILayout.EndVertical();
            GUILayout.EndArea();
            DrawFlash();
            return;
        }

        bool serverStarted = networkManager.ServerManager.Started;
        bool clientStarted = networkManager.ClientManager.Started;

        GUILayout.Label("Address");
        GUI.enabled = !clientStarted;
        _address = GUILayout.TextField(_address);
        GUI.enabled = true;

        GUILayout.BeginHorizontal();
        GUI.enabled = !serverStarted;
        if (GUILayout.Button("Start Server"))
            networkManager.ServerManager.StartConnection();
        GUI.enabled = serverStarted;
        if (GUILayout.Button("Stop Server"))
            networkManager.ServerManager.StopConnection(true);
        GUI.enabled = true;
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUI.enabled = !clientStarted;
        if (GUILayout.Button("Start Client"))
        {
            string address = string.IsNullOrWhiteSpace(_address) ? "localhost" : _address.Trim();
            networkManager.ClientManager.StartConnection(address);
        }
        GUI.enabled = clientStarted;
        if (GUILayout.Button("Stop Client"))
            networkManager.ClientManager.StopConnection();
        GUI.enabled = true;
        GUILayout.EndHorizontal();

        GUILayout.Space(6f);
        GUILayout.Label($"Server: {(serverStarted ? "Started" : "Stopped")}");
        GUILayout.Label($"Client: {(clientStarted ? "Started" : "Stopped")}");

        if (networkManager.TimeManager != null)
        {
            GUILayout.Label($"Tick: {networkManager.TimeManager.Tick}");
            if (clientStarted)
                GUILayout.Label($"RTT: {networkManager.TimeManager.RoundTripTime} ms");
        }

        for (int i = 0; i < Panels.Count; i++)
        {
            Func<string> panel = Panels[i];
            if (panel == null)
                continue;

            string text;
            try
            {
                text = panel();
            }
            catch (Exception exception)
            {
                text = $"Panel error: {exception.Message}";
            }

            if (string.IsNullOrEmpty(text))
                continue;

            GUILayout.Space(6f);
            GUILayout.BeginVertical("box");
            GUILayout.Label(text);
            GUILayout.EndVertical();
        }

        GUILayout.EndVertical();
        GUILayout.EndArea();

        DrawFlash();
    }

    private void DrawFlash()
    {
        if (string.IsNullOrEmpty(_flashMessage) || Time.unscaledTime > _flashUntil)
            return;

        if (_flashStyle == null)
        {
            _flashStyle = new GUIStyle(GUI.skin.box)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 18,
                wordWrap = true
            };
            _flashStyle.normal.textColor = Color.white;
        }

        float remaining = _flashUntil - Time.unscaledTime;
        Color previousColor = GUI.color;
        GUI.color = new Color(1f, 1f, 1f, Mathf.Clamp01(remaining / 0.5f));

        float width = Mathf.Min(500f, Screen.width - 40f);
        Rect rect = new Rect((Screen.width - width) * 0.5f, 40f, width, 60f);
        GUI.Box(rect, _flashMessage, _flashStyle);

        GUI.color = previousColor;
    }
}