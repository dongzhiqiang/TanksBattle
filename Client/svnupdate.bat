set svn_home=C:/Program Files/TortoiseSVN/bin 
set UnityProject=%~dp0

"%svn_home%"\TortoiseProc.exe/command:update /path:%UnityProject%/Assets /notempfile /closeonend:2

rem if "%1"=="" pause