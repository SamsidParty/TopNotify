
SET F=".\BUILD"
 
IF EXIST %F% RMDIR /S /Q %F%

c:\windows\system32\xcopy.exe .\MSIX .\BUILD /E /H /C /I
c:\windows\system32\xcopy.exe /s .\bin\Release\net8.0-windows10.0.17763.0 .\BUILD

cd .\BUILD

powershell -c "Add-AppxPackage -Register ./AppxManifest.xml"

cd ..\