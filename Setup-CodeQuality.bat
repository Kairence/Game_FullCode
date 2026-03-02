@ECHO OFF
SETLOCAL
TITLE Code Quality Tools Setup

ECHO ==========================================
ECHO  Code Quality Tools Setup
ECHO ==========================================
ECHO.

:: 1. Restore dotnet local tools (CSharpier, Husky)
ECHO [1/3] Restoring dotnet tools...
dotnet tool restore
IF %ERRORLEVEL% NEQ 0 (
    ECHO [!] Tool restore failed. Make sure .NET 8 SDK is installed.
    PAUSE
    EXIT /B 1
)
ECHO.

:: 2. Install Husky git hooks
ECHO [2/3] Installing Husky git hooks...
dotnet husky install
IF %ERRORLEVEL% NEQ 0 (
    ECHO [!] Husky install failed.
    PAUSE
    EXIT /B 1
)
ECHO.

:: 3. Verify
ECHO [3/3] Verifying installation...
dotnet tool run dotnet-csharpier --version
ECHO.

ECHO ==========================================
ECHO  Setup Complete!
ECHO ==========================================
ECHO.
ECHO Available commands:
ECHO   dotnet csharpier .          - Format all C# files
ECHO   dotnet csharpier --check .  - Check formatting (no changes)
ECHO   dotnet format               - Apply .editorconfig rules
ECHO   dotnet format --verify-no-changes  - Check style (no changes)
ECHO.
PAUSE
ENDLOCAL
