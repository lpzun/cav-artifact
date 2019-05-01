@echo off
::Home directory

set THISDIR=%~dp0
pushd %THISDIR%
echo echo "current directory is: %THISDIR%"

REM set CMP=../build-generic.bat

:: compile all benchmarks
rem echo "compile and generate all benchmarks"
rem for /D %%d in (*) do (
rem 	echo %%d
rem 	cd %%d
rem 	for %%f in (*.p) do ( 
rem 		echo %%~nf 
rem 		call %CMP% %%~nf
rem 	)
rem 	cd ..
rem )

echo "start compiling all benchmarks ..."
echo "it might take few minutes, please be patient ..."
for /D %%d in (*) do (
	@echo %%d
	cd %%d
	@call build.bat
	cd ..
)
