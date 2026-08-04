# Codebase Map — RESEARCH_ML

This document provides a comprehensive technical reference for the folder structure, scenes, core C# classes, ML-Agents entry points, physics systems, item mechanics, and telemetry logging in the `SPEED-Intelligent-Racing-Agents` workspace.

---

## 1. Directory & Folder Overview

* [`Assets/Karting/Scripts/AI/`](file:///c:/Unity%20Projects/Research%20Demo/ML%20TEST/Assets/Karting/Scripts/AI/): Deep Reinforcement Learning agents and ML-Agents integrations (`KartAgent.cs`).
* [`Assets/Karting/Scripts/KartSystems/`](file:///c:/Unity%20Projects/Research%20Demo/ML%20TEST/Assets/Karting/Scripts/KartSystems/): Core vehicle physics, input interfaces, and kart animation adapters (`ArcadeKart.cs`, `KartAnimation.cs`).
* [`Assets/Karting/Scripts/GameFlow/`](file:///c:/Unity%20Projects/Research%20Demo/ML%20TEST/Assets/Karting/Scripts/GameFlow/): Race management, state transitions, lap triggers, and victory conditions (`GameFlowManager.cs`, `ObjectiveCompleteLaps.cs`, `LapObject.cs`).
* [`Assets/Karting/Scripts/Items/` / `Spells/`](file:///c:/Unity%20Projects/Research%20Demo/ML%20TEST/Assets/Karting/Scripts/): Item pickup logic, projectile spells, and proximity traps (`ProjectileSpell.cs`, `TrapMine.cs`).
* [`config/ppo/`](file:///c:/Unity%20Projects/Research%20Demo/ML%20TEST/config/ppo/): ML-Agents PPO hyperparameter configuration files (`ArcadeDriver.yaml`, `ArcadeDriver2.yaml`, `ArcadeDriverML.yaml`).
* [`Results/`](file:///c:/Unity%20Projects/Research%20Demo/ML%20TEST/Results/): Trained `.onnx` models, TensorBoard summaries, and training status JSON logs.
* [`docs/`](file:///c:/Unity%20Projects/Research%20Demo/ML%20TEST/docs/): Research documentation, audit logs, decision records, research questions, and experiment templates.

---

## 2. Key Scenes

* **[`Assets/Karting/Scenes/MainScene.unity`](file:///c:/Unity%20Projects/Research%20Demo/ML%20TEST/Assets/Karting/Scenes/MainScene.unity)**: Primary 3-lap racing track with full checkpoint loops, item pickups, and multiplayer spawn grids.
* **[`Assets/Karting/Scenes/KartClassic_Training.unity`](file:///c:/Unity%20Projects/Research%20Demo/ML%20TEST/Assets/Karting/Scenes/KartClassic_Training.unity)**: Optimized ML-Agents training scene for parallel agent instances.
* **[`Assets/Karting/Scenes/IntroMenu.unity`](file:///c:/Unity%20Projects/Research%20Demo/ML%20TEST/Assets/Karting/Scenes/IntroMenu.unity)**: Main menu, lobby setup, and game mode selection.
* **[`Assets/Karting/Scenes/WinScene.unity`](file:///c:/Unity%20Projects/Research%20Demo/ML%20TEST/Assets/Karting/Scenes/WinScene.unity) & [`LoseScene.unity`](file:///c:/Unity%20Projects/Research%20Demo/ML%20TEST/Assets/Karting/Scenes/LoseScene.unity)**: Victory and defeat UI end-state scenes.

---

## 3. ML-Agents Subsystem & PPO Entry Points

* **Agent Class**: [`KartGame.AI.KartAgent`](file:///c:/Unity%20Projects/Research%20Demo/ML%20TEST/Assets/Karting/Scripts/AI/KartAgent.cs) (inherits from `Unity.MLAgents.Agent` and implements `IInput`).
* **Observation Code**:
  - `KartAgent.CollectObservations(VectorSensor sensor)` (Line 387): Encodes agent forward speed, normalized direction to next checkpoint, raycast sensor hits, and relative orientation.
* **Action Code**:
  - `KartAgent.OnActionReceived(ActionBuffers actions)` (Line 427): Converts continuous action index 0 (steering) and continuous action index 1 (acceleration/braking) into `InputData` struct consumed by `ArcadeKart`.
* **Reward Function**:
  - `PassCheckpointReward` (+1.0): Granted when crossing valid checkpoint trigger volume.
  - `TowardsCheckpointReward` (+0.01): Scaled positive reward for reducing Euclidean distance to target checkpoint.
  - `HitPenalty` (-1.0): Applied upon wall or obstacle collision.
  - `SpeedReward` (+0.02) & `AccelerationReward` (+0.01): Incentivizes forward momentum.
* **Episode Termination**:
  - `KartAgent.OnEpisodeBegin()` (Line 444): Resets agent position, orientation, velocity, and checkpoint index counters upon crash or track completion during training.

---

## 4. Vehicle Dynamics & Player Mechanics

* **Vehicle Controller**: `KartGame.KartSystems.ArcadeKart`
* **Input Interfaces**: `KartGame.KartSystems.IInput` implemented by `KeyboardInput.cs`, `KartAgent.cs`, and `NetworkedKeyboardInput.cs`.
* **Physics Calculations**:
  - Raycast ground detection with spring damper suspension alignment.
  - Top speed caps and dynamic steering torque scaling (`m_SpeedRatio`) to eliminate idle turning artifacts.
  - X-axis wheel scale mirroring (`flipX`) in `KartAnimation.cs` for single-sided custom car meshes.

---

## 5. Track Progression & Race Logic

* **Checkpoint System**: `KartGame.AI.KartAgent.Colliders` & `ObjectiveCompleteLaps.cs`.
* **Lap Trigger Logic**: `KartGame.Gameplay.LapObject`
  - Guarded against early start-line triggers during race countdown (`Time.timeSinceLevelLoad < 1.5f`).
  - Evaluates ownership safely (`netObj.IsSpawned && !netObj.IsOwner`).
* **Race Completion**: `GameFlowManager.EndGame(bool win)` triggers scene load to `WinScene.unity` upon completing 3 laps.
* **Cross-Assembly Delegate**: `GameModeManager.OnAgentFinishedRace` allows `KartAgent.cs` (`KartGame.AI` assembly) to notify `MultiplayerRaceManager` without violating assembly definition rules.

---

## 6. Item & Spell Mechanics

* **Forward Projectiles**: `ProjectileSpell.cs` / `SpellProjectile.cs` (Spawns forward-moving magic projectile with 2.0s auto-destruction on impact).
* **Proximity Traps**: `TrapMine.cs` (Deploys visible mine trap causing collision spin-out and 2.0s particle auto-destruction).

---

## 7. Logging & Telemetry CSV Export

* **CSV Data Files**: `AI_Manual_Player_Run_20260204_104604.csv`, `AI_Manual_Player_Run_20260707_114946.csv` in workspace root.
* **ML-Agents Session Summaries**: `Results/*/run_logs/timers.json` & `training_status.json`.

---

## 8. Training vs. Inference Configuration

* **Training Configs**: `config/ppo/ArcadeDriver.yaml` (Defines PPO batch size, buffer size, learning rate, beta, epsilon, gamma, and max_steps).
* **Inference Config**: `KartAgent.Mode = AgentMode.Inferencing` (Consumes pre-trained `.onnx` neural network models via Unity Barracuda).

---

## 9. External Open-Source Components

* **Unity Karting Microgame Template**: `Assets/Karting/` (Unity Companion License).
* **Unity ML-Agents Toolkit**: `Assets/ML-Agents/`, `Packages/com.unity.ml-agents` (Apache License 2.0).

---

## 10. Unclear or Risky Code Areas

* **Multiplayer Netcode Assembly Boundary**: `KartAgent.cs` operates cleanly in single-player, but cross-assembly triggers during multi-agent multiplayer evaluation require monitoring `GameModeManager.OnAgentFinishedRace`.
* **Ray Perception Layer Mask**: `KartAgent.Mask` must strictly isolate track boundaries from checkpoint triggers to avoid false collision penalties.
