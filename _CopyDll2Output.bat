@echo off

rd /s /q .\Output\Bin\Debug\

xcopy /e /s /f /i /r /y /k .\ShareLib\GF.Core\*.* .\Output\Bin\Debug\

@echo GF done

pause
echo on
