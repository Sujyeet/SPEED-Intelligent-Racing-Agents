# /research-save

Workflow for safe local checkpointing of research changes.

## Instructions
1. **Review Status**: Run `git status` and display current working tree state and all modified/untracked files.
2. **Safety Scan**: Scan modified files for accidental secrets, API tokens, passwords, private user paths, or large binary artifacts (`.glb`, temporary caches).
3. **Documentation Sync**: Verify that relevant research logs (`docs/CHANGELOG.md`, `docs/DECISION_LOG.md`, `docs/EXPERIMENT_LOG.md`) are updated with recent session changes.
4. **Validation**: Perform safe validation checks (verify file compilation / syntax validity).
5. **Approval Request**: Ask the user for explicit approval before creating a local Git commit.
6. **Commit Creation**: Upon approval, create a local Git checkpoint commit with a descriptive message (e.g., `research: checkpoint before experiment EXP-001`).
7. **Report Outcome**: Report the resulting Git commit hash to the user and explain how to restore or reference this checkpoint.
8. **Remote Push Restriction**: Never automatically push commits to GitHub without explicit user permission.
