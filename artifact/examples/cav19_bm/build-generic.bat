@echo off

if %1. == .   goto no-file
if %1. == /h. goto no-file

set src=%1

set PHOME=..\..\..
set cp=bin\x64
set pc=%PHOME%\%cp%\Binaries\pc.exe

if not exist "%pc%" goto :no-pc

echo %pc% /generate:C# %src%.p /t:%src%.4ml
call %pc% /generate:C# %src%.p /t:%src%.4ml

if NOT errorlevel 0 goto eof

echo %pc% /generate:C# /link /r:%src%.4ml
call %pc% /generate:C# /link /r:%src%.4ml

move linker.dll %src%.dll

echo now consider running something like "pt /os-list %src%.dll"

goto eof

:no-file
  echo Usage: "build <myfile>" , where "<myfile>" is the .p file argument but *without* suffix .p, e.g. "build german"
:no-pc
echo please specify P compiler
:eof
