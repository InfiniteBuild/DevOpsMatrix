@echo off

if not %1.==. if "%1"=="/force" set force=true

set genPckScriptDir=%~dp0
call %genPckScriptDir%\SetVariables.bat %force%

set prerelease=-localDev

mkdir %nugetDir% >NUL

if EXIST %nugetDir%\*.nuspec erase /f /q %nugetDir%\*.nuspec
erase /f /q %nugetDir%\*.nupkg

call %rootdir%\cm\Nuget\GeneratePackage.bat %version%%prerelease% %targetDir% %nugetDir%\interface.nuspec

%nugetexe% pack %nugetDir%\interface.nuspec -OutputDirectory %nugetDir%
