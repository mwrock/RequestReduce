@echo off
  
set Configuration=Release

if /i "%1"=="DEBUG" set Configuration=Debug
if /i "%2"=="DEBUG" set Configuration=Debug
if /i "%1"=="RESET" goto Net40
if /i "%1"=="SETUP-IIS" goto Net40
if /i "%1"=="DOWNLOAD" goto Download35

echo abc | powershell -NonInteractive -NoProfile -ExecutionPolicy unrestricted -Command "%~dp0\packages\psake.4.0.1.0\tools\psake.ps1 .\BuildScript.ps1 -properties @{configuration='%Configuration%'} -framework '3.5' BuildNet35"
goto Net40

:Download35
echo abc | powershell -NonInteractive -NoProfile -ExecutionPolicy unrestricted -Command "%~dp0\packages\psake.4.0.1.0\tools\psake.ps1 .\BuildScript.ps1 -properties @{configuration='%Configuration%'} -framework '3.5' Download35"

:Net40
echo abc | powershell -NonInteractive -NoProfile -ExecutionPolicy unrestricted -Command "%~dp0\packages\psake.4.0.1.0\tools\psake.ps1 .\BuildScript.ps1 -properties @{configuration='%Configuration%'} -framework '4.0' %1"
goto end

:end
IF %ERRORLEVEL% NEQ 0 EXIT /B %ERRORLEVEL%