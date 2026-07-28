# Krash Kart — Prototype Handover Documentation

---

## 1. Project Overview & Scope

- **Project Name:** Krash Kart (ML & Multiplayer Kart Racing Prototype)
- **Unity Version:** 2022.3.62f2
- **Key Frameworks & Tech Stack:**
  - **Netcode for GameObjects (NGO):** Networked multiplayer kart movement & state synchronization.
  - **Unity Gaming Services (Relay & Authentication):** Peer-to-peer multiplayer lobby connections over Relay.
  - **Unity ML-Agents (v2.0.1):** AI-controlled bot karts capable of single-player & multiplayer racing.
  - **Arcade Kart Physics:** Customized Microgame physics model.

---

## 2. Comprehensive Log of Technical Changes & System Upgrades

### A. Race Loop, Winning Conditions & Scene Flow
- **3-Lap Race Winning Logic:** Integrated 3-lap race completion with `LapObject.cs`, `ObjectiveCompleteLaps.cs`, `MultiplayerRaceManager.cs`, `KartAgent.cs`, and `GameFlowManager.cs`. Completing 3 laps in Single Player or Multiplayer mode triggers `GameFlowManager.EndGame(true)` and smoothly transitions to `WinScene.unity`.
- **Start/Finish Line Trigger Robustness (`LapObject.cs`):** Updated `OnTriggerEnter` to resolve player and AI karts using `other.GetComponentInParent<ArcadeKart>()`.
- **Spawn-Time Trigger Filtering:** Implemented a `Time.timeSinceLevelLoad < 1.5f` filter in `LapObject.cs` to prevent karts spawning behind the start line from prematurely triggering lap counts during race countdown.
- **Null Safety in Event Invocations (`Objective.cs`):** Replaced direct calls to `TimeDisplay.OnUpdateLap()` and `TimeDisplay.OnSetLaps()` with safe null-conditional invocations (`?.Invoke()`) to eliminate `NullReferenceException` crashes when UI elements are missing.
- **HUD Manager Guards (`Objective.cs`):** Added null checks for `m_ObjectiveHUDManger` and `m_NotificationHUDManager` during `Register` and `CompleteObjective` so game completion succeeds even if HUD elements are disabled.

### B. Physics & Kart Movement Enhancements
- **Idle Steering Glitch Fix (`ArcadeKart.cs`):** Scaled steering power dynamically with kart velocity to prevent the kart body from turning on the spot while stationary.
- **Kinematic Velocity Warning Cleanup:** Guarded `Rigidbody.velocity` and `Rigidbody.angularVelocity` writes with `if (!Rigidbody.isKinematic)` checks in `ArcadeKart.cs` and `MultiplayerRaceManager.cs` to eliminate Unity kinematic warning spam.
- **Single-Sided Wheel Geometry Fix (Stylized Car Asset Pack):** Replaced Y-rotation offsets with X-axis scale mirroring (`localScale.x = -Mathf.Abs(s.x)` via `flipX` toggle in `KartAnimation.cs` and `KartAnimationNetworked.cs`), eliminating backface rendering (black wheels) on right-side wheels.
- **Null-Safe Wheel Animations (`KartAnimation.cs`):** Added comprehensive null guards for wheel transforms, wheel colliders, and `InputData` struct handling.

### C. Gameplay Spells & VFX Optimization
- **Projectile Launch Arc (`ProjectileSpell.cs` & `SpellProjectile.cs`):** Elevated default launch pitch and exposed `upwardPitch`, `forwardOffset`, and `upwardOffset` in the Inspector for real-time tuning.
- **Permanent Mine Visibility (`TrapMine.cs`):** Removed the `GoInvisible()` coroutine so trap mines remain permanently visible to all players.
- **VFX Hierarchy Cleanup:** Added automatic 2-second destruction (`Destroy(vfx, 2f)`) for spell projectile impacts and mine explosion particle clones to keep the hierarchy clean.

### D. Networking, Architecture & Assembly Definition Decoupling
- **Assembly Reference Decoupling (`GameModeManager.cs`):** Added static event delegate `GameModeManager.OnAgentFinishedRace` in `KartGame.GameFlow` namespace. This allowed `KartAgent.cs` (`KartGame.AI.asmdef`) to notify race completion without directly referencing `MultiplayerRaceManager` or `Unity.Netcode.Runtime` (`CS0012` assembly error fix).
- **Netcode IsSpawned Guard (`LapObject.cs`):** Updated ownership check to `if (netObj != null && netObj.IsSpawned && !netObj.IsOwner)`, enabling local unspawned karts to trigger lap counts in single-player test mode.
- **Relay Re-authentication Guard (`RelayManager.cs`):** Guarded `SignInAnonymouslyAsync()` with `if (!AuthenticationService.Instance.IsSignedIn)` to prevent authentication errors on scene reload.
- **DisplayMessage Coroutine Safety (`DisplayMessage.cs`):** Ensured GameObjects are active (`gameObject.SetActive(true)`) before starting UI coroutines.

---

## 3. Problems Faced & Solutions Summary

| Problem / Issue | Root Cause | Solution Implemented |
| :--- | :--- | :--- |
| **Kart Steer Glitch** | Full steering torque applied at zero velocity | Scaled steering power by speed ratio in `ArcadeKart.cs`. |
| **Black Right Wheels** | Single-sided mesh geometry inverted by Y-180° rotation | Mirrored geometry along X-axis using `flipX` scale inversion (`-Mathf.Abs(x)`). |
| **Assembly Compiler Errors (`CS0012`)** | `KartAgent.cs` referenced Netcode classes across `asmdef` boundaries | Added static event `GameModeManager.OnAgentFinishedRace` to bridge events without cross-assembly imports. |
| **Trigger Failed on Single-Player** | `!netObj.IsOwner` evaluated to `true` when unspawned | Added `netObj.IsSpawned` check before filtering non-owned karts in `LapObject.cs`. |
| **Premature Lap 1 at Spawn** | Karts spawned inside the start line trigger box | Added `Time.timeSinceLevelLoad < 1.5f` spawn guard in `LapObject.cs`. |
| **Crash on Win Completion** | Direct `null` invocation of `TimeDisplay` and missing HUD managers | Replaced with safe `?.Invoke()` and added null guards in `Objective.cs`. |

---

## 4. Recommended Next Updates for Full Game Development

1. **Multiplayer Netcode Polish:**
   - Implement client-side prediction and server reconciliation for smooth gameplay under higher latency.
   - Add lobby settings UI for lap count selection (e.g. 3, 5, 10 laps) and track selection.

2. **AI & ML-Agent Training:**
   - Update `KartAgent.cs` vector observation size (resolve warning for 13 observations vs 12 configured size).
   - Retrain ML-Agent neural networks with spell pickup handling and obstacle avoidance.

3. **UI / UX Polish & Clean-up:**
   - Design a modern, custom game HUD (Lap counter, Speedometer, Placement Leaderboard).
   - Clean up missing script references on `InGameMenu` and `KartClassic_Player` prefabs in Unity Inspector.

4. **Audio & Asset Expansion:**
   - Expand car visual selections using the Stylized Car asset pack.
   - Add engine audio pitch shifting based on RPM/velocity and impact sound effects.

---
*Generated automatically by Antigravity AI Assistant.*
