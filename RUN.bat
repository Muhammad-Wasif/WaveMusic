@echo off
title WaveMusic Launcher
color 0A

echo ============================================
echo         WaveMusic - Launcher
echo ============================================
echo.

:: Check if dotnet is installed
dotnet --version >nul 2>&1
IF %ERRORLEVEL% NEQ 0 (
    echo [ERROR] .NET SDK not found!
    echo.
    echo Please install .NET 8 SDK from:
    echo https://dotnet.microsoft.com/download/dotnet/8.0
    echo.
    echo Download the "Windows x64 Installer" and run it.
    echo Then double-click this file again.
    echo.
    pause
    exit /b 1
)

echo [OK] .NET SDK found.
echo.
echo Restoring packages (first time only)...
dotnet restore >nul 2>&1

echo Starting WaveMusic...
echo.
dotnet run --configuration Release

IF %ERRORLEVEL% NEQ 0 (
    echo.
    echo [ERROR] Something went wrong launching the app.
    echo.
    echo Try running: dotnet build
    echo And check for error messages above.
    pause
)
