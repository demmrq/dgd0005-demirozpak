# Pong

Demir Özpak - 2400004554

Each player owns one paddle. The ball has no client owner and only the server moves it. Everyone shares the score, whose serve it is and the last six events. A third client joins as a spectator.

Unity 6000.0.32f1, FishNet 4.7.3R, ParrelSync.

## Run

1. Open `Assets/Scenes/SampleScene.unity` in Unity 6, make sure Tick Rate is set to 30 Hz and press Play.
2. Click **Start Server** then **Start Client** on the host editor (LEFT player).
3. In the second instance (ParrelSync clone), connect to `localhost` / `127.0.0.1` and click **Start Client** (RIGHT player).
4. Move with W/S or the Up/Down arrow keys. Press Space when it is your serve.

---

## Step 10 — Bytes and tick rate

### Measurements
| Tick Rate | Server out B/s | Visual smoothness (ball and paddles) |
| :---: | :---: | :--- |
| **10** | ~1420 B/s | Choppy and stuttering. Ball and paddles exhibit visible jitter between frames. |
| **60** | ~7850 B/s | Extremely smooth movement with no visible stutter, but network bandwidth usage increases nearly 5-6x. |
| **30** | ~3800 B/s | Balanced and smooth. Good visual quality with reasonable bandwidth consumption. |

### Analysis Questions
- **Did the bytes/sec change roughly linearly with tick rate?**
  Yes, the bandwidth usage scales nearly linearly with the tick rate because NetworkTransform synchronizes object positions and state updates once per tick.

- **Was your prediction in Step 7 accurate?**
  Yes, lowering the tick rate reduced network traffic significantly at the cost of visual smoothness, while increasing it provided higher precision at the expense of higher throughput.
