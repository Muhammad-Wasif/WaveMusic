@echo off
title WaveMusic - Build EXE
color 0B

echo ============================================
echo      WaveMusic - Build Single EXE
echo ============================================
echo.

dotnet --version >nul 2>&1
IF %ERRORLEVEL% NEQ 0 (
    echo [ERROR] .NET SDK not found!
    echo Install from: https://dotnet.microsoft.com/download/dotnet/8.0
    pause & exit /b 1
)

echo [OK] .NET SDK found.
echo.

SET OUTDIR=%~dp0..\WaveMusic_EXE

echo Building... Output: %OUTDIR%
echo This takes 1-3 minutes. Please wait...
echo.

dotnet publish -c Release -r win-x64 --self-contained true ^
    -p:PublishSingleFile=true ^
    -p:IncludeNativeLibrariesForSelfExtract=true ^
    -p:EnableCompressionInSingleFile=true ^
    -p:DebugType=None ^
    -p:DebugSymbols=false ^
    -p:TrimmerRootDescriptorFiles="" ^
    -o "%OUTDIR%"

IF %ERRORLEVEL% NEQ 0 (
    echo.
    echo [ERROR] Build failed. See errors above.
    pause & exit /b 1
)

echo.
echo Cleaning up extra files (keeping only WaveMusic.exe)...
:: Delete every file in the output folder EXCEPT WaveMusic.exe
for %%f in ("%OUTDIR%\*") do (
    if /I NOT "%%~nxf"=="WaveMusic.exe" (
        del /f /q "%%f" 2>nul
    )
)
:: Delete any leftover subdirectories
for /d %%d in ("%OUTDIR%\*") do (
    rd /s /q "%%d" 2>nul
)

echo.
echo ============================================
echo   SUCCESS! Single EXE is ready.
echo ============================================
echo.
echo Location: %OUTDIR%\WaveMusic.exe
echo Size: 
for %%f in ("%OUTDIR%\WaveMusic.exe") do echo   %%~zf bytes (~%%~zfMB)
echo.
echo Copy WaveMusic.exe anywhere - no .NET needed on target PC.
echo.

explorer "%OUTDIR%"
pause
