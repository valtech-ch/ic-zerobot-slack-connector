@ECHO OFF
IF "%DOTNET_PATH%"=="" ECHO SET DOTNET_PATH=C:\Windows\Microsoft.NET\Framework\v4.0.30319

%DOTNET_PATH%\MSBuild.exe /p:Configuration=Debug Docer.shfbproj
