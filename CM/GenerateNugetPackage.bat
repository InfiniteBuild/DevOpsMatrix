@echo off

set scriptDir=%~dp0
set rootDir=%scriptDir%..
set publishDir=%rootDir%\Publish
set nugetDir=%PublishDir%\Nuget
set nugetexe=%rootDir%\buildtools\nuget\nuget.exe
set targetDir=%PublishDir%\Release

if "%version%"=="" for /f "tokens=1,2,3* delims=<>" %%i in (%scriptDir%\version\assemblyversion.props) do if "%%j"=="FileVersion" set version=%%k

mkdir %nugetDir% >NUL

REM Create nuspec
echo Create NuSpec file
if EXIST %nugetDir%\interface.nuspec erase /f /q %nugetDir%\interface.nuspec

echo ^<?xml version="1.0" encoding="utf-8"?^> >> %nugetDir%\interface.nuspec
echo ^<package xmlns="http://schemas.microsoft.com/packaging/2013/05/nuspec.xsd"^> >> %nugetDir%\interface.nuspec
echo ^<metadata^> >> %nugetDir%\interface.nuspec
echo ^<id^>DevOpsMatrix^</id^> >> %nugetDir%\interface.nuspec
echo ^<version^>%version%^</version^> >> %nugetDir%\interface.nuspec
echo ^<description^>A library to interface with DevOps systems^</description^> >> %nugetDir%\interface.nuspec
echo ^<authors^>Jared Shipley^</authors^> >> %nugetDir%\interface.nuspec
echo ^<repository type="git" url="https://github.com/OrgShipjd2001/DevOpsMatrix.git" /^> >> %nugetDir%\interface.nuspec
echo ^<readme^>docs\README.md^</readme^> >> %nugetDir%\interface.nuspec
echo ^<dependencies^> >> %nugetDir%\interface.nuspec
echo ^<group targetFramework=".NET8.0" /^> >> %nugetDir%\interface.nuspec
echo ^</dependencies^> >> %nugetDir%\interface.nuspec
echo ^</metadata^> >> %nugetDir%\interface.nuspec
echo ^<files^> >> %nugetDir%\interface.nuspec

echo ^<file src="%targetDir%\DevOpsLib\**" target="lib\net8.0"/^> >> %nugetDir%\interface.nuspec
echo ^<file src="%rootdir%\CM\Nuget\DevOpsMatrix.targets" target="build"/^> >> %nugetDir%\interface.nuspec
echo ^<file src="%rootDir%\README.md" target="docs\" /^> >> %nugetDir%\interface.nuspec

echo ^</files^> >> %nugetDir%\interface.nuspec
echo ^</package^> >> %nugetDir%\interface.nuspec


echo Generate Nuget package
erase /f /q %nugetDir%\*.nupkg
%nugetexe% pack %nugetDir%\interface.nuspec -OutputDirectory %nugetDir%

:Done
