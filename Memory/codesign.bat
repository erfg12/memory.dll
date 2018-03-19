@echo off
set /p id="Enter private key file pass: "
"C:\Program Files (x86)\Windows Kits\10\bin\10.0.16299.0\x86\signtool" sign /t http://timestamp.globalsign.com/scripts/timstamp.dll /f "%UserProfile%\Documents\My Private Key.pfx" /p %id% "bin\Debug\Memory.dll"
pause