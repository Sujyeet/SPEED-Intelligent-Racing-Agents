# Project Overview

## SPEED: Intelligent Racing Agents

### Research Objective

This project investigates the development and evaluation of intelligent racing agents using Deep Reinforcement Learning (DRL) within the Unity ML-Agents framework. The primary research question explores how different agent architectures like heuristic, deep reinforcement learning, and human control perform in autonomous racing scenarios, with emphasis on both performance metrics and human-like behavioral characteristics.

### Key Research Questions

- **Performance Comparison**: How do DRL agents compare against traditional heuristic approaches and human players in terms of lap times, collision rates, and consistency?
- **Human-like Behavior**: Can DRL agents be trained to exhibit natural, human-like driving patterns while maintaining competitive performance?
- **Reward Shaping Impact**: What effect do different reward function designs have on agent learning stability and behavioral outcomes?
- **Generalization Capability**: How well do trained agents adapt to novel track configurations and racing scenarios?

### Methodology Overview

#### Agent Types Evaluated

1. **Heuristic Agent**
   - Rule-based decision making
   - Deterministic trajectory planning
   - Baseline for performance comparison

2. **DRL Agent (PPO-based)**
   - Neural network policy learning
   - Continuous action space control
   - Reward-driven optimization

3. **Human Control Agent**
   - Manual keyboard/controller input
   - Reference standard for natural behavior
   - Ground truth for human-likeness metrics

#### Evaluation Metrics

- **Performance**: Mean lap time, standard deviation, completion rate
- **Safety**: Collision frequency and severity analysis
- **Behavioral**: Path deviation, steering smoothness, human-likeness scoring
- **Consistency**: Episode-to-episode variance in performance

### Technical Architecture

#### Environment
- **Platform**: Unity 2022.3 with ML-Agents Release 20
- **Physics**: Realistic vehicle dynamics and collision detection
- **Track Design**: Multiple configurations for training and evaluation
- **Observation Space**: 14-dimensional state vector (velocity, position, track sensors)
- **Action Space**: Continuous control (steering, acceleration, braking)

#### Training Infrastructure
- **Algorithm**: Proximal Policy Optimization (PPO)
- **Network Architecture**: Lightweight feedforward neural networks
- **Training Duration**: 1M+ timesteps with early stopping
- **Evaluation Protocol**: 100 episodes per agent configuration

### Key Findings Summary

#### Performance Results
- DRL agents achieve competitive lap times (39.8s ± 1.4s) compared to human players (44.2s ± 2.8s)
- Heuristic agents provide consistent but slower performance (41.5s ± 2.0s)
- Collision rates: DRL < 2%, Human ~3%, Heuristic ~0% (but slower)

#### Behavioral Insights
- Simplified reward functions lead to more stable training and human-like behavior
- Over-complex reward structures cause training instability and unnatural driving patterns
- DRL agents can learn smooth, adaptive driving strategies that closely mimic human behavior

#### Design Lessons
- Lightweight network architectures outperform complex deep networks for this domain
- Targeted reward shaping is more effective than comprehensive multi-term rewards
- Fixed random seeds and controlled evaluation protocols ensure reproducible results

### Research Contributions

1. **Empirical Analysis**: Comprehensive comparison of agent architectures in realistic racing simulation
2. **Behavioral Modeling**: Quantitative framework for measuring human-likeness in autonomous agents
3. **Reproducible Research**: Complete codebase, data, and evaluation protocols for replication
4. **Practical Insights**: Design guidelines for training stable, human-like racing agents

### Applications and Future Work

#### Immediate Applications
- Autonomous vehicle testing and validation
- Game AI development for racing simulations
- Human-robot interaction research
- Driver assistance system prototyping

#### Future Research Directions
- Multi-agent racing scenarios and competition
- Transfer learning across different vehicle types and track configurations
- Integration with real-world autonomous vehicle systems
- Advanced behavioral modeling using inverse reinforcement learning

### Repository Structure

This project is organized for maximum accessibility and reproducibility:

```
SPEED-Intelligent-Racing-Agents/
├── Documentation/          # Complete project documentation
├── DRL-Agent-Project/     # Deep RL implementation and models
├── Heuristic-Agent-Project/ # Rule-based agent implementation
├── Results-and-Analysis/  # Experimental data and analysis
├── requirements.txt       # Python dependencies
└── README.md             # Quick start guide
```

### Getting Started

1. **Quick Setup**: Follow `Documentation/installation.md` for environment setup
2. **Agent Evaluation**: Use `Documentation/experiment-guide.md` for running experiments
3. **Data Analysis**: See `Documentation/analysis-guide.md` for result interpretation
4. **Reproduction**: Complete instructions in `Documentation/reproducibility.md`

### Citation

If this work contributes to your research, please cite:

```bibtex
@mastersthesis{sujyeet2025speed,
  title={SPEED: Intelligent Racing Agents Using Deep Reinforcement Learning and Unity ML-Agents},
  author={Sujyeet},
  year={2025},
  school={Queen Mary University of London},
  type={MSc Thesis}
}
```

---

**Last Updated**: August 2025  
**Version**: 1.0  
**Maintainer**: Sujyeet  
**License**: MIT
