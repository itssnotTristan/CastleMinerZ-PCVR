@echo off
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%~dp0Setup-QuestStandalone.ps1" %*
if errorlevel 1 echo Setup stopped. Read the message above and START-HERE.html before trying again.
pause
