@echo off

if not %1.==. if "%1"=="/force" set force=true

set genPckScriptDir=%~dp0
call %genPckScriptDir%\SetVariables.bat %force%

set prerelease=-localDev

mkdir %nugetDir% >NUL

if EXIST %nugetDir%\*.nuspec erase /f /q %nugetDir%\*.nuspec
erase /f /q %nugetDir%\*.nupkg

call %rootdir%\cm\Nuget\GeneratePackage.bat %version%%prerelease% %pubRelDir% %nugetDir%\interface.nuspec

%rootDir%\buildtools\nuget\nuget.exe pack %nugetDir%\interface.nuspec -OutputDirectory %nugetDir%
