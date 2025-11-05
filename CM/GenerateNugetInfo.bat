@echo off

if not %1.==. if "%1"=="/force" set force=force

set genNugtScriptDir=%~dp0
call %genNugtScriptDir%\setvariables.bat %force%

pushd %rootDir%

mkdir %nugetDir% >NUL
mkdir %nugetDir%\Data >NUL
copy /y %rootDir%\License %nugetDir%\Data\License.txt
copy /y %rootDir%\ReadMe.md %nugetDir%\Data\ReadMe.md
copy /y %rootDir%\CM\Nuget\DevOpsMatrix.props %nugetDir%\Data\DevOpsMatrix.props
copy /y %rootDir%\Resources\DevOpsMatrix.jpg %nugetDir%\Data\DevOpsMatrix.jpg

echo.
echo Retrieve Nuget package dependency info

REM for readability, set the list in a variable (list is comma delimited)
set csprojList=Common\DevOpsMatrixCore\DevOpsMatrixCore.csproj
set csprojList=%csprojList%,Common\DevOpsMatrixInterface\DevOpsMatrixInterface.csproj
set csprojList=%csprojList%,Modules\TfsDevOpsServer\TfsDevOpsServer.csproj
powershell %rootDir%\cm\scripts\generate_dependencies.ps1 -csprojFiles %csprojList% -outputDir %nugetDir%\Data

popd
