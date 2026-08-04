# RESEARCH_ML — Development Log & Prototype

This repository contains the prototype codebase and development log for RESEARCH_ML, an active Unity 2022.3 kart racing project. It demonstrates real-time peer-to-peer multiplayer via Unity Netcode for GameObjects and Unity Relay, alongside autonomous AI opponents trained using Unity ML-Agents.

## Project Status

* Stage: Active Development Prototype
* Focus: Multiplayer Networking, ML-Agent AI, and Physics Integration

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

## Setup and Local Testing

1. Clone the repository:
   ```bash
   git clone https://github.com/Sujyeet/SPEED-Intelligent-Racing-Agents.git
   ```
2. Open Unity Hub and add the project using Unity 2022.3.62f2.
3. Open `Assets/Karting/Scenes/IntroMenu.unity` or `MainScene.unity`.
4. Press Play in the Unity Editor to start local testing or host a multiplayer session.

## Documentation and Dev Log

For complete technical notes, architectural breakdown, and issue resolution history, refer to [PROTOTYPE_DOCUMENTATION.md](PROTOTYPE_DOCUMENTATION.md).

## License

This project is licensed under the MIT License. See the LICENSE file for details.
