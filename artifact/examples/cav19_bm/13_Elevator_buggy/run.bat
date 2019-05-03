@echo off

set pt=%1
set ext=%2

set f=Elevator_buggy

:: paramters
set BOUND=1
set PREFIX=0


if not exist "%f%.dll" goto :noF

set start=%time%

REM PAT experiments
@echo Run PAT ...
@echo %pt% /os-list /queue-prefix:%PREFIX% %f%.dll
@call %pt% /os-list /queue-prefix:%PREFIX% %f%.dll > %f%_pat.%ext%

set end=%time%

set options="tokens=1-4 delims=:.,"
for /f %options% %%a in ("%start%") do set start_h=%%a&set /a start_m=100%%b %% 100&set /a start_s=100%%c %% 100&set /a start_ms=100%%d %% 100
for /f %options% %%a in ("%end%") do set end_h=%%a&set /a end_m=100%%b %% 100&set /a end_s=100%%c %% 100&set /a end_ms=100%%d %% 100

set /a hours=%end_h%-%start_h%
set /a mins=%end_m%-%start_m%
set /a secs=%end_s%-%start_s%
set /a ms=%end_ms%-%start_ms%
if %ms% lss 0 set /a secs = %secs% - 1 & set /a ms = 100%ms%
if %secs% lss 0 set /a mins = %mins% - 1 & set /a secs = 60%secs%
if %mins% lss 0 set /a hours = %hours% - 1 & set /a mins = 60%mins%
if %hours% lss 0 set /a hours = 24%hours%
if 1%ms% lss 100 set ms=0%ms%

:: Mission accomplished
set /a totalsecs = %hours%*3600 + %mins%*60 + %secs%
echo find a bug at 1 ... >> %f%_pat.%ext%
echo Time elapsed: %totalsecs%.%ms% seconds >> %f%_pat.%ext%


REM PTESTer experiments
@echo Run PTester ...
for /L %%k in (1,1,%BOUND%) do (
	set pstart=%time%
	@echo %pt% /dfs /queue-bound:%%k %f%.dll
	@call %pt% /dfs /queue-bound:%%k %f%.dll > %f%_ptester_%%k.%ext%
	
	set pend=%time%

	set options="tokens=1-4 delims=:.,"
	for /f %options% %%a in ("%pstart%") do set start_h=%%a&set /a start_m=100%%b %% 100&set /a start_s=100%%c %% 100&set /a start_ms=100%%d %% 100
	for /f %options% %%a in ("%pend%") do set end_h=%%a&set /a end_m=100%%b %% 100&set /a end_s=100%%c %% 100&set /a end_ms=100%%d %% 100

	set /a hours=%end_h%-%start_h%
	set /a mins=%end_m%-%start_m%
	set /a secs=%end_s%-%start_s%
	set /a ms=%end_ms%-%start_ms%
	if %ms% lss 0 set /a secs = %secs% - 1 & set /a ms = 100%ms%
	if %secs% lss 0 set /a mins = %mins% - 1 & set /a secs = 60%secs%
	if %mins% lss 0 set /a hours = %hours% - 1 & set /a mins = 60%mins%
	if %hours% lss 0 set /a hours = 24%hours%
	if 1%ms% lss 100 set ms=0%ms%

	:: Mission accomplished
	set /a totalsecs = %hours%*3600 + %mins%*60 + %secs%
	echo find a bug at 1 ... >> %f%_ptester_%%k.%ext%
	echo Time elapsed: %totalsecs%.%ms% seconds >> %f%_ptester_%%k.%ext%
)

goto :eof
:noF
echo please compile P program first
exit /b 1