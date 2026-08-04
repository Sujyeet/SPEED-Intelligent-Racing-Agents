# /experiment-start

Workflow for initializing a new experimental run.

## Instructions
1. Assign a unique Experiment ID (e.g., `EXP-001`).
2. Create an experiment configuration file in `docs/experiments/configs/EXP-001_config.yaml` or update hyperparameters in `config/ppo/`.
3. Confirm independent variables, dependent variables, metrics, random seed, and evaluation criteria with the user.
4. Log the experimental setup template inside `docs/EXPERIMENT_LOG.md`.
5. **Do not run training or evaluation scripts automatically unless explicitly instructed by the user.**
