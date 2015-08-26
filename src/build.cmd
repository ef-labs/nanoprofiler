@echo Off
SETLOCAL
set Config=%1
set Version=%2

if "%Config%" == "" (
   set Config=Release
)
if "%Version%" == "" (
   set Version=1.3.0.1
)

set EnableNuGetPackageRestore=true 

%WINDIR%\Microsoft.NET\Framework\v4.0.30319\msbuild .\Build\Build.proj /p:Configuration="%Config%" /p:Platform="AnyCPU" /m /v:M /fl /flp:LogFile=msbuild.log;Verbosity=Detailed /nr:false 
ENDLOCAL