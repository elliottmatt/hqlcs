@echo off
fc pdt_%1.txt hqlcs_%1.txt 1> nul 2> nul
if %ERRORLEVEL% equ 0 goto good

sort pdt_%1.txt > pdt_%1.sort.txt
sort hqlcs_%1.txt > hqlcs_%1.sort.txt
fc pdt_%1.sort.txt hqlcs_%1.sort.txt 1> nul 2> nul
if %ERRORLEVEL% equ 0 goto good
echo DIFF
goto end

:good
echo SAME

:end
@echo on