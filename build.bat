@echo off
  
set Configuration=Release

echo abc | powershell -NonInteractive -NoProfile -ExecutionPolicy unrestricted -Command "%~dp0\Packages\PSake\psake.ps1 .\BuildScript.ps1 -properties @{configuration='%Configuration%'} -parameters @{arg0='%1'; arg1='%2'; arg2='%3'} -framework '4.0' %1"
goto end

:end
IF %ERRORLEVEL% NEQ 0 EXIT /B %ERRORLEVEL%