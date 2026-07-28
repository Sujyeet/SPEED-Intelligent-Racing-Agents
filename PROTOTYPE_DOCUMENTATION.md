# Krash Kart Technical Documentation

## 1. Overview

* Project Name: Krash Kart
* Unity Version: 2022.3.62f2
* Core Systems: Unity Netcode for GameObjects, Unity Relay, Unity ML-Agents v2.0.1, Arcade Kart Physics

## 2. System Changes and Fixes

### Race Management and Scene Transitions
* 3-Lap Winning Condition: Connected `LapObject.cs`, `ObjectiveCompleteLaps.cs`, `MultiplayerRaceManager.cs`, `KartAgent.cs`, and `GameFlowManager.cs`. Completing three laps triggers `GameFlowManager.EndGame(true)` and loads `WinScene.unity`.
* Start Line Trigger Resolution: Refactored `LapObject.cs` trigger detection to resolve karts using `GetComponentInParent<ArcadeKart>()`.
* Spawn Guard: Added a 1.5-second timer guard (`Time.timeSinceLevelLoad < 1.5f`) in `LapObject.cs` to prevent karts positioned behind the line from triggering lap counts during race countdown.
* Event Invocation Safety: Updated `Objective.cs` to use null-conditional operators (`?.Invoke()`) for `TimeDisplay` events to prevent `NullReferenceException` crashes when UI objects are missing.
* Objective Manager Guards: Added null checks for `m_ObjectiveHUDManger` and `m_NotificationHUDManager` in `Objective.cs` during registration and completion.

### Kart Physics and Animation
* Idle Steering Adjustment: Scaled steering torque relative to kart velocity in `ArcadeKart.cs` to prevent karts from turning on the spot when idle.
* Kinematic Velocity Warnings: Guarded `Rigidbody.velocity` and `Rigidbody.angularVelocity` writes with `if (!Rigidbody.isKinematic)` checks in `ArcadeKart.cs` and `MultiplayerRaceManager.cs`.
* Wheel Mesh Flip Support: Added an X-axis scale flip (`flipX`) in `KartAnimation.cs` and `KartAnimationNetworked.cs` to render single-sided mesh geometry correctly on right-side wheels.
* Wheel Animation Null Checks: Added null validation in `KartAnimation.cs` for wheel transforms, wheel colliders, and input struct references.

### Items and Spells
* Projectile Trajectory: Adjusted default launch angle and exposed pitch, forward, and upward offsets in `ProjectileSpell.cs` for inspector tuning.
* Mine Visibility: Removed `GoInvisible()` coroutine from `TrapMine.cs` so mines remain visible to all players.
* VFX Lifecycle: Added explicit destruction timers (`Destroy(vfx, 2f)`) for spell and mine particle prefabs.

### Architecture and Networking
* Assembly Reference Decoupling: Added `GameModeManager.OnAgentFinishedRace` event delegate in `KartGame.GameFlow` namespace. This allows `KartAgent.cs` (`KartGame.AI.asmdef`) to notify race completion without directly referencing `MultiplayerRaceManager` or `Unity.Netcode.Runtime`.
* Netcode Spawn Verification: Updated `LapObject.cs` ownership checks to `if (netObj != null && netObj.IsSpawned && !netObj.IsOwner)` to support local unspawned test karts.
* Authentication Guard: Added `if (!AuthenticationService.Instance.IsSignedIn)` in `RelayManager.cs` to prevent authentication exceptions on scene reload.
* DisplayMessage Safety: Updated `DisplayMessage.cs` to verify `activeInHierarchy` before starting coroutines.

## 3. Issues and Resolutions Summary

| Problem | Cause | Solution |
| :--- | :--- | :--- |
| Steering turned while stationary | Fixed torque applied regardless of speed | Scaled steering power by speed ratio in `ArcadeKart.cs`. |
| Black wheels on right side | Single-sided mesh geometry inverted by Y-180 rotation | Applied X-axis scale mirroring (`-Mathf.Abs(x)`). |
| Assembly compilation error CS0012 | `KartAgent.cs` referenced Netcode classes across assembly definition | Added static event `GameModeManager.OnAgentFinishedRace`. |
| Lap trigger failed in single-player | `!netObj.IsOwner` evaluated to true when unspawned | Added `netObj.IsSpawned` check in `LapObject.cs`. |
| Premature lap count at spawn | Karts spawned inside the start line trigger box | Added 1.5s level load time guard in `LapObject.cs`. |
| Scene load crash on win | Null invocation of missing `TimeDisplay` and HUD managers | Added null-conditional checks in `Objective.cs`. |

## 4. Next Updates

1. Implement client-side prediction and server reconciliation for player movement under higher latency.
2. Update `KartAgent.cs` observation vector configuration and retrain ML-Agent models with obstacle avoidance.
3. Design custom HUD elements for lap tracking and position leaderboards.
4. Remove unused script references on `InGameMenu` and `KartClassic_Player` prefabs.
