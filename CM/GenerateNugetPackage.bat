@echo off

set prerelease=-alpha

set scriptDir=%~dp0
set rootDir=%scriptDir%..
set publishDir=%rootDir%\Publish
set nugetDir=%PublishDir%\Nuget
set nugetexe=%rootDir%\buildtools\nuget\nuget.exe
set targetDir=%PublishDir%\Release

if "%version%"=="" for /f "tokens=1,2,3* delims=<>" %%i in (%scriptDir%\version\assemblyversion.props) do if "%%j"=="FileVersion" set version=%%k
mkdir %nugetDir% >NUL

if EXIST %nugetDir%\*.nuspec erase /f /q %nugetDir%\*.nuspec
erase /f /q %nugetDir%\*.nupkg

call %rootdir%\cm\Nuget\GeneratePackage.bat %version%%prerelease% %targetDir% %nugetDir%\interface.nuspec

%nugetexe% pack %nugetDir%\interface.nuspec -OutputDirectory %nugetDir%
