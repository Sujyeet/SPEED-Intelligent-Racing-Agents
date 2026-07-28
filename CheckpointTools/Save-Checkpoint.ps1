<#
.SYNOPSIS
    Creates a timestamped checkpoint (backup) of your Unity project.
.DESCRIPTION
    Backs up Assets/ and ProjectSettings/ folders to a timestamped folder.
    Excludes Library/, Temp/, Logs/, obj/, bin/, .vs/, Builds/, and Checkpoints/.
.USAGE
    .\Save-Checkpoint.ps1
    .\Save-Checkpoint.ps1 -Name "before-multiplayer-refactor"
    .\Save-Checkpoint.ps1 -Name "ai-training-working" -Keep 10
.PARAMETER Name
    Optional label to identify this checkpoint.
.PARAMETER Keep
    Maximum number of checkpoints to keep (oldest deleted first). Default: 20
.PARAMETER ProjectPath
    Path to Unity project. Default: current directory
#>

param(
    [Parameter(Mandatory=$false)]
    [string]$Name = "",

    [Parameter(Mandatory=$false)]
    [int]$Keep = 20,

    [Parameter(Mandatory=$false)]
    [string]$ProjectPath = "."
)

$ProjectRoot = Resolve-Path $ProjectPath
$CheckpointDir = Join-Path $ProjectRoot "Checkpoints"
$AssetsDir = Join-Path $ProjectRoot "Assets"
$SettingsDir = Join-Path $ProjectRoot "ProjectSettings"
$PackagesDir = Join-Path $ProjectRoot "Packages"

if (-not (Test-Path $CheckpointDir)) {
    New-Item -ItemType Directory -Path $CheckpointDir -Force | Out-Null
}

$Timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
$LabelPart = if ($Name) { "_$Name" } else { "" }
$CheckpointName = "checkpoint_$Timestamp$LabelPart"
$CheckpointPath = Join-Path $CheckpointDir $CheckpointName

Write-Host "Creating checkpoint: $CheckpointName" -ForegroundColor Cyan
Write-Host "   Source: $ProjectRoot" -ForegroundColor Gray
Write-Host "   Destination: $CheckpointPath" -ForegroundColor Gray

$FoldersToBackup = @()
if (Test-Path $AssetsDir) { $FoldersToBackup += $AssetsDir }
if (Test-Path $SettingsDir) { $FoldersToBackup += $SettingsDir }
if (Test-Path $PackagesDir) { $FoldersToBackup += $PackagesDir }

$FilesToBackup = @(
    "Packages/manifest.json",
    "Packages/packages-lock.json",
    "ProjectSettings/ProjectVersion.txt",
    "ProjectSettings/ProjectSettings.asset"
) | Where-Object { Test-Path (Join-Path $ProjectRoot $_) }

New-Item -ItemType Directory -Path $CheckpointPath -Force | Out-Null

$excludeDirs = @("Library", "Temp", "Logs", "obj", "bin", ".vs", "Builds", "Checkpoints", ".git", "node_modules", "__pycache__")
$excludeArgs = ""
foreach ($dir in $excludeDirs) { $excludeArgs += " /XD $dir" }

foreach ($srcFolder in $FoldersToBackup) {
    $folderName = Split-Path $srcFolder -Leaf
    $destFolder = Join-Path $CheckpointPath $folderName
    Write-Host "   Copying $folderName..." -NoNewline -ForegroundColor Yellow
    
    $cmd = "robocopy `"$srcFolder`" `"$destFolder`" /E /R:1 /W:1 /NFL /NDL /NP /MT:8 $excludeArgs"
    $result = Invoke-Expression $cmd
    $code = $LASTEXITCODE
    
    if ($code -le 7) { Write-Host " [OK]" -ForegroundColor Green }
    else { Write-Host " [WARNING exit code $code]" -ForegroundColor Yellow }
}

foreach ($relPath in $FilesToBackup) {
    $src = Join-Path $ProjectRoot $relPath
    $dest = Join-Path $CheckpointPath $relPath
    $destDir = Split-Path $dest -Parent
    if (-not (Test-Path $destDir)) { New-Item -ItemType Directory -Path $destDir -Force | Out-Null }
    Copy-Item $src -Destination $dest -Force -ErrorAction SilentlyContinue
}

$TotalSize = (Get-ChildItem $CheckpointPath -Recurse -File | Measure-Object -Property Length -Sum).Sum
$FileCount = (Get-ChildItem $CheckpointPath -Recurse -File).Count
$SizeMB = [math]::Round($TotalSize / 1MB, 2)

Write-Host "`nCheckpoint created successfully!" -ForegroundColor Green
Write-Host "   Name: $CheckpointName" -ForegroundColor Gray
Write-Host "   Files: $FileCount" -ForegroundColor Gray
Write-Host "   Size: $SizeMB MB" -ForegroundColor Gray
Write-Host "   Location: $CheckpointPath" -ForegroundColor Gray

Write-Host "`nCleaning up old checkpoints (keeping latest $Keep)..." -ForegroundColor Cyan
$AllCheckpoints = Get-ChildItem $CheckpointDir -Directory -ErrorAction SilentlyContinue | Where-Object { $_.Name -like "checkpoint_*" } | Sort-Object LastWriteTime -Descending

if ($AllCheckpoints.Count -gt $Keep) {
    $ToDelete = $AllCheckpoints | Select-Object -Skip $Keep
    foreach ($old in $ToDelete) {
        Write-Host "   Removing: $($old.Name)" -ForegroundColor DarkGray
        Remove-Item $old.FullName -Recurse -Force -ErrorAction SilentlyContinue
    }
    Write-Host "   Removed $($ToDelete.Count) old checkpoint(s)." -ForegroundColor Gray
} else {
    Write-Host "   No cleanup needed ($($AllCheckpoints.Count) / $Keep)." -ForegroundColor Gray
}

Write-Host "`nTo restore: .\Load-Checkpoint.ps1`n" -ForegroundColor Cyan