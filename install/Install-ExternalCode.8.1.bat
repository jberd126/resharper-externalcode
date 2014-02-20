::
:: Install ReSharper external sources plugin.
:: 
@echo off
setlocal enableextensions

:: ReSharper plugins are deployed using package structure:
:: 	 \{Product}\{Version}\[VsVersion]\["plugins"|"settings"|"annotations"]
:: http://confluence.jetbrains.com/display/NETCOM/1.06+Packaging+%28R8%29
set RESHARPER_VERSION=8.1
set RESHARPER_BASEDIR=JetBrains\ReSharper\v%RESHARPER_VERSION%
set RESHARPER_PLUGIN=externalsources

:: Use Visual Studio version in plugin directory, if specified.
if [%1] == [] goto install
if "%1" == "/?" goto show_help
if "%1" == "VS2008" set RESHARPER_VS_VERSION=v9.0
if "%1" == "VS2010" set RESHARPER_VS_VERSION=v10.0
if "%1" == "VS2012" set RESHARPER_VS_VERSION=v11.0
if "%1" == "VS2013" set RESHARPER_VS_VERSION=v12.0

if not defined RESHARPER_VS_VERSION (
	echo Invalid Visual Studio version specified.
	goto show_help
)
set RESHARPER_BASEDIR=%RESHARPER_BASEDIR%\%RESHARPER_VS_VERSION%

:install
set SOURCE_PLUGINDIR=%~dp0%RESHARPER_PLUGIN%.%RESHARPER_VERSION%
set TARGET_PLUGINDIR=%LOCALAPPDATA%\%RESHARPER_BASEDIR%\plugins\%RESHARPER_PLUGIN%

echo Installing "%RESHARPER_PLUGIN%" plugin to "%TARGET_PLUGINDIR%"

if exist "%TARGET_PLUGINDIR%" (
	ECHO del /q /s %TARGET_PLUGINDIR%\*.* 2> NUL
) else (
	mkdir "%TARGET_PLUGINDIR%"
)
ECHO @copy /y "%SOURCE_PLUGINDIR%\*.*" "%TARGET_PLUGINDIR%"

:: Unblock files downloaded from Internet
:: https://github.com/citizenmatt/UnblockZoneIdentifier
pushd "%TARGET_PLUGINDIR%"
for /r %%i in (*) do "%~dp0\UnblockZoneIdentifier.exe" "%%i"
popd

echo Completed.
pause
goto end

:show_help
echo Installs external sources plugin for ReSharper v%RESHARPER_VERSION%.
echo.
echo %0 [vs_version]
echo.
echo   vs_version	Optional Visual Studio version to target.
echo				It can be one of: 2008, 2010, 2012, or 2013
echo.

:end