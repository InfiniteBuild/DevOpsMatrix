@echo off

set scriptDir=%~dp0
set rootDir=%scriptDir%..
set publishDir=%rootDir%\Publish
set pubRelDir=%publishdir%\Release
set pubDebDir=%publishdir%\Debug

if exist %PublishDir% rmdir /s /q %PublishDir%

pushd %rootDir%

buildtools\Nuget\Nuget.exe restore Modules\TfsSoapApiExecutor\TfsSoapApiExecutor.csproj

dotnet publish Common\DevOpsMatrixCore\DevOpsMatrixCore.csproj -o %pubDebDir%\DevOpsMatrix --no-self-contained -c Debug
dotnet publish Common\DevOpsMatrixInterface\DevOpsMatrixInterface.csproj -o %pubDebDir%\DevOpsMatrix --no-self-contained -c Debug
dotnet publish Modules\TfsDevOpsServer\TfsDevOpsServer.csproj -o %pubDebDir%\DevOpsMatrix\modules\Tfs --no-self-contained -c Debug
copy /y %rootDir%\License %pubDebDir%\License.txt
copy /y %rootDir%\ReadMe.md %pubDebDir%\ReadMe.md

dotnet build Modules\TfsSoapApiExecutor\TfsSoapApiExecutor.csproj -p:Configuration=Debug;Platform=AnyCPU -t:Rebuild
robocopy /e /s Modules\TfsSoapApiExecutor\bin\Debug %pubDebDir%\DevOpsMatrix\modules\TfsSoap

dotnet publish Common\DevOpsMatrixCore\DevOpsMatrixCore.csproj -o %pubRelDir%\DevOpsMatrix --no-self-contained -c Release
dotnet publish Common\DevOpsMatrixInterface\DevOpsMatrixInterface.csproj -o %pubRelDir%\DevOpsMatrix --no-self-contained -c Release
dotnet publish Modules\TfsDevOpsServer\TfsDevOpsServer.csproj -o %pubRelDir%\DevOpsMatrix\modules\Tfs --no-self-contained -c Release
copy /y %rootDir%\License %pubRelDir%\License.txt
copy /y %rootDir%\ReadMe.md %pubRelDir%\ReadMe.md

dotnet build Modules\TfsSoapApiExecutor\TfsSoapApiExecutor.csproj -p:Configuration=Release;Platform=AnyCPU -t:Rebuild
robocopy /e /s Modules\TfsSoapApiExecutor\bin\Release %pubRelDir%\DevOpsMatrix\modules\TfsSoap
popd

:Done