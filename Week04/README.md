# Week 04 Homework: The Server Decides

Demir Özpak - 2400004554

The paddle is now server-authoritative. The client sends only its input axis (up, down or nothing) once per tick, the server moves and clamps the paddle, and the server ignores a second input from the same client inside 30 ms.

---

### Step 2 — Input up, position down

- **RTT measured:** ~7 ms on the client window (up to ~20 ms at times), Multiplayer Play Mode on one laptop, tick rate 30.
- Pressing W now moves my paddle in every window, my own window included, slightly late: the input has to reach the server and the new position has to come back.

---

### Step 3 — Check the input

**The server receives `axis = 50`. Which line makes the paddle move at normal speed, and on which machine does that line run?**  
`axis = Mathf.Clamp(axis, -1f, 1f);` inside `ServerSetAxis`. It runs on the **server**, because `ServerSetAxis` is a `[ServerRpc]`: the client only calls it, the body executes on the server.

---

### Step 4 — Rate limit 30 ms

**Why 30 ms?**  
At tick rate 30 one tick is ~33 ms, and an honest client sends exactly one input per tick. 30 ms is just under one tick, so a real keyboard always passes, and anything arriving faster than that is a program, not a player.

**What happens with 5 ms?**  
The limit would be far below one tick, so a cheating client could send several inputs per tick and most of them would be accepted, flooding the server with RPCs. The rate limit would protect almost nothing. (In our code the time comes from `TimeManager.Tick`, so two inputs in the same tick have a gap of 0 and are still rejected; with a real clock 5 ms would let about six inputs per tick through.)

**What happens with 500 ms?**  
The server would accept only one input every 15 ticks. Honest players would be punished: the paddle would react up to half a second late, and releasing the key could be ignored, so the paddle keeps moving after the player let go.

**Observation:** in normal play the counter still rises slowly (a few rejections every few seconds per paddle). With two editors on one laptop, two honest packets sometimes arrive in the same server tick and the second one is rejected. This is the cost of putting the limit so close to one tick; the next tick's input arrives right after, so the paddle does not stutter.

---

### Step 5 — The cheating client

| KEY | SERVER CONSOLE | PADDLE |
| :--- | :--- | :--- |
| **C once** | `[Server] Rejected input from client 1: too fast.` ×20, counter +20 | moves at normal speed |
| **X once** | nothing | moves at normal speed, never faster |
| **T once** | nothing | jumps to the ball on the client only, snaps back to the server position as soon as the server sends the next update (pressing W/S) |

**Server Console after one C:**

![Server console after one C](step5-console.png)

*Why 20 and not 19:* in the tick where C was pressed, the normal per-tick input (sent from `OnTick`) also arrived. The server received 21 inputs in that tick, accepted 1 and rejected 20. The rule is the same as in the table: one input per tick.

1. **C sends 20 inputs and the server accepts one. Why is the paddle not 20 times faster?**  
   An input does not move the paddle; it only sets `_serverAxis`, clamped to ±1. The server moves the paddle once per tick by `_serverAxis * speed * TickDelta`, so the speed is fixed no matter how many inputs arrive, and the extra 19 are rejected anyway.
2. **T moved the paddle on your screen. Why did the ball on the server not notice?**  
   T changed only the client's local copy of the transform and sent no RPC. The ball runs on the server and collides with the server's copy of the paddle, which never moved; with Client Authoritative off, the client's position is never sent to the server.
3. **Which of the three cheats would also fail without the rate limit, and why?**  
   X and T. X fails because of the clamp (50 becomes 1), and T fails because the client no longer has authority over its position. C would also not make the paddle faster, but without the rate limit the server would accept and process every spammed RPC.
