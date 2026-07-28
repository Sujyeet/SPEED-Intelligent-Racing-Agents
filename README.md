# Krash Kart

Krash Kart is a hybrid multiplayer and machine learning kart racing game built in Unity 2022.3. It combines real-time peer-to-peer networking via Unity Netcode for GameObjects and Unity Relay with autonomous AI opponents trained using Unity ML-Agents.

## Key Features

* Networked Multiplayer: Host and client matchmaking powered by Unity Relay and Unity Netcode for GameObjects.
* Machine Learning AI: Autonomous bot karts trained with Proximal Policy Optimization (PPO) using Unity ML-Agents.
* 3-Lap Race Loop: Complete race progression tracking checkpoints, lap triggers, and end-of-race victory state transitions.
* Gameplay Items: Pickups including forward projectiles, visible proximity mines, and custom particle impact effects.
* Custom Vehicle Support: Scaled animation adapters with X-axis mesh mirroring to support custom car models and single-sided mesh geometry.

## Tech Stack

* Engine: Unity 2022.3.62f2
* Networking: Unity Netcode for GameObjects, Unity Gaming Services Relay
* AI Framework: Unity ML-Agents v2.0.1 (Barracuda inference runtime)
* Physics: Custom raycast Arcade Kart physics model

## Setup and Running

1. Clone the repository to your local machine:
   ```bash
   git clone https://github.com/username/krash-kart.git
   ```
2. Open Unity Hub and add the project using Unity 2022.3.62f2.
3. Open `Assets/Karting/Scenes/IntroMenu.unity` or `MainScene.unity`.
4. Press Play in the Unity Editor to start local testing or host a multiplayer session.

## Project Structure

* `Assets/Karting/Scripts/AI`: ML-Agent driver implementations and training adapters.
* `Assets/Karting/Scripts/Multiplayer`: Netcode synchronization, Relay connection managers, and networked kart controllers.
* `Assets/Karting/Scripts/GameModes`: Checkpoint logic, lap objects, and race objective handlers.
* `Assets/Karting/Scripts/KartSystems`: Physics controllers, steering, and wheel animation logic.

## Documentation

A complete technical breakdown of architectural decisions, bug fixes, and development notes is available in [PROTOTYPE_DOCUMENTATION.md](PROTOTYPE_DOCUMENTATION.md).

## License

This project is licensed under the MIT License. See the LICENSE file for details.
