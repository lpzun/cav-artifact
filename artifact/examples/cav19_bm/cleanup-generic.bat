@echo off

REM thorough cleanup, including dlls

del *.4ml 2> nul
del *.cs  2> nul
del *.pdb 2> nul
del *.out 2> nul
del *.dll *~ 2> nul
del coverge.dgml 2> nul
del coverge.txt 2> nul

del concretes*      2> nul
del abstracts*      2> nul
del abstract_succs* 2> nul
del transitions*    2> nul
del a.txt           2> nul
del unreached.txt   2> nul
del reached*.txt    2> nul
