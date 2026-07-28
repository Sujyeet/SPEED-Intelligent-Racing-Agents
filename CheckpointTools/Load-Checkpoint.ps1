<#
.SYNOPSIS
    Restores your Unity project from a checkpoint (backup).
#>

param(
    [Parameter(Mandatory=$false, Position=0)]
    [string]$Name = "",

    [Parameter(Mandatory=$false)]
    [int]$Index = -1,

    [Parameter(Mandatory=$false)]
    [switch]$List,

    [Parameter(Mandatory=$false)]
    [string]$ProjectPath = ".",

    [Parameter(Mandatory=$false)]
    [switch]$Force
)

$ProjectRoot = Resolve-Path $ProjectPath
$CheckpointDir = Join-Path $ProjectRoot "Checkpoints"
$AssetsDir = Join-Path $ProjectRoot "Assets"
$SettingsDir = Join-Path $ProjectRoot "ProjectSettings"
$PackagesDir = Join-Path $ProjectRoot "Packages"

$AllCheckpoints = Get-ChildItem $CheckpointDir -Directory -ErrorAction SilentlyContinue | 
    Where-Object { $_.Name -like "checkpoint_*" } | 
    Sort-Object LastWriteTime -Descending

if ($AllCheckpoints.Count -eq 0) {
    Write-Host "No checkpoints found in $CheckpointDir" -ForegroundColor Red
    Write-Host "   Run .\Save-Checkpoint.ps1 first to create one." -ForegroundColor Yellow
    exit 1
}

function Show-CheckpointList {
    Write-Host "`nAvailable Checkpoints (newest first):" -ForegroundColor Cyan
    Write-Host ("-" * 80) -ForegroundColor DarkGray
    for ($i = 0; $i -lt $AllCheckpoints.Count; $i++) {
        $cp = $AllCheckpoints[$i]
        $size = (Get-ChildItem $cp.FullName -Recurse -File -ErrorAction SilentlyContinue | Measure-Object -Property Length -Sum).Sum
        $sizeMB = [math]::Round($size / 1MB, 2)
        $age = (Get-Date) - $cp.LastWriteTime
        $ageStr = if ($age.TotalHours -lt 1) { "{0:N0}m ago" -f $age.TotalMinutes } 
                  elseif ($age.TotalDays -lt 1) { "{0:N1}h ago" -f $age.TotalHours }
                  else { "{0:N1}d ago" -f $age.TotalDays }
        
        $marker = if ($i -eq 0) { "==> " } else { "    " }
        Write-Host "$marker[$i] $($cp.Name)" -ForegroundColor White
        Write-Host "     $($cp.LastWriteTime)  ($ageStr)  ${sizeMB} MB" -ForegroundColor Gray
    }
    Write-Host ("-" * 80) -ForegroundColor DarkGray
}

Show-CheckpointList

if ($List) { exit 0 }

$SelectedCheckpoint = $null

if ($Name) {
    $SelectedCheckpoint = $AllCheckpoints | Where-Object { $_.Name -eq $Name } | Select-Object -First 1
    if (-not $SelectedCheckpoint) {
        Write-Host "Checkpoint '$Name' not found." -ForegroundColor Red
        exit 1
    }
}
elseif ($Index -ge 0 -and $Index -lt $AllCheckpoints.Count) {
    $SelectedCheckpoint = $AllCheckpoints[$Index]
}
else {
    Write-Host "`nEnter checkpoint number to restore (0 = newest), or 'q' to quit:" -ForegroundColor Yellow -NoNewline
    $input = Read-Host
    if ($input -eq 'q' -or $input -eq 'Q') { exit 0 }
    if ([int]::TryParse($input, [ref]$null) -and [int]$input -ge 0 -and [int]$input -lt $AllCheckpoints.Count) {
        $SelectedCheckpoint = $AllCheckpoints[[int]$input]
    } else {
        Write-Host "Invalid selection." -ForegroundColor Red
        exit 1
    }
}

$CheckpointPath = $SelectedCheckpoint.FullName
$CheckpointName = $SelectedCheckpoint.Name

Write-Host "`nPreparing to restore: $CheckpointName" -ForegroundColor Cyan

$EmergencyBackup = "emergency_backup_$(Get-Date -Format 'yyyyMMdd_HHmmss')"
$EmergencyPath = Join-Path $CheckpointDir $EmergencyBackup
Write-Host "Creating safety backup of current state: $EmergencyBackup" -ForegroundColor Yellow

New-Item -ItemType Directory -Path $EmergencyPath -Force | Out-Null
foreach ($folder in @($AssetsDir, $SettingsDir, $PackagesDir)) {
    if (Test-Path $folder) {
        $fname = Split-Path $folder -Leaf
        $cmd = "robocopy `"$folder`" `"`(Join-Path $EmergencyPath $fname)`" /E /R:1 /W:1 /NFL /NDL /NP /MT:8 /XD Library Temp Logs obj bin .vs Builds Checkpoints .git node_modules __pycache__"
        Invoke-Expression $cmd
    }
}
$EmergencySize = (Get-ChildItem $EmergencyPath -Recurse -File -ErrorAction SilentlyContinue | Measure-Object -Property Length -Sum).Sum
Write-Host "   Safety backup created ($( [math]::Round($EmergencySize/1MB, 2) ) MB)" -ForegroundColor Green

if (-not $Force) {
    Write-Host "`nWARNING: This will REPLACE your current Assets/ and ProjectSettings/" -ForegroundColor Red
    Write-Host "   A safety backup was created at: $EmergencyPath" -ForegroundColor Yellow
    Write-Host "`nType 'YES' to confirm restore:" -ForegroundColor Yellow -NoNewline
    $confirm = Read-Host
    if ($confirm -ne 'YES') {
        Write-Host "Restore cancelled." -ForegroundColor Red
        Remove-Item $EmergencyPath -Recurse -Force -ErrorAction SilentlyContinue
        exit 0
    }
}

Write-Host "`nRestoring from checkpoint..." -ForegroundColor Cyan

$RestoredCount = 0
foreach ($srcFolder in @(
    @{Src=Join-Path $CheckpointPath "Assets"; Dest=$AssetsDir},
    @{Src=Join-Path $CheckpointPath "ProjectSettings"; Dest=$SettingsDir},
    @{Src=Join-Path $CheckpointPath "Packages"; Dest=$PackagesDir}
)) {
    if (Test-Path $srcFolder.Src) {
        $folderName = Split-Path $srcFolder.Dest -Leaf
        Write-Host "   Restoring $folderName..." -NoNewline -ForegroundColor Yellow
        
        if (Test-Path $srcFolder.Dest) {
            Remove-Item $srcFolder.Dest -Recurse -Force -ErrorAction SilentlyContinue
        }
        
        $cmd = "robocopy `"$($srcFolder.Src)`" `"$($srcFolder.Dest)`" /E /R:1 /W:1 /NFL /NDL /NP /MT:8"
        Invoke-Expression $cmd
        $exitCode = $LASTEXITCODE
        
        if ($exitCode -le 7) {
            Write-Host " [OK]" -ForegroundColor Green
            $RestoredCount++
        } else {
            Write-Host " [FAILED exit code $exitCode]" -ForegroundColor Red
        }
    } else {
        Write-Host "   Skipping $($srcFolder.Dest) - not in checkpoint" -ForegroundColor Gray
    }
}

$ConfigFiles = @("Packages/manifest.json", "Packages/packages-lock.json")
foreach ($relPath in $ConfigFiles) {
    $src = Join-Path $CheckpointPath $relPath
    $dest = Join-Path $ProjectRoot $relPath
    if (Test-Path $src) {
        $destDir = Split-Path $dest -Parent
        if (-not (Test-Path $destDir)) { New-Item -ItemType Directory -Path $destDir -Force | Out-Null }
        Copy-Item $src -Destination $dest -Force -ErrorAction SilentlyContinue
    }
}

Write-Host "`nRestore complete! $RestoredCount folder(s) restored." -ForegroundColor Green
Write-Host "   Restored from: $CheckpointName" -ForegroundColor Gray
Write-Host "   Safety backup kept at: $EmergencyPath" -ForegroundColor Yellow
Write-Host "`nNext steps:" -ForegroundColor Cyan
Write-Host "   1. Open Unity - it will reimport assets (may take a minute)" -ForegroundColor Gray
Write-Host "   2. If issues: delete Library/ folder and reopen Unity" -ForegroundColor Gray
Write-Host "   3. To undo this restore: run Load-Checkpoint.ps1 and pick '$EmergencyBackup'" -ForegroundColor Gray