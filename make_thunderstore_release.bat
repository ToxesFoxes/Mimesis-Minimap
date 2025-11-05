@echo off
REM Windows batch script to create Thunderstore release

REM Copy the compiled DLL to thunderstore directory
copy ".\bin\Release\netstandard2.1\Minimap.dll" ".\thunderstore\Minimap.dll"
if %errorlevel% neq 0 (
    echo Failed to copy Minimap.dll
    exit /b 1
)

REM Copy README to thunderstore directory
copy ".\README.md" ".\thunderstore\README.md"
if %errorlevel% neq 0 (
    echo Failed to copy README.md
    exit /b 1
)

REM Change to thunderstore directory and create zip
cd thunderstore
if %errorlevel% neq 0 (
    echo Failed to change to thunderstore directory
    exit /b 1
)

REM Create zip file using PowerShell (available on Windows 10+)
powershell -Command "Compress-Archive -Path .\* -DestinationPath ..\Release-thunderstore.zip -Force"
if %errorlevel% neq 0 (
    echo Failed to create zip file
    cd ..
    exit /b 1
)

REM Return to parent directory
cd ..

echo Thunderstore release created successfully: Release-thunderstore.zip