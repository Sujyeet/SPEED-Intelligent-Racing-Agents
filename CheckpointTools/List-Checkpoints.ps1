<#
.SYNOPSIS
    Lists all available checkpoints with details.
#>

param(
    [Parameter(Mandatory=$false)]
    [int]$Last = 0,

    [Parameter(Mandatory=$false)]
    [switch]$ShowSize,

    [Parameter(Mandatory=$false)]
    [string]$ProjectPath = "."
)

$ProjectRoot = Resolve-Path $ProjectPath
$CheckpointDir = Join-Path $ProjectRoot "Checkpoints"

if (-not (Test-Path $CheckpointDir)) {
    Write-Host "No Checkpoints folder found at: $CheckpointDir" -ForegroundColor Red
    Write-Host "   Run .\Save-Checkpoint.ps1 first to create one." -ForegroundColor Yellow
    exit 1
}

$AllCheckpoints = Get-ChildItem $CheckpointDir -Directory -ErrorAction SilentlyContinue | 
    Where-Object { $_.Name -like "checkpoint_*" } | 
    Sort-Object LastWriteTime -Descending

if ($AllCheckpoints.Count -eq 0) {
    Write-Host "No checkpoints found." -ForegroundColor Yellow
    exit 0
}

if ($Last -gt 0) {
    $AllCheckpoints = $AllCheckpoints | Select-Object -First $Last
}

Write-Host "`nCheckpoints in $CheckpointDir`n" -ForegroundColor Cyan
Write-Host ("-" * 100) -ForegroundColor DarkGray

$TotalSize = 0
foreach ($cp in $AllCheckpoints) {
    $size = 0
    if ($ShowSize) {
        $size = (Get-ChildItem $cp.FullName -Recurse -File -ErrorAction SilentlyContinue | Measure-Object -Property Length -Sum).Sum
        $TotalSize += $size
    }
    
    $age = (Get-Date) - $cp.LastWriteTime
    if ($age.TotalHours -lt 1) { $ageStr = "{0:N0}m ago" -f $age.TotalMinutes }
    elseif ($age.TotalDays -lt 1) { $ageStr = "{0:N1}h ago" -f $age.TotalHours }
    else { $ageStr = "{0:N1}d ago" -f $age.TotalDays }
    
    $desc = ""
    if ($cp.Name -match 'checkpoint_\d{8}_\d{6}_(.+)') {
        $desc = "  [ $($matches[1].Replace('_', ' ')) ]"
    }
    
    $sizeStr = ""
    if ($ShowSize -and $size -gt 0) { 
        $sizeStr = "  [{0:N1} MB]" -f ($size/1MB) 
    }
    
    Write-Host "  $($cp.Name)$desc" -ForegroundColor White
    Write-Host "     $($cp.LastWriteTime)  ($ageStr)$sizeStr" -ForegroundColor Gray
}

Write-Host ("-" * 100) -ForegroundColor DarkGray
Write-Host "  Total: $($AllCheckpoints.Count) checkpoint(s)" -ForegroundColor Cyan
if ($ShowSize -and $TotalSize -gt 0) {
    $combined = "{0:N1} MB" -f ($TotalSize/1MB)
    Write-Host "  Combined size: $combined" -ForegroundColor Cyan
}
Write-Host ""