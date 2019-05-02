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
@echo %pt% /os-list /queue-prefix:%PREFIX% %f%.dll
@call %pt% /os-list /queue-prefix:%PREFIX% %f%.dll > %f%_pat.%ext%

goto :eof
:noF
echo please compile P program first
exit /b 1