# Unity Checkpoint Tools

Simple PowerShell scripts to **backup** and **restore** your Unity project state.  
Perfect for: saving working states before risky changes, comparing AI agent versions, experimenting safely.

---

## 📁 Structure

```
YourProject/
├── Assets/
├── ProjectSettings/
├── Packages/
├── Checkpoints/              ← Created automatically
│   ├── checkpoint_20250115_143022_working-ai/
│   │   ├── Assets/
│   │   ├── ProjectSettings/
│   │   └── Packages/
│   └── checkpoint_20250115_164500_multiplayer-test/
└── CheckpointTools/          ← These scripts
    ├── Save-Checkpoint.ps1
    ├── Load-Checkpoint.ps1
    └── List-Checkpoints.ps1
```

---

## 🚀 Quick Start

### 1. Open PowerShell in your Unity project folder
```powershell
cd "C:\Unity Projects\ML TEST\ML TEST"
```

### 2. Save a checkpoint (before making changes)
```powershell
.\CheckpointTools\Save-Checkpoint.ps1 -Name "working-ai"
```
Creates: `Checkpoints/checkpoint_20250115_143022_working-ai/`

### 3. Make your changes (edit scripts, test AI, break things)

### 4. If broken → Restore instantly
```powershell
.\CheckpointTools\Load-Checkpoint.ps1
```
Select the checkpoint number (0 = latest) → Type `YES` → Done.

---

## 📋 Commands

### Save-Checkpoint.ps1
```powershell
# Interactive (prompts for name)
.\CheckpointTools\Save-Checkpoint.ps1

# With custom name
.\CheckpointTools\Save-Checkpoint.ps1 -Name "before-refactor"

# Quick save (auto-name with timestamp)
.\CheckpointTools\Save-Checkpoint.ps1 -Quick
```

**What gets backed up:**
- `Assets/` (scripts, scenes, prefabs, materials, etc.)
- `ProjectSettings/` (physics, tags, layers, quality, ML-Agents config)
- `Packages/manifest.json` + `packages-lock.json`
- **Excludes:** `Library/`, `Temp/`, `Logs/`, `Builds/`, `Checkpoints/`, `.git/`, `obj/`, `bin/`, `__pycache__/`

---

### Load-Checkpoint.ps1
```powershell
# Interactive menu (shows all checkpoints with dates/sizes)
.\CheckpointTools\Load-Checkpoint.ps1

# Restore specific checkpoint by name
.\CheckpointTools\Load-Checkpoint.ps1 -Name "checkpoint_20250115_143022_working-ai"

# Restore by index (0 = newest)
.\CheckpointTools\Load-Checkpoint.ps1 -Index 0

# Just list, don't restore
.\CheckpointTools\Load-Checkpoint.ps1 -List

# Skip confirmation (for automation)
.\CheckpointTools\Load-Checkpoint.ps1 -Index 0 -Force
```

**Safety features:**
- Creates **emergency backup** of current state before restoring
- Shows exactly what will be replaced
- Requires typing `YES` to confirm (unless `-Force`)
- Tells you how to undo if something goes wrong

---

### List-Checkpoints.ps1
```powershell
# Show all checkpoints
.\CheckpointTools\List-Checkpoints.ps1

# Show last 5 only
.\CheckpointTools\List-Checkpoints.ps1 -Last 5

# Include folder sizes (slower)
.\CheckpointTools\List-Checkpoints.ps1 -Size
```

---

## 💡 Typical Workflows

### AI Training Experiments
```powershell
# 1. Save baseline
.\Save-Checkpoint.ps1 -Name "baseline-ppo-500k"

# 2. Try new reward function
# ... edit KartAgent.cs ...

# 3. If worse, restore in 10 seconds
.\Load-Checkpoint.ps1 -Index 0

# 4. If better, save new baseline
.\Save-Checkpoint.ps1 -Name "better-reward-v2"
```

### Multiplayer Testing
```powershell
# Before changing NetworkedArcadeKart.cs
.\Save-Checkpoint.ps1 -Name "mp-working-sync"

# Test new interpolation...
# If desync happens:
.\Load-Checkpoint.ps1 -Name "checkpoint_..._mp-working-sync"
```

### Daily Checkpoints
```powershell
# End of day
.\Save-Checkpoint.ps1 -Name "eod-$(Get-Date -Format 'yyyyMMdd')"
```

---

## ⚙️ Requirements

- **Windows** with PowerShell 5.1+ (built into Windows 10/11)
- **robocopy** (built into Windows)
- Run from **project root** (where `Assets/` folder is)

---

## 🔧 Customization

Edit `Save-Checkpoint.ps1` to change:

```powershell
# Folders to backup (add/remove)
$FoldersToBackup = @("Assets", "ProjectSettings", "Packages")

# Folders to exclude (add your own)
$ExcludeDirs = @("Library", "Temp", "Logs", "Builds", "Checkpoints", ".git", "obj", "bin", "__pycache__", "node_modules")
```

---

## ⚠️ Notes

| Scenario | What Happens |
|----------|--------------|
| Unity open during restore | **Close Unity first** — it locks files |
| Restore seems stuck | Check Task Manager for `robocopy` — large projects take 10-30s |
| Assets missing after restore | Delete `Library/` folder and reopen Unity (reimports) |
| Want to delete old checkpoints | Safe to delete folders in `Checkpoints/` manually |

---

## 🆘 Emergency Recovery

If something goes **really wrong**:

1. Your last safety backup is at:  
   `Checkpoints/emergency_backup_YYYYMMDD_HHMMSS/`
2. Run:  
   `.\Load-Checkpoint.ps1 -Name "emergency_backup_..."`
3. Or manually copy folders back from that directory

---

## 📝 License

MIT — Use freely in your Unity projects.