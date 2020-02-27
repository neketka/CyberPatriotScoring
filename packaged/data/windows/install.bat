@echo off

if not exist install.bat (
    echo Please 'cd' into this folder.
	pause
	exit
)

NET SESSION >nul 2>&1
IF %ERRORLEVEL% EQU 0 (
    ECHO Installing service... 
) ELSE (
    ECHO Please run with administrator.
	PAUSE
	EXIT
)

for %%X in (python.exe) do (set FOUND=%%~$PATH:X)
if not defined FOUND (
	echo Please install python with pip and make sure it is on the PATH
	pause
	exit
)

python -m pip install pywin32
python -m pip install py-notifier
python -m pip install playsound
python -m pip install win10toast

mkdir C:\scoring
robocopy .\scoring C:\scoring
schtasks /create /tn "Scoring Engine Service" /sc onlogon /tr "pythonw.exe C:\scoring\main.py"

echo Set oWS = WScript.CreateObject("WScript.Shell") > CreateShortcut.vbs
echo sLinkFile = "%HOMEDRIVE%%HOMEPATH%\Desktop\Scoring Report.lnk" >> CreateShortcut.vbs
echo Set oLink = oWS.CreateShortcut(sLinkFile) >> CreateShortcut.vbs
echo oLink.IconLocation = "C:\scoring\icon.ico" >> CreateShortcut.vbs
echo oLink.TargetPath = "C:\scoring\report.html" >> CreateShortcut.vbs
echo oLink.Save >> CreateShortcut.vbs
cscript CreateShortcut.vbs
del CreateShortcut.vbs

echo Set oWS = WScript.CreateObject("WScript.Shell") > CreateShortcut.vbs
echo sLinkFile = "%HOMEDRIVE%%HOMEPATH%\Desktop\Readme.lnk" >> CreateShortcut.vbs
echo Set oLink = oWS.CreateShortcut(sLinkFile) >> CreateShortcut.vbs
echo oLink.IconLocation = "C:\scoring\icon.ico" >> CreateShortcut.vbs
echo oLink.TargetPath = "C:\scoring\readme.html" >> CreateShortcut.vbs
echo oLink.Save >> CreateShortcut.vbs
cscript CreateShortcut.vbs
del CreateShortcut.vbs

echo Press any key to reboot...

PAUSE

shutdown.exe /r /t 00