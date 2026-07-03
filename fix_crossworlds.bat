@echo off
title Crossworlds Unity 6 API Fixer
echo ============================================
echo  Crossworlds -- Unity 6 compile error fixer
echo ============================================
echo.
echo Scanning D:\Crossworlds for:
echo   - using UnityEngine.Networking  (dead UNET imports)
echo   - FindObjectsOfType / FindObjectOfType  (obsolete Unity 6 API)
echo   - FindObjectsByType single-param  (removed overload)
echo   - TextAlignmentOptions.Midline/Capline/Baseline  (removed TMPro enums)
echo.
cd /d "D:\Crossworlds\CrossWorlds"
powershell -ExecutionPolicy Bypass -File "fix-unity6-api.ps1"
echo.
echo Press any key to close...
pause >nul
