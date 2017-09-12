call "%~dp0/svnupdate.bat" 1
echo 更新完成!

set UnityExe="D:/Unity52/Editor/Unity.exe"
set UnityProject=%~dp0

%UnityExe% -nographics -batchmode -projectPath %UnityProject% -args %1 -quit -executeMethod BuildUtil.BuildByCommonline
echo 完成!
