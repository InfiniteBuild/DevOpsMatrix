@echo off

if not %1.==. if "%1"=="/force" set force=true

set pubProjScriptDir=%~dp0
call %pubProjScriptDir%\setvariables.bat %force%

if exist %PublishDir% rmdir /s /q %PublishDir%

pushd %rootDir%

buildtools\Nuget\Nuget.exe restore Modules\TfsSoapApiExecutor\TfsSoapApiExecutor.csproj

dotnet publish Common\DevOpsMatrixCore\DevOpsMatrixCore.csproj -o %pubDebDir%\DevOpsMatrix --no-self-contained -c Debug
dotnet publish Common\DevOpsMatrixInterface\DevOpsMatrixInterface.csproj -o %pubDebDir%\DevOpsMatrix --no-self-contained -c Debug
dotnet publish Modules\TfsDevOpsServer\TfsDevOpsServer.csproj -o %pubDebDir%\DevOpsMatrix\modules\Tfs --no-self-contained -c Debug

dotnet build Modules\TfsSoapApiExecutor\TfsSoapApiExecutor.csproj -p:Configuration=Debug;Platform=AnyCPU -t:Rebuild
robocopy /e /s Modules\TfsSoapApiExecutor\bin\Debug %pubDebDir%\DevOpsMatrix\modules\TfsSoap

dotnet publish Common\DevOpsMatrixCore\DevOpsMatrixCore.csproj -o %pubRelDir%\DevOpsMatrix --no-self-contained -c Release
dotnet publish Common\DevOpsMatrixInterface\DevOpsMatrixInterface.csproj -o %pubRelDir%\DevOpsMatrix --no-self-contained -c Release
dotnet publish Modules\TfsDevOpsServer\TfsDevOpsServer.csproj -o %pubRelDir%\DevOpsMatrix\modules\Tfs --no-self-contained -c Release

dotnet build Modules\TfsSoapApiExecutor\TfsSoapApiExecutor.csproj -p:Configuration=Release;Platform=AnyCPU -t:Rebuild
robocopy /e /s Modules\TfsSoapApiExecutor\bin\Release %pubRelDir%\DevOpsMatrix\modules\TfsSoap

mkdir %nugetDir% >NUL
mkdir %nugetDir%\Data >NUL
copy /y %rootDir%\License %nugetDir%\Data\License.txt
copy /y %rootDir%\ReadMe.md %nugetDir%\Data\ReadMe.md
copy /y %rootDir%\CM\Nuget\DevOpsMatrix.props %nugetDir%\Data\DevOpsMatrix.props
copy /y %rootDir%\Resources\DevOpsMatrix.jpg %nugetDir%\Data\DevOpsMatrix.jpg


popd

:Done