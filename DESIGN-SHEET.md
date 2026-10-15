# Network Pong - Step 7 Design Sheet

**Student:** Demir Özpak  
**Student ID:** 2400004554  
**Course:** Multiplayer Programming  
**Topic:** Step 7 - Server-Authoritative Ball, Collision System & Network GameState  

---

## 1. Architecture & Authority Model
- **Overall Authority:** Server-Authoritative model.
- **Ball Simulation:** The `NetworkBall` position, physics bounce reflections, scoring bounds, and reset states are calculated strictly on the server/host instance.
- **Client Synchronization:** Clients receive position and state updates via FishNet's `NetworkTransform` component (`Client Authoritative` is disabled).
- **Paddles:** Synchronized via `NetworkTransform` with `Client Authoritative` enabled for responsive local player controls.
- **Game State:** Managed globally by `GameState` utilizing `[SyncVar]` to keep game state and score identical across all peers.

---

## 2. Component Specifications

### A. NetworkBall & Physics
- **Components:** `NetworkObject`, `NetworkTransform` (Server-authoritative), Collider, Rigidbody.
- **State Management:**
  - `SyncVar<bool> InPlay`: Indicates whether the round is active.
- **Collision Rules:**
  - **Arena Top/Bottom Walls:** Inverts the Y velocity (`velocity.y = -velocity.y`).
  - **Paddles:** Inverts the X velocity (`velocity.x = -velocity.x`) and calculates a deflection angle based on the vertical hit offset on the paddle.
  - **Goal Triggers (Left / Right):** When the ball crosses a goal boundary, the server reports a goal to `GameState`, resets ball coordinates to `(0, 0, 0)`, and sets `InPlay = false`.

### B. GameState & Scoring
- **Network Synchronization:**
  - `SyncVar<int> LeftScore`
  - `SyncVar<int> RightScore`
  - `SyncVar<int> ServingSide` (0 for Left player, 1 for Right player)
- **Round Logic:**
  - Evaluates active player connections before starting play.
  - Controls round reset delays and launches the ball towards the conceding player after a goal.

### C. HUD & Diagnostics
- Integrated scoreboard overlay displaying real-time scores centered at the top of the screen.
- Diagnostics configured via `TimeManager` (Tick Rate: 30) and statistics logging.