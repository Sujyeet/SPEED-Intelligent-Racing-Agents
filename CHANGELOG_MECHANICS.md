# Krash Kart Mechanics and Code Changelog

## 1. 3-Lap Race Winning System

* **What Changed**: Connected `LapObject.cs`, `ObjectiveCompleteLaps.cs`, `MultiplayerRaceManager.cs`, `KartAgent.cs`, and `GameFlowManager.cs` to establish a 3-lap winning condition across Single Player and Multiplayer modes.
* **Why**: The prototype lacked an active race loop, win condition, and victory scene transition.
* **How**: 
  - `LapObject.cs` detects kart triggers using `GetComponentInParent<ArcadeKart>()`.
  - `ObjectiveCompleteLaps.cs` tracks lap progress (`currentLap`), updating from 1 to 3.
  - When `currentLap >= lapsToComplete` (3), `ObjectiveCompleteLaps` invokes `GameFlowManager.SendMessage("EndGame", true)`, playing victory audio/messages and loading `WinScene.unity`.
* **Why Not Alternatives**: 
  - *Alternative 1 (Time-based race finish)*: Rejected because lap-based completion provides clear competitive milestones for both human players and AI agents.
  - *Alternative 2 (Hardcoding scene load directly inside LapObject)*: Rejected because decoupling scene transitions through `GameFlowManager` preserves standard microgame event flow and prevents breaking UI toast notifications.

## 2. Idle Steering Power Scaling

* **What Changed**: Updated `ArcadeKart.cs` to scale steering power dynamically based on kart forward velocity (`m_SpeedRatio`).
* **Why**: The kart body turned on the spot while completely stationary (idle), breaking realistic kart physics.
* **How**: Steering torque applied to the Rigidbody in `ArcadeKart.cs` is multiplied by the kart's normalized forward velocity ratio, reducing steering torque to zero when stationary.
* **Why Not Alternatives**:
  - *Alternative 1 (Locking steering input keys when speed is zero)*: Rejected because it prevents wheel visual turning animation while waiting at the starting grid.

## 3. Wheel Geometry Mirroring Adapter

* **What Changed**: Added an X-axis scale mirroring toggle (`flipX`) in `KartAnimation.cs` and `KartAnimationNetworked.cs`.
* **Why**: Right-side wheel meshes from custom asset packs (e.g. Stylized Car) used single-sided geometry. Y-180 degree rotation exposed backfaces, causing right-side wheels to render pitch black.
* **How**: `UpdateWheelFromCollider` checks if `flipX` is enabled and sets `wheelTransform.localScale.x = -Mathf.Abs(scale.x)`, cleanly mirroring mesh normals outwards.
* **Why Not Alternatives**:
  - *Alternative 2 (Editing mesh normals in Blender or external 3D software)*: Rejected to avoid modifying source asset files and maintain a pure code-based solution within Unity.

## 4. Cross-Assembly Agent Race Finish Delegate

* **What Changed**: Added static event `GameModeManager.OnAgentFinishedRace` in `KartGame.GameFlow` namespace.
* **Why**: Calling `MultiplayerRaceManager` directly from `KartAgent.cs` caused C# compilation errors (`CS0012` / `CS0234`) due to assembly definition (`asmdef`) boundary restrictions between `KartGame.AI` and Netcode runtime.
* **How**: `KartAgent.cs` triggers `GameModeManager.OnAgentFinishedRace?.Invoke(this)` upon completing 3 laps. `MultiplayerRaceManager` subscribes to this delegate during `OnEnable`.
* **Why Not Alternatives**:
  - *Alternative 1 (Adding Unity.Netcode.Runtime reference to KartGame.AI.asmdef)*: Rejected because AI agent code should remain decoupled from multiplayer networking assemblies for single-player training efficiency.

## 5. Particle VFX Auto-Destruction

* **What Changed**: Added `Destroy(vfx, 2f)` in `SpellProjectile.cs` and `TrapMine.cs`.
* **Why**: Spell impact and mine explosion particle prefabs remained in the active scene hierarchy indefinitely, creating memory clutter and performance degradation over long sessions.
* **How**: Invoked Unity `Destroy` with a 2-second delay immediately upon instantiating impact particle effects.
* **Why Not Alternatives**:
  - *Alternative 1 (Leaving particles unmanaged)*: Rejected due to hierarchy bloat.
  - *Alternative 2 (Complex Object Pooling)*: Deferred until full release optimization phase to keep prototype particle lifecycle simple.
