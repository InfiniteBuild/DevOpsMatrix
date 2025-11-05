@echo off

if not %1.==. if "%1"=="/force" set force=force


set pubProjScriptDir=%~dp0
call %pubProjScriptDir%\setvariables.bat %force%

if exist %buildDir% rmdir /s /q %buildDir%

pushd %rootDir%

buildtools\Nuget\Nuget.exe restore Modules\TfsSoapApiExecutor\TfsSoapApiExecutor.csproj

dotnet build Common\DevOpsMatrixCore\DevOpsMatrixCore.csproj -o %pubDebDir%\DevOpsMatrix -p:Configuration=Debug;Platform=AnyCPU -t:Rebuild
dotnet build Common\DevOpsMatrixInterface\DevOpsMatrixInterface.csproj -o %pubDebDir%\DevOpsMatrix -p:Configuration=Debug;Platform=AnyCPU -t:Rebuild
dotnet build Modules\TfsDevOpsServer\TfsDevOpsServer.csproj -o %pubDebDir%\DevOpsMatrix\modules\Tfs -p:Configuration=Debug;Platform=AnyCPU -t:Rebuild

dotnet build Modules\TfsSoapApiExecutor\TfsSoapApiExecutor.csproj -p:Configuration=Debug;Platform=AnyCPU -t:Rebuild
robocopy /e /s Modules\TfsSoapApiExecutor\bin\Debug %pubDebDir%\DevOpsMatrix\modules\TfsSoap

dotnet build Common\DevOpsMatrixCore\DevOpsMatrixCore.csproj -o %pubRelDir%\DevOpsMatrix -p:Configuration=Release;Platform=AnyCPU -t:Rebuild
dotnet build Common\DevOpsMatrixInterface\DevOpsMatrixInterface.csproj -o %pubRelDir%\DevOpsMatrix -p:Configuration=Release;Platform=AnyCPU -t:Rebuild
dotnet build Modules\TfsDevOpsServer\TfsDevOpsServer.csproj -o %pubRelDir%\DevOpsMatrix\modules\Tfs -p:Configuration=Release;Platform=AnyCPU -t:Rebuild

dotnet build Modules\TfsSoapApiExecutor\TfsSoapApiExecutor.csproj -p:Configuration=Release;Platform=AnyCPU -t:Rebuild
robocopy /e /s Modules\TfsSoapApiExecutor\bin\Release %pubRelDir%\DevOpsMatrix\modules\TfsSoap

goto BuildComplete

:BuildError
echo ERROR during build
set scripterror=true
goto Done

:BuildComplete
call %pubProjScriptDir%\GenerateNugetInfo.bat

:Done

popd

if "%scripterror%"=="true" exit /b 1
exit /b 0