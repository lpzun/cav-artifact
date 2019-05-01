@echo off

:: PTester 
set PTESTER=pt /dfs

:: Pat
set PAT=pt /os-list

:: paramters
set BOUND=5
set PREFIX=5

for /D %%d in (*) do (
	::@echo %%d
	cd %%d
	for %%f in (*.dll) do (
		@echo %%~nf
		@echo %PAT% /queue-prefix:%PREFIX% %%~nf.dll
		@call %PAT% /queue-prefix:%PREFIX% %%~nf.dll > %%~nf_pat.out
		
		@echo %PTESTER% /queue-bound:%BOUND% %%~nf.dll
		@echo %PTESTER% /queue-bound:%BOUND% %%~nf.dll > %%~nf_ptester.out
	)
	cd ..
)