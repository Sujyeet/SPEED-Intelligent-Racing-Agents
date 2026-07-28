# 🏎️ Unity ML-Agents & Netcode Kart Racing

![Unity](https://img.shields.io/badge/Unity-2022.3.62f2-blue.svg?logo=unity)
![ML-Agents](https://img.shields.io/badge/ML--Agents-v2.0.1-green.svg)
![Netcode](https://img.shields.io/badge/Netcode-NGO%20%2B%20Relay-orange.svg)
![License](https://img.shields.io/badge/License-MIT-purple.svg)

An advanced hybrid **Multiplayer & Reinforcement Learning Kart Racing Game** built with Unity 2022.3, **Unity Netcode for GameObjects (NGO)**, **Unity Gaming Services (Relay)**, and **Unity ML-Agents**.

---

## ✨ Features

- **🌐 Networked Multiplayer:** Full peer-to-peer multiplayer powered by Unity Relay and Netcode for GameObjects.
- **🤖 Reinforcement Learning AI:** Autonomous AI opponents trained using Unity ML-Agents (PPO algorithm).
- **🏁 3-Lap Race Winning System:** Seamless race loop with checkpoint tracking, lap counting, and victory scene transitions.
- **✨ Dynamic Gameplay Spells:** Throwable projectiles, permanent trap mines, and interactive power-ups with clean particle VFX.
- **🚗 Custom Vehicle Rendering:** Dual-mode animation adapters supporting single-sided geometry scale mirroring for custom car assets.

---

## 🛠️ Tech Stack & Architecture

- **Engine:** Unity 2022.3.62f2
- **Networking:** Unity Netcode for GameObjects (NGO) + Unity Relay Services
- **Machine Learning:** Unity ML-Agents Framework v2.0.1 (Barracuda Policy Inference)
- **Physics:** Custom Arcade Kart Physics & Raycast Suspension
- **UI & Flow:** Event-driven `GameModeManager` and decoupled assembly architecture (`KartGame.AI`, `KartGame.asmdef`).

---

## 🚀 Getting Started

### Prerequisites

- **Unity Hub** with **Unity 2022.3.62f2** installed.
- **Git** for version control.

### Installation

1. Clone the repository:
   ```bash
   git clone https://github.com/<your-username>/<your-repo-name>.git
   ```
2. Open **Unity Hub** -> Click **Add** -> Select the cloned project folder.
3. Open the project in **Unity 2022.3.62f2**.
4. Open the main scene:
   `Assets/Karting/Scenes/MainScene.unity` or `IntroMenu.unity`.
5. Press **Play** in the Unity Editor or click **Host Game** to test multiplayer & AI racing!

---

## 📄 Documentation

For full technical specifications, bug fix logs, and architectural breakdown, refer to [PROTOTYPE_DOCUMENTATION.md](PROTOTYPE_DOCUMENTATION.md).

---

## 📜 License

Distributed under the MIT License. See `LICENSE` for more information.
