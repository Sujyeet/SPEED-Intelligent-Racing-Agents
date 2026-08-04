# Experiment Log

This log tracks all formal training and evaluation experiments conducted within the `SPEED-Intelligent-Racing-Agents` research environment.

---

## Reusable Experiment Log Template

Copy and paste the template block below for each new experimental run.

```markdown
### Experiment Log: [EXP-ID]

* **Experiment ID**: EXP-000
* **Date & Time**: YYYY-MM-DD THH:MM:SS
* **Research Question**: [Primary / Secondary / Custom]
* **Hypothesis**: [Brief statement of hypothesis]

#### Environment & Build Context
* **Git Commit Hash**: [Commit Hash]
* **Unity Version**: 2022.3.62f2
* **ML-Agents Version**: v2.0.1
* **Scene Name**: [e.g., MainScene.unity / KartClassic_Training.unity]
* **Track Layout**: [e.g., Main Loop / Complex Oval / Obstacle Track]
* **Kart Configuration**: [Default ArcadeKart stats / Top Speed / Torque]

#### Model & Policy Parameters
* **Agent Checkpoint**: [e.g., Results/ppo/ArcadeDriver.onnx / None (Training from scratch)]
* **Observation Space Configuration**: [Raycast count, vector observations, velocity inputs]
* **Action Space Configuration**: [Continuous steering/throttle, Discrete item triggers]
* **Reward Structure**: [Checkpoint progress reward, time penalty, wall collision penalty]
* **Player Mechanics State**: [Enabled / Disabled / Bot-driven]
* **Opponent Configuration**: [Solo / 1 Human / N Bot Agents]

#### Execution Parameters
* **Training Steps**: [e.g., 500,000 steps]
* **Random Seed**: [e.g., 42]
* **Evaluation Runs**: [e.g., 20 episodes]

#### Data & Results
* **Metrics**: 
  - Mean Lap Time: [X.XX seconds]
  - Lap Completion Rate: [XX%]
  - Total Reward Mean: [X.XX]
* **Raw Data Location**: `research/raw_data/EXP-000/`
* **Quantitative Results**: [Summary of empirical outcomes]
* **Unexpected Behavior**: [Any unexpected agent maneuvers or instabilities]
* **Limitations**: [Observed boundary conditions or unhandled cases]
* **Interpretation**: [Evaluation against initial hypothesis]
* **Next Action**: [Recommended follow-up step]
```

---

## Log History

*(No formal experiments executed yet. Template initialized for Phase 2 setup.)*
