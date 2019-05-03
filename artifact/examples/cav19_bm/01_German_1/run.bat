@echo off

set pt=%1
set ext=%2

set f=german_1

:: paramters
set BOUND=6
set PREFIX=0


if not exist "%f%.dll" goto :noF

REM PAT experiments
@echo Run PAT+I ...
@echo %pt% /os-list /queue-prefix:%PREFIX% %f%.dll /qutl:"true:ask_excl # 1 <= ask_share # 1 <= &:true"
@call %pt% /os-list /queue-prefix:%PREFIX% %f%.dll /qutl:"true:ask_excl # 1 <= ask_share # 1 <= &:true" > %f%_pati.%ext%

goto :eof
:noF
echo please compile P program first
exit /b 1