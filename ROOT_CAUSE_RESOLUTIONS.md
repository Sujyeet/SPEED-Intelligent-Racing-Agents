# RESEARCH_ML Root Cause & Resolution Log

## 1. Single-Player Lap Trigger Ignition Failure

* **Issue Description**: Crossing `StartFinishLine` in single-player or local editor test mode did not register laps or print trigger logs.
* **Root Cause Analysis**: `LapObject.cs` contained the check:
  `if (netObj != null && !netObj.IsOwner) return;`
  In single-player or unspawned editor test mode, `netObj.IsSpawned` is `false`, causing `netObj.IsOwner` to evaluate to `false`. The method returned early on the first line, blocking all lap processing.
* **Resolution Strategy**: Updated the ownership filter in `LapObject.cs`:
  `if (netObj != null && netObj.IsSpawned && !netObj.IsOwner) return;`
* **Verification Method**: Confirmed that unspawned local karts trigger `StartFinishLine` logs and advance lap counters during single-player testing.

## 2. Premature Lap Trigger at Race Countdown

* **Issue Description**: Laps were incrementing to 1 immediately at 0.0 seconds into the race before the player drove forward.
* **Root Cause Analysis**: Karts were instantiated at scene origin directly inside the `StartFinishLine` trigger collider volume before `NetworkedArcadeKart` relocated them to starting grid positions 3 meters behind the line.
* **Resolution Strategy**: Added a level load timer guard in `LapObject.cs`:
  `if (Time.timeSinceLevelLoad < 1.5f) return;`
* **Verification Method**: Verified that initial kart spawn placement during countdown is ignored, and lap 1 only starts when the player accelerates forward across the finish line.

## 3. UI NullReferenceException on Lap Completion

* **Issue Description**: Completing laps threw a `NullReferenceException` in `Objective.cs` and `ObjectiveCompleteLaps.cs`, crashing the script before `WinScene` could load.
* **Root Cause Analysis**: 
  - `Objective.cs` directly called `TimeDisplay.OnUpdateLap()` without checking if `TimeDisplay` was subscribed to by active UI elements.
  - `Objective.CompleteObjective` called `m_ObjectiveHUDManger.UnregisterObjective(this)` without verifying if `m_ObjectiveHUDManger` existed in the scene.
* **Resolution Strategy**:
  - Replaced direct action calls with null-conditional invocations: `TimeDisplay.OnUpdateLap?.Invoke()`.
  - Added null guards in `Objective.cs` for HUD manager references: `if (m_ObjectiveHUDManger != null)`.
  - Reordered `ObjectiveCompleteLaps.cs` to trigger `GameFlowManager.SendMessage("EndGame", true)` before HUD cleanup.
* **Verification Method**: Tested 3-lap completions in scene testing; verified zero console exceptions and clean automatic transition to `WinScene.unity`.

## 4. Inactive GameObject Coroutine Exception

* **Issue Description**: `DisplayMessage.cs` threw `Coroutine couldn't be started because the game object 'WinGameMessage' is inactive!`.
* **Root Cause Analysis**: `Display()` attempted to launch a timing coroutine on a disabled GameObject instance (`WinGameMessage`).
* **Resolution Strategy**: Updated `DisplayMessage.cs` to check `gameObject.activeInHierarchy` and call `gameObject.SetActive(true)` prior to invoking `StartCoroutine`.
* **Verification Method**: Confirmed victory notification messages display on screen without coroutine initialization errors.

## 5. Relay Re-Authentication Exception on Scene Reload

* **Issue Description**: Reloading the scene or re-hosting threw `[Relay] Failed to initialize: Invalid state for this operation. The player is already signed in.`.
* **Root Cause Analysis**: `RelayManager.cs` called `AuthenticationService.Instance.SignInAnonymouslyAsync()` unconditionally on `Start()`.
* **Resolution Strategy**: Wrapped authentication with `if (!AuthenticationService.Instance.IsSignedIn)` check.
* **Verification Method**: Verified seamless hosting and scene reloads without authentication state exceptions.

## 6. InputStruct Null Check Compiler Error

* **Issue Description**: `KartAnimation.cs` failed compilation with `error CS0019: Operator '==' cannot be applied to operands of type 'InputData' and '<null>'`.
* **Root Cause Analysis**: `InputData` is a value-type C# struct on `ArcadeKart`, making `kartController.Input == null` invalid syntax.
* **Resolution Strategy**: Updated line 56 in `KartAnimation.cs` to check `if (kartController == null) return;`.
* **Verification Method**: Verified error-free compilation in Unity Editor.
