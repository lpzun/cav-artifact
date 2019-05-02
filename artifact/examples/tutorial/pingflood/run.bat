@echo off

set THISDIR=%~dp0
pushd %THISDIR%
echo echo "current directory is: %THISDIR%"

set PHOME=..\..\..
set cp=bin\x64
set pt=%PHOME%\%cp%\Binaries\pt.exe

if not exist "%pt%" goto :no-pt

set t=pingflood

@echo %pt% %t%.dll /os-list /queue-prefix:4
@call %pt% %t%.dll /os-list /queue-prefix:4 > %t%_pat.out

REM we try different kind of invariants
@echo @echo %pt% %t%.dll /os-list /qutl:"true:DONE PRIME ! G => () G"
@call %pt% %t%.dll /os-list /qutl:"true:DONE PRIME ! G => () G" > %t%_pati_1.out

@echo %pt% %t%.dll /os-list /qutl:"true:$ DONE = DONE # 0 = => () PING PING G => () G &"
@call %pt% %t%.dll /os-list /qutl:"true:$ DONE = DONE # 0 = => () PING PING G => () G &" > %t%_pati_2.out

goto :eof
:no-pt
echo please specify pt.exe