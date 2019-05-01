set THISDIR=%~dp0
pushd %THISDIR%
set pc=..\..\..\bin\x64\Binaries\pc.exe

if not exist "%pc%" goto :noP

set pt=..\..\..\bin\x64\Binaries\pt.exe

%pc% /generate:C# /shared FailureDetector.p Timer.p TestDriver.p /t:FailureDetector.4ml

if NOT errorlevel 0 goto :eof

%pc% /generate:C# /link /shared TestScript.p /r:FailureDetector.4ml

if NOT errorlevel 0 goto :eof

%pt% /psharp Test0.dll

goto :eof
:noP
echo please specify P compiler
exit /b 1
