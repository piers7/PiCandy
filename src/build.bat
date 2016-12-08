@echo off
pushd %~dp0
if not exist .nuget\nuget.exe (
    echo Downloading nuget.exe
    mkdir .nuget 2> nul
    powershell -noninteractive -noprofile -command { wget https://dist.nuget.org/win-x86-commandline/latest/nuget.exe -outfile .nuget\nuget.exe }
    if errorlevel 1 goto error
)
if not exist packages\fake (
    echo Downloading FAKE
    .nuget\nuget.exe install FAKE -outputdirectory packages -excludeversion
)
.\packages\fake\tools\fake.exe build.fsx %*
if errorlevel 1 goto error

goto :eof

:error
echo Exiting with error %errorlevel%
exit /b %errorlevel%