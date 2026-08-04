# Initial Repository Audit

* **Date & Time**: 2026-08-04T18:13:29+05:30
* **Repository**: SPEED-Intelligent-Racing-Agents (Research Copy)
* **Local Workspace Path**: `c:\Unity Projects\Research Demo\ML TEST`

---

## 1. Current Branch
* **Active Branch**: `develop`

## 2. Current Commit Hash
* **Commit Hash**: `f882099b67ec924cc0959589bcf333efa1ba9210`
* **Commit Message**: `Test save 03/08/2026 19:30`

## 3. Git Remote Name and URL
* **Remote Name**: `origin`
* **Remote URL**: `https://github.com/Sujyeet/krash-kart-devlog.git`

## 4. Working Tree Status
* **Working Tree Clean**: **NO (DIRTY)**
* **Uncommitted Modified Files** (9 files):
  1. `.agents/AGENTS.md`
  2. `CHANGELOG_MECHANICS.md`
  3. `GAME_DEVELOPMENT_BLUEPRINT.md`
  4. `GIT_GUIDE.md`
  5. `PROTOTYPE_DOCUMENTATION.md`
  6. `ProjectSettings/ProjectSettings.asset`
  7. `README.md`
  8. `ROOT_CAUSE_RESOLUTIONS.md`
  9. `kart-git.ps1`

## 5. Existing Branches and Tags
* **Local Branches**:
  - `develop` (current)
  - `main`
* **Remote Tracking Branches**:
  - `origin/develop`
  - `origin/main`
* **Tags**:
  - `prototype-v1.0`

## 6. Repository Structure
* **Unity Root Structure**:
  - `Assets/`: Unity game assets, C# scripts, prefabs, scenes, ML-Agents timers
  - `Packages/`: `manifest.json`, `packages-lock.json`
  - `ProjectSettings/`: Unity project configuration files
  - `UserSettings/`: Local Unity user preferences
  - `Library/`, `Logs/`, `Temp/`: Auto-generated Unity build and cache folders (git ignored)
* **ML / Research Directories**:
  - `config/ppo/`: PPO training configurations (`ArcadeDriver.yaml`, `ArcadeDriver2.yaml`, `ArcadeDriverML.yaml`, `simple oval yaml.txt`)
  - `Results/`: ML-Agents run outputs (`complex_track_obstacles`, `complex_track_obstacles_v2`, `complex_track_obstacles_v3`, `complex_track_v1_new`, `complex_track_v2_new`, `ppo`) containing TensorBoard summaries, `training_status.json`, `timers.json`, and `.onnx` models
  - `Checkpoints/`: Snapshot backups of past project states
* **Scripts & Tools**:
  - `kart-git.ps1`: Interactive PowerShell Git management script

## 7. Existing Documentation
* `README.md`: Project overview and dev log summary
* `CHANGELOG_MECHANICS.md`: Log of code and mechanics changes
* `ROOT_CAUSE_RESOLUTIONS.md`: Issue, bug, and crash resolution log
* `GAME_DEVELOPMENT_BLUEPRINT.md`: Living master production plan and roadmap
* `PROTOTYPE_DOCUMENTATION.md`: Technical documentation for mechanics and physics
* `GIT_GUIDE.md`: Developer guide for Git operations using `kart-git.ps1`
* `IDEA.md`: High-level project rationale
* `.agents/AGENTS.md`: Agent behavior rules and documentation protocol

## 8. Existing Unity Projects
* **Single Unity Project Root**: `c:\Unity Projects\Research Demo\ML TEST`
* **Unity Engine Version**: `2022.3.62f2`

## 9. Existing Trained Models and Configuration Files
* **Trained Models (`.onnx`)**:
  - `Results/complex_track_obstacles/*.onnx`
  - `Results/complex_track_obstacles_v2/*.onnx`
  - `Results/complex_track_obstacles_v3/*.onnx`
  - `Results/complex_track_v1_new/*.onnx`
  - `Results/complex_track_v2_new/*.onnx`
  - `Results/ppo/*.onnx`
* **ML Training Configurations**:
  - `config/ppo/ArcadeDriver.yaml`
  - `config/ppo/ArcadeDriver2.yaml`
  - `config/ppo/ArcadeDriverML.yaml`
  - `Results/*/configuration.yaml`

## 10. Existing Result Files
* **CSV Telemetry Data**:
  - `AI_Manual_Player_Run_20260204_104604.csv`
  - `AI_Manual_Player_Run_20260707_114946.csv`
* **TensorBoard & Training Summaries**:
  - `Results/*/run_logs/training_status.json`
  - `Results/*/run_logs/timers.json`

## 11. Files That May Contain Secrets, Private Data, or Credentials
* `ProjectSettings/ProjectSettings.asset`:
  - `cloudProjectId`: `f58b6c77-dc4b-46f9-8a15-3cced833b6f4`
  - `organizationId`: `sujyeeet`
* `Assets/ML-Agents/Timers/*_timers.json`:
  - Contains command line parameters with session access tokens (`accessToken ...`).
* Note: `.gitignore` ignores `UserSettings/`, `Library/`, `Logs/`, and `venv/`.

## 12. Files Originating from External Open-Source Projects
* **Unity Karting Microgame Template** (Unity Technologies):
  - `Assets/Karting/` (Kart physics, track assets, tutorial assemblies)
* **Unity ML-Agents Toolkit v2.0.1** (Unity Technologies):
  - `Assets/ML-Agents/`, `Packages/com.unity.ml-agents`

## 13. Existing Licence and Attribution Information
* **Unity Microgame Template**: Subject to Unity Companion License / Unity Asset Store Terms.
* **Unity ML-Agents Toolkit**: Apache License 2.0 (standard upstream license).
* **Root License File**: Not present in repository root.

## 14. Existing .gitignore Coverage
* **Covered Patterns**:
  - Unity temporary directories (`Library/`, `Temp/`, `Obj/`, `Build/`, `Builds/`, `Logs/`, `UserSettings/`, `MemoryCaptures/`)
  - Python virtual environments & caches (`venv/`, `.venv/`, `__pycache__/`, `*.pyc`)
  - Checkpoint and results output folders (`Checkpoints/`, `Results/`, `CheckpointTools/`)
  - Raw CSV metrics (`*.csv`, `ignore.conf`)
  - IDE solution and project metadata (`.vs/`, `.idea/`, `.vscode/`, `*.csproj`, `*.sln`, etc.)
* **Gaps**: `*.onnx` files inside `Assets/` are not explicitly listed in `.gitignore` if tracked.

## 15. Large Files Unsuitable for Direct GitHub Upload
* `Assets/nerf_dartbullet.glb` (~5.2 MB)
* `Checkpoints/` archive copies (already excluded via `.gitignore`)
* Multi-megabyte `.onnx` neural network models in `Results/` (already excluded via `.gitignore`)
