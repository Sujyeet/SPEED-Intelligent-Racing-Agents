# kart-git.ps1 — Krash Kart Git Helper Script
# Run this file from the project folder to manage version control easily.
# Usage: Right-click PowerShell > Run as Administrator, then navigate here.

param (
    [Parameter(Position=0)]
    [string]$Action,

    [Parameter(Position=1)]
    [string]$Message = "",

    [Parameter(Position=2)]
    [string]$BranchOrTag = ""
)

$ProjectRoot = $PSScriptRoot
Set-Location $ProjectRoot

function Show-Help {
    Write-Host ""
    Write-Host "=============================================" -ForegroundColor Cyan
    Write-Host "   KRASH KART — GIT CONTROL HELPER" -ForegroundColor Cyan
    Write-Host "=============================================" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "USAGE: .\kart-git.ps1 <action> [message] [branch/tag]" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "EVERYDAY ACTIONS:" -ForegroundColor Green
    Write-Host "  save    [message]       Save your current work with a message"
    Write-Host "  status                  See what files have changed"
    Write-Host "  history                 See the last 15 saves"
    Write-Host ""
    Write-Host "FEATURE BRANCHES (Work in isolation, merge when done):" -ForegroundColor Green
    Write-Host "  new-feature [name]      Start working on a new feature"
    Write-Host "  finish-feature [name]   Merge feature into develop when it works"
    Write-Host "  list-branches           See all branches"
    Write-Host ""
    Write-Host "SNAPSHOTS (Safe rollback points):" -ForegroundColor Green
    Write-Host "  snapshot  [name]        Create a named snapshot of the current state"
    Write-Host "  snapshots               List all named snapshots"
    Write-Host ""
    Write-Host "REVERTING (Go back to a previous version):" -ForegroundColor Green
    Write-Host "  revert-file [filepath]  Undo changes to one specific file"
    Write-Host "  revert-last             Undo the last save (keeps your files safe)"
    Write-Host "  revert-to   [tag/hash]  Jump back to a specific snapshot or save"
    Write-Host ""
    Write-Host "GITHUB SYNC:" -ForegroundColor Green
    Write-Host "  push                    Upload all saves to GitHub"
    Write-Host "  pull                    Download latest from GitHub"
    Write-Host ""
    Write-Host "EXAMPLES:" -ForegroundColor Magenta
    Write-Host '  .\kart-git.ps1 save "add drift boost tier 1"'
    Write-Host '  .\kart-git.ps1 new-feature drift-boost'
    Write-Host '  .\kart-git.ps1 snapshot "before-refactor-kart-physics"'
    Write-Host '  .\kart-git.ps1 revert-to prototype-v1.0'
    Write-Host '  .\kart-git.ps1 revert-file Assets/Karting/Scripts/KartSystems/ArcadeKart.cs'
    Write-Host ""
}

function Git-Save {
    if ($Message -eq "") {
        Write-Host "ERROR: Please provide a save message." -ForegroundColor Red
        Write-Host 'Example: .\kart-git.ps1 save "fixed drift boost timer"'
        return
    }
    git add -A
    git commit -m $Message
    Write-Host ""
    Write-Host "Saved: $Message" -ForegroundColor Green
}

function Git-Status {
    Write-Host ""
    Write-Host "--- Current Branch ---" -ForegroundColor Cyan
    git branch --show-current
    Write-Host ""
    Write-Host "--- Changed Files ---" -ForegroundColor Cyan
    git status -s
    Write-Host ""
}

function Git-History {
    Write-Host ""
    Write-Host "--- Last 15 Saves ---" -ForegroundColor Cyan
    git log --oneline -15 --decorate
    Write-Host ""
}

function Git-NewFeature {
    if ($Message -eq "") {
        Write-Host "ERROR: Provide a feature name." -ForegroundColor Red
        Write-Host 'Example: .\kart-git.ps1 new-feature drift-boost'
        return
    }
    $branch = "feature/$Message"
    git checkout develop
    git checkout -b $branch
    Write-Host ""
    Write-Host "Created and switched to branch: $branch" -ForegroundColor Green
    Write-Host "Work here freely. When the feature is working, run:" -ForegroundColor Yellow
    Write-Host "  .\kart-git.ps1 finish-feature $Message" -ForegroundColor Yellow
    Write-Host ""
}

function Git-FinishFeature {
    if ($Message -eq "") {
        Write-Host "ERROR: Provide the feature name to finish." -ForegroundColor Red
        Write-Host 'Example: .\kart-git.ps1 finish-feature drift-boost'
        return
    }
    $branch = "feature/$Message"
    Write-Host "Merging $branch into develop..." -ForegroundColor Cyan
    git checkout develop
    git merge --no-ff $branch -m "feat: merge $Message into develop"
    Write-Host ""
    Write-Host "Feature merged. Develop branch is now updated." -ForegroundColor Green
    Write-Host "The feature branch '$branch' is kept for reference." -ForegroundColor Yellow
    Write-Host ""
}

function Git-ListBranches {
    Write-Host ""
    Write-Host "--- All Branches ---" -ForegroundColor Cyan
    git branch -a
    Write-Host ""
}

function Git-Snapshot {
    if ($Message -eq "") {
        Write-Host "ERROR: Provide a snapshot name (no spaces, use dashes)." -ForegroundColor Red
        Write-Host 'Example: .\kart-git.ps1 snapshot "before-physics-refactor"'
        return
    }
    $tag = $Message -replace ' ', '-'
    git add -A
    git commit -m "snapshot: $tag" --allow-empty
    git tag -a $tag -m "Manual snapshot: $tag"
    Write-Host ""
    Write-Host "Snapshot created: $tag" -ForegroundColor Green
    Write-Host "To return here later: .\kart-git.ps1 revert-to $tag" -ForegroundColor Yellow
    Write-Host ""
}

function Git-ListSnapshots {
    Write-Host ""
    Write-Host "--- Named Snapshots (Tags) ---" -ForegroundColor Cyan
    git tag -n1
    Write-Host ""
}

function Git-RevertFile {
    if ($Message -eq "") {
        Write-Host "ERROR: Provide the file path to revert." -ForegroundColor Red
        Write-Host 'Example: .\kart-git.ps1 revert-file Assets/Karting/Scripts/AI/KartAgent.cs'
        return
    }
    git checkout HEAD -- $Message
    Write-Host ""
    Write-Host "Reverted: $Message" -ForegroundColor Green
    Write-Host "The file is now back to the last saved version." -ForegroundColor Yellow
    Write-Host ""
}

function Git-RevertLast {
    Write-Host ""
    Write-Host "This will UNDO your last save but keep all your file changes." -ForegroundColor Yellow
    Write-Host "Your files will NOT be lost. The save record is removed." -ForegroundColor Yellow
    $confirm = Read-Host "Type YES to confirm"
    if ($confirm -eq "YES") {
        git reset --soft HEAD~1
        Write-Host "Last save undone. Your file changes are still here." -ForegroundColor Green
    } else {
        Write-Host "Cancelled." -ForegroundColor Gray
    }
    Write-Host ""
}

function Git-RevertTo {
    if ($Message -eq "") {
        Write-Host "ERROR: Provide a tag name or commit hash to go back to." -ForegroundColor Red
        Write-Host 'Example: .\kart-git.ps1 revert-to prototype-v1.0'
        Write-Host 'Example: .\kart-git.ps1 revert-to 59cee33'
        return
    }
    Write-Host ""
    Write-Host "WARNING: This creates a new branch from '$Message'." -ForegroundColor Red
    Write-Host "Your current work is NOT lost — it stays on your current branch." -ForegroundColor Yellow
    $confirm = Read-Host "Type YES to confirm"
    if ($confirm -eq "YES") {
        $restoreBranch = "restore/$Message"
        git checkout -b $restoreBranch $Message
        Write-Host ""
        Write-Host "You are now on branch: $restoreBranch" -ForegroundColor Green
        Write-Host "This is the project state at: $Message" -ForegroundColor Green
        Write-Host "To go back to your work: git checkout develop" -ForegroundColor Yellow
        Write-Host ""
    } else {
        Write-Host "Cancelled." -ForegroundColor Gray
    }
}

function Git-Push {
    $branch = git branch --show-current
    Write-Host ""
    Write-Host "Pushing '$branch' to GitHub..." -ForegroundColor Cyan
    git push origin $branch --follow-tags
    Write-Host ""
    Write-Host "Upload complete." -ForegroundColor Green
    Write-Host ""
}

function Git-Pull {
    $branch = git branch --show-current
    Write-Host ""
    Write-Host "Downloading latest from GitHub ('$branch')..." -ForegroundColor Cyan
    git pull origin $branch
    Write-Host ""
}

# --- Main Router ---
switch ($Action) {
    "save"           { Git-Save }
    "status"         { Git-Status }
    "history"        { Git-History }
    "new-feature"    { Git-NewFeature }
    "finish-feature" { Git-FinishFeature }
    "list-branches"  { Git-ListBranches }
    "snapshot"       { Git-Snapshot }
    "snapshots"      { Git-ListSnapshots }
    "revert-file"    { Git-RevertFile }
    "revert-last"    { Git-RevertLast }
    "revert-to"      { Git-RevertTo }
    "push"           { Git-Push }
    "pull"           { Git-Pull }
    default          { Show-Help }
}
