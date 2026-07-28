@echo off
title Unity Checkpoint Tools
cd /d "%~dp0.."

echo.
echo ╔══════════════════════════════════════════════════════════════╗
echo ║        UNITY CHECKPOINT TOOLS - Quick Launcher              ║
echo ╚══════════════════════════════════════════════════════════════╝
echo.
echo Project: %CD%
echo Checkpoints: %CD%\Checkpoints
echo.
echo Available commands:
echo.
echo   1) SAVE   - Create a new checkpoint (with optional name)
echo   2) LOAD   - Restore from a checkpoint (interactive menu)
echo   3) LIST   - Show all checkpoints with dates/sizes
echo   4) OPEN   - Open Checkpoints folder in Explorer
echo   5) EXIT
echo.
set /p choice="Enter choice (1-5): "

if "%choice%"=="1" (
    echo.
    set /p name="Checkpoint name (optional, press Enter to skip): "
    if "%name%"=="" (
        powershell -ExecutionPolicy Bypass -File "CheckpointTools\Save-Checkpoint.ps1"
    ) else (
        powershell -ExecutionPolicy Bypass -File "CheckpointTools\Save-Checkpoint.ps1" -Name "%name%"
    )
    echo.
    pause
)

if "%choice%"=="2" (
    powershell -ExecutionPolicy Bypass -File "CheckpointTools\Load-Checkpoint.ps1"
    echo.
    pause
)

if "%choice%"=="3" (
    powershell -ExecutionPolicy Bypass -File "CheckpointTools\List-Checkpoints.ps1" -ShowSize
    echo.
    pause
)

if "%choice%"=="4" (
    if exist "Checkpoints" (
        explorer "Checkpoints"
    ) else (
        echo Checkpoints folder doesn't exist yet. Create one first!
        pause
    )
)

if "%choice%"=="5" exit /b