@echo off

set THISDIR=%~dp0
pushd %THISDIR%

echo "current directory is: %THISDIR%"

::set CMP=../cleanup.bat

echo "start cleaning up the compilation outputs of benchmark ..."
for /D %%d in (*) do (
	echo %%d
	cd %%d
	call %THISDIR%/cleanup-generic.bat
	cd ..
)


