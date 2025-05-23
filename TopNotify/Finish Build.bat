
cd .\bin\production\x64
signtool sign /f "%USERPROFILE%\Documents\Certificates\SamsidParty Private.pfx" /p %SP_KEY% /fd SHA256 .\TopNotify.exe

cd ..\..\..\

SET F=".\BUILD"
 
IF EXIST %F% RMDIR /S /Q %F%

c:\windows\system32\xcopy.exe .\MSIX .\BUILD /E /H /C /I
c:\windows\system32\xcopy.exe /s .\bin\production\x64 .\BUILD

cd .\BUILD

MakeAppx pack /d .\ /p .\TopNotify.msix
signtool sign /f "%USERPROFILE%\Documents\Certificates\SamsidParty Private.pfx" /p %SP_KEY% /fd SHA256 .\TopNotify.msix