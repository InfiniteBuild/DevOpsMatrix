@echo off

setlocal

set packVer=%1
set sourceDir=%2
set nuspecFile=%3
set nugetDir=%sourceDir%\..\Nuget

echo packVer: %packVer%
echo sourceDir: %sourceDir%
echo nuspecFile: %nuspecFile%
echo nugetDir: %nugetDir%

REM Create nuspec
echo Create NuSpec file
if EXIST %nuspecFile% erase /f /q %nuspecFile%

echo ^<?xml version="1.0" encoding="utf-8"?^> >> %nuspecFile%
echo ^<package xmlns="http://schemas.microsoft.com/packaging/2013/05/nuspec.xsd"^> >> %nuspecFile%

echo ^<metadata^> >> %nuspecFile%
echo ^<id^>DevOpsMatrix^</id^> >> %nuspecFile%
echo ^<version^>%packVer%^</version^> >> %nuspecFile%
echo ^<description^>A library to interface with different DevOps systems through a common interface.^</description^> >> %nuspecFile%
echo ^<authors^>Jared Shipley^</authors^> >> %nuspecFile%
echo ^<repository type="git" url="https://github.com/OrgShipjd2001/DevOpsMatrix.git" /^> >> %nuspecFile%
echo ^<readme^>docs\README.md^</readme^> >> %nuspecFile%
echo ^<license type="file"^>License.txt^</license^>  >> %nuspecFile%
echo ^<icon^>images/DevOpsMatrix.jpg^</icon^> >> %nuspecFile%
echo ^<dependencies^> >> %nuspecFile%

REM Corrected FOR loop to process XML dependency files
for %%i in ("%nugetDir%\data\dependencies*.xml") do (
    type %%i >> %nuspecFile%
	echo. >> %nuspecFile%
)

echo ^</dependencies^> >> %nuspecFile%
echo ^</metadata^> >> %nuspecFile%

echo ^<files^> >> %nuspecFile%
echo ^<file src="%sourceDir%\DevOpsMatrix\**" target="lib\net8.0"/^> >> %nuspecFile%
echo ^<file src="%nugetDir%\Data\README.md" target="docs\" /^> >> %nuspecFile%
echo ^<file src="%nugetDir%\Data\License.txt" target="" /^> >> %nuspecFile%
echo ^<file src="%nugetDir%\Data\DevOpsMatrix.props" target="build\" /^> >> %nuspecFile%
echo ^<file src="%nugetDir%\Data\DevOpsMatrix.jpg" target="images\" /^> >> %nuspecFile%
echo ^</files^> >> %nuspecFile%

echo ^</package^> >> %nuspecFile%

:Done
endlocal
