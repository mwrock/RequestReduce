@echo off
  
set Configuration=Release

if /i "%1"=="DEBUG" set Configuration=Debug
if /i "%2"=="DEBUG" set Configuration=Debug

%windir%\Microsoft.NET\Framework64\v4.0.30319\MSBuild.exe .nuget\nuget.targets /t:RestorePackages
echo abc | powershell -NonInteractive -NoProfile -ExecutionPolicy unrestricted -Command "$psakeDir = ([array](dir %~dp0packages\psake.*))[-1]; .$psakeDir\tools\psake.ps1 .\BuildScript.ps1 -properties @{configuration='%Configuration%'} -ScriptPath $psakeDir\tools %1; if ($psake.build_success -eq $false) { exit 1 } else { exit 0 }"
goto end

:end
IF %ERRORLEVEL% NEQ 0 EXIT /B %ERRORLEVEL%