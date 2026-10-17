# Week 03 Homework: Pong Connects and Replicates

Demir Özpak - 2400004554

---

### Checkpoint D
1. **Which component gives the ObjectId?**  
   The `NetworkObject` component.
2. **Which machine chose LEFT, how did the client learn it?**  
   The server chose LEFT in `OnStartServer` by picking the first free side, and the client learned it via synchronization of the `SyncVar<int> Side`.
3. **Which line stops other clients moving your paddle?**  
   The ownership check in `Update()`: `if (!IsOwner) return;`.

---

### Predictions (Before Step 6) & Grades
1. **You press W, nothing moves elsewhere. Who must tell whom, in what order?**  
   The client sends its transform update to the server, then the server replicates it to the other clients.  
   **Grade:** Right
2. **Three balls. Which machine should own the one real ball? What must a client receive to draw it?**  
   The server must own the one real ball; clients only need to receive position updates via NetworkTransform to render it.  
   **Grade:** Right
3. **If the server believes "my paddle is at z = 3.2" from a client, what is the first cheat?**  
   Teleportation / speed hacking (the client sending arbitrary coordinate jumps without server-side validation).  
   **Grade:** Right

---

### Connection Card (Step 11)
- **Partner:** Osman Özay
- **Transport:** Tugboat (UDP 7770)
- **Tick Rate:** 30 Hz
- **Network:** Mobile Hotspot

| Direction | Server IP | Client ID / Side | Partner Client ID / Side | RTT | Worked First Try? |
| :--- | :--- | :--- | :--- | :--- | :--- |
| **I host, partner joins** | 192.168.43.15 | 0 / LEFT | 1 / RIGHT | ~24 ms | Yes |
| **Partner hosts, I join** | 192.168.43.88 | 1 / RIGHT | 0 / LEFT | ~28 ms | Yes |

*Both paddles moved smoothly on both screens without input cross-talk.*

---

### Design Sheet
| THING | STATE OR EVENT? | FISHNET PIECE | ON WHICH OBJECT? | WHO WRITES / CALLS? |
| :--- | :--- | :--- | :--- | :--- |
| **Ball position** | State | NetworkTransform | Ball prefab | Server writes |
| **Ball in play?** | State | SyncVar\<bool\> | Ball prefab | Server writes |
| **Two scores** | State | SyncVar\<int\> | GameState | Server writes |
| **Whose serve** | State | SyncVar\<int\> | GameState | Server writes |
| **"Let me serve"** | Event | ServerRpc | PaddleController | Client calls, Server runs |
| **"LEFT scored!" to all** | Event | ObserversRpc | PaddleController | Server calls, Observers run |
| **"YOU SERVE" to one** | Event | TargetRpc | PaddleController | Server calls, Target client runs |
| **Event feed** | State | SyncList\<string\> | GameState | Server writes |

*Which script creates the one ball, where, in which callback?*  
`BallSpawner.cs` creates the ball on the Court object during the `OnStartServer` callback using `ServerManager.Spawn()`.  
*First line of the ball's Update:*  
`if (!IsServerInitialized) return;`

---

### Step 8 Questions
1. **Which RPC for a hit sound on all clients? Which for "not your serve" to one?**  
   `[ObserversRpc]` for the hit sound to all; `[TargetRpc]` for "not your serve" to a single client.
2. **Where is the score, who may change it?**  
   The score is stored in `GameState.cs` as a `SyncVar<int>`, and only the server can modify it.
3. **Ball bounced on server but passed through on your screen: which paddle copy did the server use, why different?**  
   The server used its authoritative copy of the paddle. They differed due to network latency and interpolation delay between the client's local input and server state.

---

### Step 10 — Bytes and Tick Rate

| TICK RATE | SERVER OUT B/S (RALLY) | BALL IN A CLIENT WINDOW | OTHER PADDLE |
| :---: | :---: | :--- | :--- |
| **10** | ~1420 B/s | Choppy and stuttering | Jittery between updates |
| **30** | ~3800 B/s | Balanced, smooth and playable | Stable interpolation |
| **60** | ~7850 B/s | Extremely smooth movement | Very responsive |

- **What did 60 cost?** Network bandwidth usage scaled up roughly linearly, almost doubling compared to 30 Hz.
- **What did it buy?** Higher visual fidelity, smoother motion, and lower perceived interpolation delay.
- **Why did the ball step even for the player moving a paddle?** Because the ball is simulated strictly on the server at discrete server tick rates without local client-side prediction.

---

### Notes Section 14 Answers
1. **Latency & Travel Time:** Network packets take physical time to travel, meaning clients always render slightly delayed server state.
2. **Server Architectures:** Dedicated servers run independently and prevent host advantage, whereas listen servers run server and host client in the same process.
3. **UDP vs TCP:** UDP avoids head-of-line blocking by not waiting for lost packets, which is ideal for real-time state updates like ball position.
4. **NetworkObject vs NetworkBehaviour:** `NetworkObject` manages network identity, ownership, and spawning. `NetworkBehaviour` enables network-aware scripts, RPCs, and SyncVars.
5. **Ownership Execution:** On a client, only paddles where `IsOwner == true` process local keyboard inputs, preventing players from controlling opponent paddles.
6. **NetworkTransform Settings:** Client Authoritative is enabled on paddles so owners control their movement, but disabled on the ball because only the server moves it.
7. **Spawning Authoritatively:** Shared network objects must be instantiated and spawned on the server using `ServerManager.Spawn()`; local instantiation only affects the local instance.
8. **State vs Event:** Continuous values (score, position, state) belong in SyncVars/NetworkTransform, while one-off moments (goals, serve triggers, SFX) use RPC events.
9. **Authoritative Logic:** The server validates collisions and game state. Clients cannot directly write server-side game variables; they must send requests via `[ServerRpc]`.
10. **Tick Rate Impact:** Higher tick rates improve synchronization precision at the cost of linear bandwidth overhead.