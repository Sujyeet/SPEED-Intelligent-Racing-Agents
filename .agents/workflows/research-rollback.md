# /research-rollback

Workflow for restoring previous commits or safe research baselines without silent data loss.

## Instructions
1. **Show Commit History**: Display recent Git commit history (`git log -n 5 --oneline`) and tags.
2. **Identify Target Point**: Present available restore points to the user for selection.
3. **Explain Impact**: Clearly explain which files and changes will be affected by the restore operation.
4. **Safety Tagging**: Create a safety tag (`pre-restore-YYYYMMDD-HHMMSS`) of the current state before executing any restoration.
5. **Request Confirmation**: Ask for explicit user confirmation before modifying working tree files.
6. **Execute Restoration**: Restore files cleanly after receiving confirmation.
7. **Prohibit Destructive Reset**: Never execute destructive `git reset --hard` or `git clean -fd` operations automatically without explicit confirmation.
