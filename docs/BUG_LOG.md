# Bug & Exception Log

This log records technical issues, runtime exceptions, physics glitches, and engine crashes encountered during research.

---

## Bug Record Template

```markdown
### Bug: [BUG-ID]
* **Bug ID**: BUG-000
* **Date**: YYYY-MM-DD
* **Symptom**: [Observed error message, crash log, or unintended behavior]
* **Reproduction Steps**: [Steps to trigger issue]
* **Suspected Cause**: [Root cause analysis]
* **Fix**: [Code or configuration adjustment]
* **Verification**: [Proof of resolution]
* **Research Impact**: [Impact on experimental validity or data integrity]
```

---

## Historical Resolved Issues Summary

* **BUG-001 (Single-Player Lap Trigger Ignition Failure)**: Resolved in `LapObject.cs` by adding `netObj.IsSpawned` guard before evaluating ownership.
* **BUG-002 (Premature Lap Trigger at Race Countdown)**: Resolved in `LapObject.cs` by adding a 1.5-second level load timer guard (`Time.timeSinceLevelLoad < 1.5f`).
* **BUG-003 (UI NullReferenceException on Lap Completion)**: Resolved in `Objective.cs` and `ObjectiveCompleteLaps.cs` by introducing null-conditional delegate invocations (`TimeDisplay.OnUpdateLap?.Invoke()`).
