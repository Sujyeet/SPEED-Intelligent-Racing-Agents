# Project Overview — RESEARCH_ML

## 1. Project Purpose
The purpose of this repository (`SPEED-Intelligent-Racing-Agents`) is to serve as a dedicated, documented, reproducible, and safely versioned AI research environment for investigating Deep Reinforcement Learning (DRL) and heuristic decision-making in non-player character (NPC) racing agents.

## 2. Technical Stack & Framework Versions
* **Unity Engine Version**: `2022.3.62f2`
* **ML-Agents Framework Version**: `v2.0.1` (`com.unity.ml-agents` package, Barracuda inference runtime)
* **Networking Framework**: Unity Netcode for GameObjects (NGO) & Unity Gaming Services Relay (used for multiplayer testing)

## 3. Workspace Purpose & Architecture Relationship
* **Research-Copy Purpose**: This repository is the designated RESEARCH COPY for algorithmic experiments, evaluation, telemetry gathering, and research documentation.
* **Full-Game Folder Relationship**: The full-game production version is completely separate and maintained outside of this workspace. The research copy isolates agent evaluation and training from commercial game asset iterations.

## 4. Open-Source Origin & Licensing
* **Original Open-Source Base**: 
  - Unity Karting Microgame Template (Unity Technologies) — License: Unity Companion License / Asset Store EULA.
  - Unity ML-Agents Toolkit v2.0.1 (Unity Technologies) — License: Apache License 2.0.
* **Original Repository URL**: Not verified (Local prototype base derived from Unity Microgame samples).

## 5. Agent Systems
* **PPO Agent Source**: Implemented in [`Assets/Karting/Scripts/AI/KartAgent.cs`](file:///c:/Unity%20Projects/Research%20Demo/ML%20TEST/Assets/Karting/Scripts/AI/KartAgent.cs).
* **Agent Architecture**: Inherits from `Unity.MLAgents.Agent`. Uses `RayPerceptionSensor3D` for environmental awareness and discrete/continuous action distribution for steering, acceleration, braking, and item triggering.

## 6. Vehicle & Physics System
* **Kart Controller**: Implemented in [`Assets/Karting/Scripts/KartSystems/Inputs/KeyboardInput.cs`](file:///c:/Unity%20Projects/Research%20Demo/ML%20TEST/Assets/Karting/Scripts/KartSystems/Inputs/KeyboardInput.cs) and `ArcadeKart.cs`.
* **Physics Model**: Raycast-based arcade vehicle physics with custom steering torque, ground alignment, top speed caps, and dynamic velocity scaling.

## 7. Tracks & Environment Scenes
* **Active Scenes**:
  - `Assets/Karting/Scenes/IntroMenu.unity` (Main menu & lobby)
  - `Assets/Karting/Scenes/MainScene.unity` (Primary racing track with checkpoints)
  - `Assets/Karting/Scenes/KartClassic_Training.unity` (Dedicated ML-Agents training scene)
  - `Assets/Karting/Scenes/WinScene.unity` & `LoseScene.unity` (End-of-race state scenes)

## 8. Player & Item Systems
* **Player Mechanics**: Manual driving controls (Keyboard/Gamepad), dynamic drift boosting, checkpoint progression tracking, and 3-lap winning system.
* **Spell / Pickup System**: Item pickups including forward projectiles (`SpellProjectile.cs` / `ProjectileSpell.cs`) and proximity mines (`TrapMine.cs`).

## 9. Thesis Prototype & Known Differences
* **Known Prototype Modifications**:
  - Added 3-lap race completion loop connected to `GameFlowManager`.
  - Added wheel mesh scale mirroring (`flipX`) for right-side wheels.
  - Added cross-assembly event delegate (`GameModeManager.OnAgentFinishedRace`) to notify race managers without introducing circular assembly dependencies.
  - Added particle VFX auto-destruction timers (2.0s delay).
* **Unverified Thesis Prototype Baseline Differences**: Not verified (Subject to systematic comparison during initial baseline runs).

## 10. Unknown Information Requiring Verification
* Exact historical hyperparameters used for initial `prototype-v1.0` model training runs.
* Full item synergy matrix used for heuristic agent baseline comparisons.
