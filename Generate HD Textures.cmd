@echo off
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%~dp0CastleMinerZVR\Generate-HDTextures.ps1"
if errorlevel 1 echo HD conversion failed. Original textures are preserved.
pause
