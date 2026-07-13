# SPEED — Smart Player Engaging Environment using Deep Reinforcement Learning

SPEED is a research-focused autonomous racing project that compares classical AI with modern Deep Reinforcement Learning (DRL) in Unity. The repository implements and benchmarks two distinct approaches: a deterministic waypoint-based controller and a DRL agent trained using Unity ML-Agents with the PPO algorithm.

Designed with reproducibility and open science in mind, SPEED provides comprehensive documentation, rigorous benchmarking, and systematic evaluation to highlight the strengths and trade-offs of rule-based and learning-based autonomous driving techniques.

## Playtest Prototype
Try the interactive playtest prototype on Itch.io:  **[Play SPEED Racing Agents Demo](https://sujitvarma.itch.io/ml-agent-playtest-prototype)**

Experience the Enhanced Natural DRL agent in action, compare its behavior with your gameplay, and see the research findings firsthand.

## Agent Demonstration Videos
### Side-by-Side Agent Comparison
| Heuristic Agent | Natural DRL Agent |
|---|---|
| ![Heuristic Agent Demo](Results-and-Analysis/HEU.gif) | ![DRL Agent Demo](Results-and-Analysis/DRL.gif) |
| **Characteristics:** | **Characteristics:** |
| • Precise waypoint following | • Adaptive behavior learning |
| • Deterministic responses | • Human-like smoothness |
| • Collision-free navigation | • Dynamic decision making |

## Project Motivation
Autonomous racing is a prime testbed for intelligent agent research. It requires tight integration of perception, sequential policy, real-time optimization, and adaptability to diverse environmental conditions. This project addresses critical open questions:

• **When and where** do engineered heuristic controllers outperform or underperform RL agents?
• **How does** reward shaping and neural architecture affect RL agent learning, stability, and generalization?
• **Can** DRL agents demonstrate truly human-like behavior, as measured by both efficiency and naturalness?

## Summary of Contributions
• **Heuristic Controller**: Fully deterministic, high performance waypoint follower. Adaptively controls speed and heading for efficient, collision-free lap completion. All parameters and logs are documented for reproducibility.
• **DRL Racing Agent**: PPO-based agent using raycast-derived sensor input, curriculum learning, and custom reward structure. Multiple ablation studies highlight the trade-offs in architectural depth, reward complexity, and agent robustness.
• **Human Benchmark Suite**: Tracks and scenes support human demos for direct comparison like lap times, steering smoothness, and behavioral metrics.
• **Open Experimental Pipeline**: Source code, configuration files, trained models, raw logs, and exhaustive documentation are provided to enable full reproduction of all reported findings.

## Repository Structure
```
SPEED-Intelligent-Racing-Agents/
├── Heuristic-Agent-Project/     # Deterministic controller (Unity 2021.3.45f1)
│   ├── Assets/
│   ├── ProjectSettings/
│   └── Packages/
├── DRL-Agent-Project/           # PPO agent, custom reward, evaluation scenes
│   ├── Assets/
│   ├── ProjectSettings/
│   ├── Packages/
│   └── Trained-Models/
├── Results-and-Analysis/        # Performance logs, comparison data, CSVs
├── Documentation/               # Research paper, user/setup guides
│   ├── SPEED-Dissertation.pdf
│   └── Setup-Instructions.md
└── README.md
```

## Technical Stack
• **Game Engine**: Unity 2021.3.45f1 LTS
• **ML Toolkit**: Unity ML-Agents 0.30.0 (PPO algorithm)
• **Programming**: C# (Unity scripting), Python 3.8+
• **Hardware**: 8GB+ RAM, CUDA GPU recommended for DRL
• **Visualization**: Unity scene GUI, TensorBoard, custom plots
• **Versioning**: Git, atomic commits for all experiments

## Setup and Reproduction
### Prerequisites
• Unity 2021.3.45f1 LTS (mandatory version)
• Python 3.8+ with pip
• ML-Agents 0.30.0 (`pip install mlagents==0.30.0`)

### Installation Steps
1. **Clone the repository**:
```bash
git clone https://github.com/Sujyeet/SPEED-Intelligent-Racing-Agents.git
cd SPEED-Intelligent-Racing-Agents
```

2. **Open projects in Unity Hub**: select either `Heuristic-Agent-Project` or `DRL-Agent-Project`.
   • Only the folders `Assets`, `Packages`, and `ProjectSettings` are strictly required.
   • On first open, import will take several minutes due to dependency resolution.

3. **(Optional) Set up a Python environment for training**:
```bash
python -m venv racing_env
# Windows: racing_env\Scripts\activate
# Mac/Linux: source racing_env/bin/activate
pip install -r requirements.txt
```

## Running and Evaluation
### Heuristic Agent
• Load `Heuristic-Agent-Project` in Unity
• Select a race scene (e.g., `Assets/Karting/Scenes/MainScene.unity`)
• Press Play to run the agent; logs are saved for every episode (Paste the desired output directory in the editor)

### DRL Agent (PPO)
• Open `DRL-Agent-Project` in Unity, ensure ML-Agents is present
• Assign any `.onnx` model from `Trained-Models/` to the kart agent in test scenes
• Set Behavior Type to "Inference Only" and press Play to visualize evaluation
• **Retraining**: See `Documentation/Setup-Instructions.md` for hyperparameters, scripts, and training protocol

### Human Benchmark
• Scenes allowing manual play (keyboard/controller) are included, with instructions in the UI
• Run with identical evaluation pipeline to ensure compatibility of all recorded metrics

## Methodology and Experiments
### Controlled Testing
• Evaluation on fixed-seed, multi-lap racing tracks for statistical robustness
• Lap time, completion rate, collisions, steering profiles, and path deviation are recorded per episode

### Ablation Studies
• **Reward structure**: From complex (multiple terms) to minimal (progress, smoothness, speed)
• **Network architecture**: 3-layer/256 vs. 2-layer/128 PPO; training stability and convergence tracked
• **Simulation time scale & curriculum**: Real-time vs 10x acceleration; effect on sample efficiency
• **Generalization**: Evaluations on alternate tracks for out-of-distribution robustness
• **Result**: Simpler reward and lightweight networks consistently improved DRL reliability. See main paper for summary tables (e.g., Table IV, Table V) and cumulative reward learning curves.

### Performance Metrics (example table)
| Agent | Mean Lap Time (s) | Std Dev | Collision Rate | Human-Likeness |
|-------|-------------------|---------|----------------|-----------------|
| Heuristic Agent | 41.5 | 2.0 | 0% | Low |
| PPO DRL Agent (Baseline) | 39.8 | 1.4 | <2% | High |
| DRL Agent (Enhanced Humanlike) | 43.0 | 1.1 | <1% | Very High |
| Human Player | 44.2 | 2.8 | ~3% | Baseline |

### Behavioral Analysis
Beyond speed, the work quantifies "natural" behavior:
• **Path deviation and steering variance**—plotted time series show DRL agents closely track human smoothness, unlike the mechanical, deterministic baseline
• **Error and recovery**: Analysis of wall-collisions and recovery strategies
• **Comprehensive logging**: All logs available for reproduction in `Results-and-Analysis/`

### Key Lessons
• Over-complexity (deep networks, multi-term rewards) led to instability and slow convergence
• Well-tuned heuristics provide reliability but limited adaptability to novel track layouts
• Simplified DRL with targeted reward shaping yields both superior lap times and higher human-likeness scores in controlled experiments

## Full Reproducibility Commitment
• All experiments use documented seeds and config files; every result in the paper can be regenerated from scripts in this repository
• **Paper draft**: `Documentation/SPEED-Dissertation.pdf` (complete analyses, figures, and extended results)
• **Installation and reproducibility guide**: `Documentation/Setup-Instructions.md` (step-by-step for every OS)

## Collaboration & Academic Use
• **Academic collaboration**: Designed for MSc and PhD research use; contact author for dataset sharing, integration studies, or advanced experiments
• **Contributing**: Issue tracker and pull requests are welcome for reproducibility, cross-validation, or improved agent design studies



## Author & Contact
• **Sujit G** (MSc Autonomous Racing, [sujitvarma70@gmail.com])
• Queen Mary University of London (for academic coordination)
• For technical questions, open a GitHub Issue or contact by email

## License
All project source, code, models, data, and documentation are released under the MIT License. See LICENSE for details. Intended for research, education, and open collaboration.
