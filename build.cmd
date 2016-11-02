@ECHO OFF
REM see http://joshua.poehls.me/powershell-batch-file-wrapper/

SET SCRIPTNAME=%~d0%~p0%~n0.ps1
SET ARGS=%*
IF [%1] NEQ [] GOTO ESCAPE_ARGS

:POWERSHELL
ECHO %ARGS%
PowerShell.exe -NoProfile -NonInteractive -NoLogo -ExecutionPolicy Unrestricted -Command "& { $ErrorActionPreference = 'Stop'; & '%SCRIPTNAME%' @args; EXIT $LASTEXITCODE }" %ARGS%
EXIT /B %ERRORLEVEL%

:ESCAPE_ARGS
SET ARGS=%ARGS:"=\"%
SET ARGS=%ARGS:`=``%
SET ARGS=%ARGS:'=`'%
SET ARGS=%ARGS:$=`$%
SET ARGS=%ARGS:{=`}%
SET ARGS=%ARGS:}=`}%
SET ARGS=%ARGS:(=`(%
SET ARGS=%ARGS:)=`)%
SET ARGS=%ARGS:,=`,%
SET ARGS=%ARGS:^%=%

GOTO POWERSHELL