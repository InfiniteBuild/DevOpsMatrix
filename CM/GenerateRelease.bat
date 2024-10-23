@echo off

set version=1.0.0
set scriptDir=%~dp0
set rootDir=%scriptDir%..
set publishDir=%rootDir%\Publish
set zipDir=%PublishDir%\zip
set targetDir=%PublishDir%\Release

if exist %PublishDir% rmdir /s /q %PublishDir%

pushd %rootDir%
call %rootDir%\cm\PublishProjects.bat
popd

for /f "tokens=1,2,3* delims=<>" %%i in (%scriptDir%\version\assemblyversion.props) do if "%%j"=="FileVersion" set version=%%k
echo Version: %version%

call %rootdir%\cm\GenerateNugetPackage.bat

%rootdir%\buildtools\7-zip\7z.exe a %zipdir%\DevOpsMatrix_%version%.zip %targetdir%\DevOpsMatrix\**

:Done
