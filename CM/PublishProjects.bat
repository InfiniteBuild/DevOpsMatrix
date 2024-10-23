@echo off

set scriptDir=%~dp0
set rootDir=%scriptDir%..\..
set publishDir=%rootDir%\Publish
set pubRelDir=%publishdir%\Release
set pubDebDir=%publishdir%\Debug

if exist %PublishDir% rmdir /s /q %PublishDir%

pushd %scriptDir%..\..

dotnet publish Common\DevOpsCore\DevOpsCore.csproj -o %pubDebDir%\DevOpsLib --no-self-contained -c Debug
dotnet publish Common\DevOpsInterface\DevOpsInterface.csproj -o %pubDebDir%\DevOpsLib --no-self-contained -c Debug
dotnet publish Modules\TfsDevOpsServer\TfsDevOpsServer.csproj -o %pubDebDir%\DevOpsLib\modules\Tfs --no-self-contained -c Debug

dotnet build Modules\TfsSoapApiExecutor\TfsSoapApiExecutor.csproj -p:Configuration=Debug;Platform=AnyCPU -t:Rebuild
robocopy /e /s Modules\TfsSoapApiExecutor\bin\Debug %pubDebDir%\DevOpsLib\modules\TfsSoap

dotnet publish Common\DevOpsCore\DevOpsCore.csproj -o %pubRelDir%\DevOpsLib --no-self-contained -c Release
dotnet publish Common\DevOpsInterface\DevOpsInterface.csproj -o %pubRelDir%\DevOpsLib --no-self-contained -c Release
dotnet publish Modules\TfsDevOpsServer\TfsDevOpsServer.csproj -o %pubRelDir%\DevOpsLib\modules\Tfs --no-self-contained -c Release

dotnet build Modules\TfsSoapApiExecutor\TfsSoapApiExecutor.csproj -p:Configuration=Release;Platform=AnyCPU -t:Rebuild
robocopy /e /s Modules\TfsSoapApiExecutor\bin\Release %pubRelDir%\DevOpsLib\modules\TfsSoap
popd

:Done