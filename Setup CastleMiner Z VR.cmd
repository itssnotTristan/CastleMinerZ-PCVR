@echo off
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%~dp0CastleMinerZVR\Setup-CastleMinerZVR.ps1"
if errorlevel 1 echo Setup failed. See the message and setup log above.
pause
