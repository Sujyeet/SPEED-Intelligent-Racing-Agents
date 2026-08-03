# Krash Kart — Git Version Control Guide

## The Short Version (Read This First)

You have two permanent branches:

| Branch | Purpose |
| :--- | :--- |
| `main` | Stable, tested, releasable code only. Never touch directly. |
| `develop` | Active development. All day-to-day work goes here. |

When you start a new feature, you branch off `develop`, work freely, then merge back when it is working. If anything breaks, you can always revert.

You are currently on: **`develop`**

---

## The Helper Script

All Git operations are wrapped into a single PowerShell script: `kart-git.ps1`

Open a PowerShell terminal in your project folder and run:

```powershell
.\kart-git.ps1
```

This prints all available commands with examples.

---

## Daily Workflow

### 1. Save Your Progress
Run this whenever you reach a working state (like a manual save point in a game):

```powershell
.\kart-git.ps1 save "describe what you just did"
```

Examples:
```powershell
.\kart-git.ps1 save "add drift boost timer to ArcadeKart"
.\kart-git.ps1 save "fix wheel spin animation on left side"
.\kart-git.ps1 save "connect item pickup to spell system"
```

---

### 2. Check What Has Changed

```powershell
.\kart-git.ps1 status
```

Shows which files you have modified since the last save.

---

### 3. See Your Save History

```powershell
.\kart-git.ps1 history
```

Prints the last 15 saves with their ID codes and messages.

---

## Working on a New Feature

When starting a new feature that might break things, always create a feature branch:

```powershell
# Step 1: Start a new isolated workspace for the feature
.\kart-git.ps1 new-feature drift-boost

# ... do your work, make saves normally ...
.\kart-git.ps1 save "add tier 1 drift accumulation"
.\kart-git.ps1 save "add boost velocity impulse on release"

# Step 2: When the feature is working, merge it back
.\kart-git.ps1 finish-feature drift-boost
```

You are now safely back on `develop` with the feature included. If the feature broke everything, you simply delete the feature branch and nothing in `develop` is affected.

---

## Creating Named Snapshots

A snapshot is like a named bookmark you can jump back to at any time. Create one before any risky change:

```powershell
.\kart-git.ps1 snapshot before-physics-refactor
```

List all your snapshots:
```powershell
.\kart-git.ps1 snapshots
```

The existing snapshot from the completed prototype is: `prototype-v1.0`

---

## Reverting (Going Back)

### Option A: Undo Changes to One File Only

If you broke a single script and want it back to the last saved version:

```powershell
.\kart-git.ps1 revert-file Assets/Karting/Scripts/AI/KartAgent.cs
```

This does NOT affect any other file.

---

### Option B: Undo the Last Save

If your last save was a mistake and you want to remove it (but keep the file changes on disk):

```powershell
.\kart-git.ps1 revert-last
```

You will be asked to type YES to confirm. Your files are NOT deleted.

---

### Option C: Jump Back to a Snapshot or Old Save

This is the nuclear option for when something is completely broken and unfixable. It opens the project at an older state in a new branch, leaving your current work untouched:

```powershell
# Go back to the prototype snapshot
.\kart-git.ps1 revert-to prototype-v1.0

# Go back to any save by its ID (from the history command)
.\kart-git.ps1 revert-to 59cee33
```

You are now viewing the old version. To go back to your current work:

```powershell
git checkout develop
```

---

## Syncing With GitHub

Upload all local saves to GitHub:

```powershell
.\kart-git.ps1 push
```

Download any changes from GitHub (if working across machines):

```powershell
.\kart-git.ps1 pull
```

---

## Branch Map (Visual Overview)

```
main
 |
 |--- prototype-v1.0 (TAG — stable rollback point)
 |
develop  <-- you are here, all active work
 |
 |--- feature/drift-boost        (example feature in progress)
 |--- feature/garage-system      (example feature in progress)
 |--- restore/prototype-v1.0     (created only if you use revert-to)
```

---

## Rules to Remember

1. Never commit directly to `main`. It is a deployment branch only.
2. Always create a feature branch before starting a new Phase feature.
3. Always create a snapshot before any major refactor.
4. Commit messages should describe what changed, not what you did (e.g. "add drift boost" not "worked on kart").

---

## Quick Reference Card

| What You Want To Do | Command |
| :--- | :--- |
| Save current work | `.\kart-git.ps1 save "message"` |
| See what changed | `.\kart-git.ps1 status` |
| See save history | `.\kart-git.ps1 history` |
| Start a new feature | `.\kart-git.ps1 new-feature name` |
| Finish and merge a feature | `.\kart-git.ps1 finish-feature name` |
| Create a named snapshot | `.\kart-git.ps1 snapshot name` |
| List all snapshots | `.\kart-git.ps1 snapshots` |
| Undo one file | `.\kart-git.ps1 revert-file path/to/file` |
| Undo last save (keep files) | `.\kart-git.ps1 revert-last` |
| Jump to old snapshot | `.\kart-git.ps1 revert-to tag-or-id` |
| Upload to GitHub | `.\kart-git.ps1 push` |
| Download from GitHub | `.\kart-git.ps1 pull` |
