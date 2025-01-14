@echo off

set prerelese=-beta
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

copy /y %rootdir%\LICENSE %nugetDir%\LICENSE.txt

echo ^<?xml version="1.0" encoding="utf-8"?^> >> %nugetDir%\interface.nuspec
echo ^<package xmlns="http://schemas.microsoft.com/packaging/2013/05/nuspec.xsd"^> >> %nugetDir%\interface.nuspec

echo ^<metadata^> >> %nugetDir%\interface.nuspec
echo ^<id^>DevOpsMatrix^</id^> >> %nugetDir%\interface.nuspec
echo ^<version^>%version%%prerelese%^</version^> >> %nugetDir%\interface.nuspec
echo ^<description^>A library to interface with different DevOps systems through a common interface.^</description^> >> %nugetDir%\interface.nuspec
echo ^<authors^>Jared Shipley^</authors^> >> %nugetDir%\interface.nuspec
echo ^<repository type="git" url="https://github.com/OrgShipjd2001/DevOpsMatrix.git" /^> >> %nugetDir%\interface.nuspec
echo ^<readme^>docs\README.md^</readme^> >> %nugetDir%\interface.nuspec
echo ^<license type="file"^>LICENSE.txt^</license^>  >> %nugetDir%\interface.nuspec
echo ^<icon^>images/DevOpsMatrix.jpg^</icon^> >> %nugetDir%\interface.nuspec
echo ^<dependencies^> >> %nugetDir%\interface.nuspec
echo ^<group targetFramework="net8.0"^> >> %nugetDir%\interface.nuspec
echo ^<dependency id="Ude.NetStandard" version="1.2.0" /^> >> %nugetDir%\interface.nuspec
echo ^<dependency id="Microsoft.TeamFoundationServer.Client" version="19.225.1" /^> >> %nugetDir%\interface.nuspec
echo ^<dependency id="Microsoft.VisualStudio.Services.InteractiveClient" version="19.225.1" /^> >> %nugetDir%\interface.nuspec
REM echo ^<dependency id="Microsoft.TeamFoundationServer.ExtendedClient" version="19.225.1" /^> >> %nugetDir%\interface.nuspec
echo ^</group^> >> %nugetDir%\interface.nuspec
echo ^</dependencies^> >> %nugetDir%\interface.nuspec
echo ^</metadata^> >> %nugetDir%\interface.nuspec

echo ^<files^> >> %nugetDir%\interface.nuspec
echo ^<file src="%targetDir%\DevOpsMatrix\**" target="lib\net8.0"/^> >> %nugetDir%\interface.nuspec
REM echo ^<file src="%targetDir%\DevOpsMatrix\**" target="content\net8.0"/^> >> %nugetDir%\interface.nuspec
echo ^<file src="%rootDir%\README.md" target="docs\" /^> >> %nugetDir%\interface.nuspec
echo ^<file src="%nugetdir%\LICENSE.txt" target="" /^> >> %nugetDir%\interface.nuspec
echo ^<file src="%rootDir%\CM\Nuget\DevOpsMatrix.props" target="build\" /^> >> %nugetDir%\interface.nuspec
echo ^<file src="%rootdir%\Resources\DevOpsMatrix.jpg" target="images\" /^> >> %nugetDir%\interface.nuspec
echo ^</files^> >> %nugetDir%\interface.nuspec

echo ^</package^> >> %nugetDir%\interface.nuspec


echo Generate Nuget package
erase /f /q %nugetDir%\*.nupkg
%nugetexe% pack %nugetDir%\interface.nuspec -OutputDirectory %nugetDir%

%nugetexe% source |find "dev [Enabled]"
if "%ERRORLEVEL%"=="0" %nugetexe% push %nugetDir%\*.nupkg -Source Dev

:Done
