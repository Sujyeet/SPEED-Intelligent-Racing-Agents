# Research Questions — Proposed Agenda

> [!NOTE]
> The questions, hypotheses, and variables documented below represent provisional research inquiries to guide experimental design. They are not presented as proven findings or verified outcomes.

---

## Primary Research Question
**Can a racing NPC maintain effective and believable driving behavior in an expanded game environment containing player-induced disturbances?**

### 1. Motivation
Standard RL racing agents are often trained in static environments without active human disruption. In multi-agent or human-in-the-loop racing, player actions (e.g., collisions, projectile attacks, trap deployment, line blocking) introduce dynamic non-stationarity. Evaluating whether an agent maintains performance and human-like driving quality under disturbance is critical for believable game AI.

### 2. Hypotheses
* **H1_Primary**: An ML-Agent trained with dynamic disturbance Curriculum Learning will maintain >80% lap completion rate and lower trajectory variance when subjected to player attacks compared to an agent trained purely in static solo environments.

### 3. Independent Variables
* Disturbance type (No disturbance, static obstacles, dynamic player collision, projectile/mine attacks).
* Agent training protocol (Static single-agent training vs. Multi-agent dynamic disturbance training).

### 4. Dependent Variables
* Lap completion rate (%).
* Average lap time (seconds).
* Collision frequency (hits per lap).
* Trajectory deviation / smooth driving cost (variance from optimal racing line).

### 5. Baselines
* Heuristic waypoint-following AI (default Unity Karting Microgame AI).
* PPO Agent trained strictly in solo, disturbance-free track environments (`prototype-v1.0`).

### 6. Required Data
* Per-frame agent position $(x, y, z)$, velocity, and heading.
* Per-frame distance to track centerline and nearest opponent.
* Episode termination reasons (lap finish, track boundary timeout, collision reset).

### 7. Metrics
* Mean Lap Time (MLT)
* Off-track Frequency (OTF)
* Inter-agent Collision Rate (CR)
* Reward Curve Convergence Rate

### 8. Assumptions
* Physics calculations and raycast sensor readings remain deterministic across identical seeds.
* Arcade kart physics parameters (acceleration, top speed, steering torque) are fixed during comparison runs.

### 9. Threats to Validity
* **Internal**: Stochasticity in physics frame rate or unseeded random spell spawns.
* **External**: Performance on one track layout (e.g., `MainScene`) may not generalize to highly technical or narrow track layouts.

### 10. Supporting Evidence Criteria
* Statistically significant retention of completion rates and competitive lap times under active player disturbances without catastrophic policy failure.

### 11. Weakening / Rejecting Evidence Criteria
* High rate of policy spinning, wall-scraping, or freezing when hit by projectiles or collided with by human players.

---

## Secondary Research Question
**Can an NPC select an effective spell combination based on map type, available spells, opponent spell selections, and spell synergies instead of choosing randomly?**

### 1. Motivation
Random item usage by racing bots reduces tactical depth and predictable challenge in item-based racing games. Evaluating intelligent spell selection based on game state context (track layout, opponent proximity, inventory, and item synergies) can improve strategic agent capability.

### 2. Hypotheses
* **H1_Secondary**: A context-aware spell selection model (discrete action policy conditioned on game state) will achieve higher win rates and effective item hit rates compared to a uniform random item activation baseline.

### 3. Independent Variables
* Item selection strategy (Uniform random trigger, Rule-based heuristic decision tree, Context-conditioned RL policy).
* Game state context parameters (Opponent distance, opponent spell state, track section curvature, lead status).

### 4. Dependent Variables
* Win rate (%) across 50-race evaluation batches.
* Effective item hit rate (ratio of successful projectile hits / mine traps to total items used).
* Average position advancement per item deployment.

### 5. Baselines
* Random Item Activation Baseline (triggers items immediately upon pickup).
* Rule-based Heuristic Item Agent (triggers offensive items when opponent is within forward cone; defensive items when targeted).

### 6. Required Data
* Item pickup events, deployment timestamps, target relative positions, item outcome (hit/miss/wasted).
* End-of-race finish order and time deltas.

### 7. Metrics
* Item Efficiency Ratio (IER = Hits / Deployments)
* Position Delta per Item (PDI)
* Overall Win Rate (WR)

### 8. Assumptions
* Item pickup spawns occur at fixed interval locations along the track.
* All agents operate with equal vehicle speed limits and physics attributes.

### 9. Threats to Validity
* **Internal**: Confounding effect if item selection and driving steering policy are modified simultaneously.
* **External**: Fixed spell types (projectiles, mines) may not represent highly complex modular magic systems.

### 10. Supporting Evidence Criteria
* Higher win rate and significantly improved item hit efficiency for context-aware selection over random deployment.

### 11. Weakening / Rejecting Evidence Criteria
* Negligible difference in win rate or hit efficiency between random item usage and context-conditioned selection.
