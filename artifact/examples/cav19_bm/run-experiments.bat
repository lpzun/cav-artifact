@echo off

set THISDIR=%~dp0
pushd %THISDIR%
echo echo "current directory is: %THISDIR%"

set PHOME=%THISDIR%..\..
set cp=bin\x64
set pt=%PHOME%\%cp%\Binaries\pt.exe
if not exist "%pt%" goto :no-pt

for /D %%d in (*) do (
	@echo %%d
	cd %%d
	REM for %%f in (*.dll) do (
		REM @echo %%~nf
		REM @echo %PAT% /queue-prefix:%PREFIX% %%~nf.dll
		REM @call %PAT% /queue-prefix:%PREFIX% %%~nf.dll > %%~nf_pat.out
		
		REM @echo %PTESTER% /queue-bound:%BOUND% %%~nf.dll
		REM REM @echo %PTESTER% /queue-bound:%BOUND% %%~nf.dll > %%~nf_ptester.out
	REM )
	@call run.bat %PAT% log
	cd ..
)

goto :eof
:no-pt
echo please specify pt.exe