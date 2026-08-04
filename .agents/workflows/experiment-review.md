# /experiment-review

Workflow for reviewing completed experimental results and verifying research validity.

## Instructions
1. Check reproducibility (verify random seed logging, git commit hash, and configuration file stability).
2. Check baseline comparison (verify that metrics are evaluated against baseline heuristic AI and prior checkpoint runs).
3. Check for confounded variables (ensure multiple experimental variables were not changed simultaneously).
4. Analyze metrics (mean lap time, completion rate, collision frequency, cumulative reward).
5. Verify conclusions (ensure stated findings do not exceed empirical evidence).
6. Update `docs/EXPERIMENT_LOG.md` and `docs/experiments/results/` with verified findings.
