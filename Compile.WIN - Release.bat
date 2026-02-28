@ECHO OFF
SETLOCAL
SET CURPATH=%~dp0
SET EXENAME=ServUO
SET DOTNET_EXE="C:\Program Files\dotnet\dotnet.exe"

TITLE %EXENAME% Build System - .NET 8.0

:: 1. Build Ultima SDK
ECHO [1/2] Compiling Ultima SDK...
%DOTNET_EXE% build "Ultima/Ultima.csproj" -c Release
IF %ERRORLEVEL% NEQ 0 GOTO :ERROR

:: 2. Build Server Core
ECHO [2/2] Compiling %EXENAME% Server Core...
:: Outputting directly to the 4.0 root folder
%DOTNET_EXE% build "Server/Server.csproj" -c Release -o .
IF %ERRORLEVEL% NEQ 0 GOTO :ERROR

ECHO.
ECHO ==========================================
ECHO  BUILD SUCCESSFUL
ECHO ==========================================
PAUSE

:: 3. Run Server
"%CURPATH%%EXENAME%.exe"
GOTO :EOF

:ERROR
ECHO.
ECHO [!] BUILD FAILED
ECHO Please check the compiler errors above.
PAUSE
ENDLOCAL