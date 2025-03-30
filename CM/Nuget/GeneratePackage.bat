@echo off

setlocal

set packVer=%1
set sourceDir=%2
set nuspecFile=%3

set nugetDir=%sourceDir%\..\Nuget

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
echo ^<group targetFramework="net8.0"^> >> %nuspecFile%
echo ^<dependency id="Ude.NetStandard" version="1.2.0" /^> >> %nuspecFile%
echo ^<dependency id="Microsoft.TeamFoundationServer.Client" version="19.225.1" /^> >> %nuspecFile%
echo ^<dependency id="Microsoft.VisualStudio.Services.InteractiveClient" version="19.225.1" /^> >> %nuspecFile%
echo ^<dependency id="Microsoft.AspNet.WebApi.Client" version="6.0.0" /^> >> %nuspecFile%
echo ^<dependency id="Microsoft.Identity.Client" version="4.67.2" /^> >> %nuspecFile%
echo ^<dependency id="Microsoft.IdentityModel.JsonWebTokens" version="8.3.0" /^> >> %nuspecFile%
echo ^<dependency id="System.Data.SqlClient" version="4.9.0" /^> >> %nuspecFile%
echo ^<dependency id="System.Formats.Asn1" version="9.0.1" /^> >> %nuspecFile%
echo ^<dependency id="System.IdentityModel.Tokens.Jwt" version="8.3.0" /^> >> %nuspecFile%
echo ^<dependency id="System.Net.Http" version="4.3.4" /^> >> %nuspecFile%
echo ^<dependency id="System.Security.AccessControl" version="6.0.1" /^> >> %nuspecFile%
echo ^<dependency id="System.Security.Cryptography.ProtectedData" version="9.0.1" /^> >> %nuspecFile%
echo ^<dependency id="System.Text.RegularExpressions" version="4.3.1" /^> >> %nuspecFile%
echo ^<dependency id="System.Threading.Tasks.Extensions" version="4.6.0" /^> >> %nuspecFile%
echo ^<dependency id="Newtonsoft.Json" version="13.0.3" /^> >> %nuspecFile%
echo ^</group^> >> %nuspecFile%
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
