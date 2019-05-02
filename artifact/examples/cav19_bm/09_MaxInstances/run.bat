@echo off

set pt=%1
set ext=%2

set f=MaxInstances

:: paramters
set BOUND=4
set PREFIX=0


if not exist "%f%.dll" goto :noF

REM PAT experiments
@echo Run PAT ...
@echo %pt% /os-list /queue-prefix:%PREFIX% %f%.dll
@call %pt% /os-list /queue-prefix:%PREFIX% %f%.dll > %f%_pat.%ext%

REM PTESTer experiments
@echo Run PTester ...
for /L %%k in (1,1,%BOUND%) do (
	@echo %pt% /dfs /queue-bound:%%k %f%.dll
	@call %pt% /dfs /queue-bound:%%k %f%.dll > %f%_ptester_%%k.%ext%
)

goto :eof
:noF
echo please compile P program first
exit /b 1